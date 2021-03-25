using System;
using System.IO;

namespace IxMilia.Dwg
{
    public struct DwgImageData
    {
        private static byte[] StartSentinel = new byte[]
        {
            0x1F, 0x25, 0x6D, 0x07, 0xD4, 0x36, 0x28, 0x28, 0x9D, 0x57, 0xCA, 0x3F, 0x9D, 0x44, 0x10, 0x2B
        };

        private static byte[] EndSentinel = new byte[]
        {
            0xE0, 0xDA, 0x92, 0xF8, 0x2B, 0xc9, 0xD7, 0xD7, 0x62, 0xA8, 0x35, 0xC0, 0x62, 0xBB, 0xEF, 0xD4
        };

        public byte[] HeaderData { get; set; }
        public byte[] BmpData { get; set; }
        public byte[] WmfData { get; set; }

        public void WriteBitmap(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                WriteBitmap(fs);
            }
        }

        public void WriteBitmap(Stream stream)
        {
            // write the required (but for some reason missing) 14 byte header
            var totalLength = BmpData.Length + 14;
            stream.WriteByte((byte)'B');
            stream.WriteByte((byte)'M');
            var lengthBytes = BitConverter.GetBytes(totalLength);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.WriteByte(0); // reserved
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            var bitOffsetBytes = BitConverter.GetBytes(1078);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bitOffsetBytes);
            }

            stream.Write(bitOffsetBytes, 0, bitOffsetBytes.Length);

            // and the actual data
            stream.Write(BmpData, 0, BmpData.Length);
        }

        internal static DwgImageData Parse(BitReader reader)
        {
            var imageData = new DwgImageData();

            reader.ValidateSentinel(StartSentinel);
            var overallSize = reader.Read_RL();
            var startOffset = reader.Offset;
            var imageCount = reader.Read_RC();

            var headerStart = 0;
            var headerSize = 0;
            var bmpStart = 0;
            var bmpSize = 0;
            var wmfStart = 0;
            var wmfSize = 0;
            for (int i = 0; i < imageCount; i++)
            {
                var code = reader.Read_RC();
                var start = reader.Read_RL();
                var size = reader.Read_RL();

                switch (code)
                {
                    case 1:
                        headerStart = start;
                        headerSize = size;
                        break;
                    case 2:
                        bmpStart = start;
                        bmpSize = size;
                        break;
                    case 3:
                        wmfStart = start;
                        wmfSize = size;
                        break;
                }
            }

            if (headerSize > 0)
            {
                if (reader.Offset != headerStart)
                {
                    throw new DwgReadException("Unexpected image data header start.");
                }

                imageData.HeaderData = reader.ReadBytes(headerSize);
            }

            if (bmpSize > 0)
            {
                if (reader.Offset != bmpStart)
                {
                    throw new DwgReadException("Unexpected BMP data start.");
                }

                imageData.BmpData = reader.ReadBytes(bmpSize);
            }

            if (wmfSize > 0)
            {
                if (reader.Offset != wmfStart)
                {
                    throw new DwgReadException("Unexpected WMF data start.");
                }
                imageData.WmfData = reader.ReadBytes(wmfSize);
            }

            var endOffset = reader.Offset;
            var readBytes = endOffset - startOffset;
            if (readBytes != overallSize)
            {
                throw new DwgReadException($"Invalid count of read image data bytes.  Expected: {overallSize}, Actual: {readBytes}");
            }

            reader.ValidateSentinel(EndSentinel);
            return imageData;
        }

        internal void Write(BitWriter writer, int offsetAdjustment)
        {
            var startOffset = writer.Position;
            writer.WriteBytes(StartSentinel);

            var imageCount = 0;
            if (HeaderData?.Length > 0)
            {
                imageCount++;
            }

            if (BmpData?.Length > 0)
            {
                imageCount++;
            }

            if (WmfData?.Length > 0)
            {
                imageCount++;
            }

            var imageControlByteCount =
                imageCount * (1 + 4 + 4) + // each set of data writes a byte and two ints
                1; // `imageCount` below
            var overallSize =
                imageControlByteCount +
                (HeaderData?.Length ?? 0) +
                (BmpData?.Length ?? 0) +
                (WmfData?.Length ?? 0);
            writer.Write_RL(overallSize);

            writer.Write_RC((byte)imageCount);
            if (HeaderData?.Length > 0)
            {
                writer.Write_RC(1);
                writer.Write_RL(writer.Position - startOffset + offsetAdjustment); // start
                writer.Write_RL(HeaderData.Length);
            }
            if (BmpData?.Length > 0)
            {
                writer.Write_RC(2);
                writer.Write_RL(writer.Position - startOffset + offsetAdjustment); // start
                writer.Write_RL(BmpData.Length);
            }
            if (WmfData?.Length > 0)
            {
                writer.Write_RC(3);
                writer.Write_RL(writer.Position - startOffset + offsetAdjustment); // start
                writer.Write_RL(WmfData.Length);
            }

            writer.WriteBytes(EndSentinel);
        }
    }
}

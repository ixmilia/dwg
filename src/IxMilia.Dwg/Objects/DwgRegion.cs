using System.Collections.Generic;
using System.Diagnostics;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgRegion
    {
        public List<byte> RawData { get; private set; }
        public List<byte> TrailingData { get; private set; }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            WriteSpecificRegionData(writer, RawData, TrailingData, ref _objectSize);
        }

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            ParseSpecificRegionData(reader, objectBitOffsetStart, RawData, TrailingData, _objectSize);
            AssertObjectSize(reader, objectBitOffsetStart);
        }

        internal static void WriteSpecificRegionData(BitWriter writer, List<byte> rawData, List<byte> trailingData, ref int objectSize)
        {
            writer.Write_BS(64); // item type
            writer.Write_BD(1.0); // unknown
            writer.Write_BL(rawData.Count);
            writer.WriteBytes(rawData);
            writer.Write_BL(0); // end of data chunks
            writer.WriteBytes(trailingData);

            objectSize = writer.BitCount;
        }

        internal static void ParseSpecificRegionData(BitReader reader, int objectBitOffsetStart, List<byte> rawData, List<byte> trailingData, int objectSize)
        {
            var itemType = reader.Read_BS();
            if (itemType == 64)
            {
                var _unknown = reader.Read_BD(); // always 1.0?
                var charsInBlock = reader.Read_BL();
                while (charsInBlock != 0)
                {
                    for (int i = 0; i < charsInBlock; i++)
                    {
                        var c = reader.Read_RC();
                        if (c >= 0x20 && c <= 0x7E)
                        {
                            c = (byte)(0x9F - c);
                        }

                        if (c == '\t')
                        {
                            c = (byte)' ';
                        }

                        rawData.Add(c);
                    }

                    charsInBlock = reader.Read_BL();
                }
            }

            var remainingBits = objectSize - (reader.BitOffset - objectBitOffsetStart);
            var remainingBytes = remainingBits / 8;
            var trailingBytes = reader.ReadBytes(remainingBytes);
            trailingData.AddRange(trailingBytes);

            var trailingBitCount = remainingBits % 8;
            if (trailingBitCount != 0)
            {
                var finalByte = (byte)reader.ReadBits(trailingBitCount);
                trailingData.Add(finalByte);
            }
        }
    }
}

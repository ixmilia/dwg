using System;
using System.Text;

namespace IxMilia.Dwg
{
    internal class BitReader
    {
        internal int bitOffset;
        private byte currentByte;

        public byte[] Data { get; private set; }
        public int Offset { get; set; }

        public BitReader(byte[] data, int offset = 0)
        {
            bitOffset = 0;
            currentByte = 0;
            Data = data;
            Offset = offset;
        }

        public BitReader FromOffset(int offset)
        {
            return new BitReader(Data, offset);
        }

        public void AlignByte()
        {
            if (bitOffset != 0)
            {
                bitOffset = 0;
                currentByte = 0;
                Offset++;
            }
        }

        public int ReadBit()
        {
            if (bitOffset == 0)
            {
                // need to read a new byte
                if (Offset >= Data.Length)
                {
                    throw new DwgReadException("Out of data.");
                }

                currentByte = Data[Offset++];
            }

            int result = (currentByte >> (8 - bitOffset - 1)) & 0x01;
            bitOffset++;
            if (bitOffset >= 8)
            {
                bitOffset = 0;
            }

            return result;
        }

        public int ReadBits(int count)
        {
            if (count >= 32)
            {
                throw new DwgReadException("Can't read more than 32 bits at a time.");
            }

            int result = 0;
            for (int i = 0; i < count; i++)
            {
                var bit = ReadBit();
                result = (result << 1) | bit;
            }

            return result;
        }

        public byte ReadByte()
        {
            if (Offset >= Data.Length)
            {
                throw new DwgReadException("Out of data.");
            }

            if (bitOffset == 0)
            {
                return Data[Offset++];
            }
            else
            {
                var temp = (currentByte << bitOffset);
                currentByte = Data[Offset++];
                temp |= (currentByte >> (8 - bitOffset));
                return (byte)temp;
            }
        }

        public byte[] ReadBytes(long count)
        {
            var result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }

        public short ReadShort()
        {
            var bytes = ReadBytes(2);
            return (short)((bytes[1] << 8) + bytes[0]);
        }

        public int ReadInt()
        {
            var bytes = ReadBytes(4);
            return (bytes[3] << 24) + (bytes[2] << 16) + (bytes[1] << 8) + bytes[0];
        }

        public double ReadDouble()
        {
            var bytes = ReadBytes(8);
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 4; i++)
                {
                    var temp = bytes[i];
                    bytes[i] = bytes[7 - i];
                    bytes[7 - i] = temp;
                }
            }

            return BitConverter.ToDouble(bytes, 0);
        }

        public void SkipBytes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ReadByte();
            }
        }

        public string ReadStringAscii(int count)
        {
            var bytes = ReadBytes(count);
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append((char)b);
            }

            return sb.ToString();
        }
    }
}

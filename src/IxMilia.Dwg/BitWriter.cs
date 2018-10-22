using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    internal class BitWriter
    {
        private int bitOffset;
        private byte currentByte;

        public Stream BaseStream { get; }

        public BitWriter(Stream stream)
        {
            bitOffset = 0;
            BaseStream = stream;
        }

        private void FlushCurrentByte()
        {
            BaseStream.WriteByte(currentByte);
            currentByte = 0;
            bitOffset = 0;
        }

        public void Flush()
        {
            FlushCurrentByte();
            BaseStream.Flush();
        }

        public void AlignByte()
        {
            if (bitOffset != 0)
            {
                FlushCurrentByte();
            }
        }

        public BitWriter WriteBit(int value)
        {
            if (bitOffset >= 8)
            {
                // write old byte
                FlushCurrentByte();
            }

            if (value != 0)
            {
                var mask = 1 << (7 - bitOffset);
                currentByte = (byte)(currentByte | mask);
            }
            // else, just progress the bit offset

            bitOffset++;
            return this;
        }

        /// <summary>
        /// Write the left-most <paramref name="count" /> bits of <paramref name="value" />.
        /// </summary>
        public BitWriter WriteBits(int value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var toWrite = (value >> (7 - i)) & 0b1;
                WriteBit(toWrite);
            }

            return this;
        }

        public void WriteByte(byte value)
        {
            WriteBits(value, 8);
        }

        public void WriteBytes(params byte[] values)
        {
            WriteBytes((IEnumerable<byte>)values);
        }

        public void WriteBytes(IEnumerable<byte> values)
        {
            foreach (var value in values)
            {
                WriteByte(value);
            }
        }

        public void WriteShort(short value)
        {
            var lower = value & 0xFF;
            var upper = (value >> 8) & 0xFF;
            WriteBytes((byte)lower, (byte)upper);
        }

        public void WriteInt(int value)
        {
            var a = value & 0xFF;
            var b = (value >> 8) & 0xFF;
            var c = (value >> 16) & 0xFF;
            var d = (value >> 24) & 0xFF;
            WriteBytes((byte)a, (byte)b, (byte)c, (byte)d);
        }

        public void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 4; i++)
                {
                    var temp = bytes[i];
                    bytes[i] = bytes[7 - i];
                    bytes[7 - i] = temp;
                }
            }

            WriteBytes(bytes);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    internal class BitWriter
    {
        private int bitOffset;
        private byte currentByte;
        private int startPosition;

        public Stream BaseStream { get; }
        public ushort CurrentCrcValue { get; private set; }
        public int Position => (int)BaseStream.Position;
        public int BitCount => (Position - startPosition) * 8 + bitOffset;

        public BitWriter(Stream stream)
        {
            bitOffset = 0;
            BaseStream = stream;
            startPosition = Position;
        }

        private void FlushCurrentByte()
        {
            if (bitOffset > 0)
            {
                UpdateCrc(currentByte);
                BaseStream.WriteByte(currentByte);
                currentByte = 0;
                bitOffset = 0;
            }
        }

        private void UpdateCrc(byte value)
        {
            CurrentCrcValue = BitReaderExtensions.ComputeCRC(value, CurrentCrcValue);
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

        public void CopyTo(BitWriter other)
        {
            var bits = BitCount;
            var bytes = this.AsBytes();
            var spareBits = bits % 8;
            var fullBytesCount = spareBits == 0 ? bytes.Length : bytes.Length - 1;
            for (int i = 0; i < fullBytesCount; i++)
            {
                other.WriteByte(bytes[i]);
            }

            other.WriteBits(bytes[bytes.Length - 1], spareBits);
        }

        public BitWriter WriteBit(int value)
        {
            if (value != 0)
            {
                var mask = 1 << (7 - bitOffset);
                currentByte = (byte)(currentByte | mask);
            }
            // else, just progress the bit offset

            bitOffset++;
            if (bitOffset >= 8)
            {
                // write the byte
                FlushCurrentByte();
            }

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
            if (bitOffset == 0)
            {
                BaseStream.WriteByte(value);
                UpdateCrc(value);
            }
            else
            {
                currentByte |= (byte)(value >> bitOffset);
                BaseStream.WriteByte(currentByte);
                UpdateCrc(currentByte);
                currentByte = (byte)(value << (8 - bitOffset));
            }
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

        public void WriteShortBigEndian(short value)
        {
            var lower = (value >> 8) & 0xFF;
            var upper = value & 0xFF;
            WriteBytes((byte)lower, (byte)upper);
        }

        public void WriteInt(int value)
        {
            var bytes = BitWriterExtensions.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteUInt(uint value)
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
                Array.Reverse(bytes);
            }

            WriteBytes(bytes);
        }

        public void StartCrcCalculation(ushort initialValue = 0)
        {
            CurrentCrcValue = initialValue;
        }

        public void WriteCrc(ushort xorValue = 0, bool writeCrcAsMsb = false)
        {
            AlignByte();
            var toWrite = (short)(ushort)(CurrentCrcValue ^ xorValue);
            if (writeCrcAsMsb)
            {
                WriteShortBigEndian(toWrite);
            }
            else
            {
                WriteShort(toWrite);
            }
        }

        public static void WriteByteAtPosition(byte[] data, byte value, int bitLocation)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (bitLocation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bitLocation), "Location must not be negative.");
            }

            var byteOffset = bitLocation / 8;
            var bitOffset = bitLocation % 8;

            if (bitOffset == 0)
            {
                // byte-aligned insertion
                if (byteOffset > data.Length - 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(bitLocation), "Not enough bytes for insertion");
                }

                data[byteOffset] = value;
            }
            else
            {
                // arbitrary insertion
                if (byteOffset > data.Length - 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(bitLocation), "Not enough bytes for insertion");
                }

                var keepMask = 0b11111111 << bitOffset;
                data[byteOffset] = (byte)((data[byteOffset] & keepMask) | (value >> 8 - bitOffset));
                data[byteOffset + 1] = (byte)(data[byteOffset + 1] & (~keepMask) | (value << bitOffset));
            }
        }

        public static void WriteRLAtPosition(byte[] data, int value, int bitLocation)
        {
            var bytes = BitWriterExtensions.GetBytes(value);
            WriteByteAtPosition(data, bytes[0], bitLocation);
            WriteByteAtPosition(data, bytes[1], bitLocation + 8);
            WriteByteAtPosition(data, bytes[2], bitLocation + 16);
            WriteByteAtPosition(data, bytes[3], bitLocation + 24);
        }
    }
}

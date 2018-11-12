using System;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class BitReaderTests
    {
        private static BitReader Bits(params int[] data)
        {
            var bytes = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < 0 || data[i] > 255)
                {
                    // the int data type is only a convenience
                    throw new NotSupportedException("Byte values must be between [0, 255].");
                }

                bytes[i] = (byte)data[i];
            }

            return new BitReader(bytes);
        }

        [Fact]
        public void BitProgression()
        {
            var reader = Bits(0b11001101);
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Throws<DwgReadException>(() => reader.ReadBit());
        }

        [Fact]
        public void ByteProgression()
        {
            var reader = Bits(0b00000000, 0b11111111);
            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(0, reader.ReadBit());
            }

            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(1, reader.ReadBit());
            }

            Assert.Throws<DwgReadException>(() => reader.ReadBit());
        }

        [Fact]
        public void ByteProgressionOffBoundary()
        {
            var reader = Bits(0b11000001, 0b10111111);
            Assert.Equal(3, reader.Read_BB());
            Assert.Equal(6, reader.ReadByte());
            Assert.Equal(0b00111111, reader.ReadBits(6));
        }

        [Fact]
        public void AlignToByte()
        {
            // already on a byte boundary
            var reader = Bits(15, 16);
            Assert.Equal(15, reader.ReadByte());
            reader.AlignToByte();
            Assert.Equal(16, reader.ReadByte());
            Assert.Throws<DwgReadException>(() => reader.ReadBit());

            // read to a byte boundary, align is noop
            reader = Bits(0b0, 15, 16);
            for (int i = 0; i < 8; i++)
            {
                reader.ReadBit();
            }
            Assert.Equal(15, reader.ReadByte());
            reader.AlignToByte();
            Assert.Equal(16, reader.ReadByte());
            Assert.Throws<DwgReadException>(() => reader.ReadBit());

            // not on a byte boundary
            reader = Bits(0b11000000, 0b11111111);
            Assert.Equal(3, reader.Read_BB());
            reader.AlignToByte();
            Assert.Equal(255, reader.ReadByte());
            Assert.Throws<DwgReadException>(() => reader.ReadBit());
        }

        [Fact]
        public void ReadBits()
        {
            Assert.Equal(0b0001, Bits(0b11110000).ReadBits(1));
            Assert.Equal(0b0011, Bits(0b11110000).ReadBits(2));
            Assert.Equal(0b0111, Bits(0b11110000).ReadBits(3));
            Assert.Equal(0b1111, Bits(0b11110000).ReadBits(4));
        }

        [Fact]
        public void Read_BS()
        {
            Assert.Equal(342, Bits(0b00010101, 0b10000000, 0b01000000).Read_BS());
            //                       ||AAAAAA    AABBBBBB    BB------

            Assert.Equal(42, Bits(0b01001010, 0b10000000).Read_BS());
            //                      ||AAAAAA    AA------

            Assert.Equal(0, Bits(0b10000000).Read_BS());
            //                     ||------

            Assert.Equal(256, Bits(0b11000000).Read_BS());
            //                       ||------
        }

        [Fact]
        public void Read_BL()
        {
            Assert.Equal(257, Bits(0b00000000, 0b01000000, 0b01000000, 0b00000000, 0b00000000).Read_BL());
            //                       ||AAAAAA    AABBBBBB    BBCCCCCC    CCDDDDDD    DD------

            Assert.Equal(0, Bits(0b10000000).Read_BL());
            //                     ||------

            Assert.Equal(15, Bits(0b01000011, 0b11000000).Read_BL());
            //                      ||AAAAAA    AA------
        }

        [Fact]
        public void Read_BD()
        {
            Assert.Equal(23.0, Bits(0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00001101, 0b11010000, 0b00000000).Read_BD());
            //                        ||ssssss    ssssssss    ssssssss    ssssssss    ssssssss    ssssssss    ssssssee    eeeeeeee    e^------

            Assert.Equal(1.0, Bits(0b01000000).Read_BD());
            //                       ||------

            Assert.Equal(0.0, Bits(0b10000000).Read_BD());
            //                       ||------
        }

        [Fact]
        public void Read_MC()
        {
            Assert.Equal(4610, Bits(0b10000010, 0b00100100).Read_MC());
            Assert.Equal(112823273, Bits(0b11101001, 0b10010111, 0b11100110, 0b00110101).Read_MC());
            Assert.Equal(-1413, Bits(0b10000101, 0b01001011).Read_MC());
            Assert.Equal(64, Bits(0b11000000, 0b00000000).Read_MC());
            Assert.Equal(0, Bits(0b00000000).Read_MC());
        }

        [Fact]
        public void Read_MS()
        {
            Assert.Equal(4650033, Bits(0b00110001, 0b11110100, 0b10001101, 0b00000000).Read_MS());
        }

        [Fact]
        public void Read_H()
        {
            Assert.Equal(new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, 0x05E7), Bits(0b01010010, 0b00000101, 0b11100111).Read_H());
        }

        [Fact]
        public void ComputeCRC()
        {
            var data = new byte[] { 0b01000000, 0b11100000 };
            var expected = (ushort)0b01010101_01010101;
            var initialValue = (ushort)0x2C8C;
            var actual = BitReaderExtensions.ComputeCRC(data, 0, data.Length, initialValue);
            Assert.Equal(expected, actual);
        }
    }
}

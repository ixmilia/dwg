using System;
using System.IO;
using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class BitWriterTests
    {
        private static BitWriter Writer()
        {
            var stream = new MemoryStream();
            return new BitWriter(stream);
        }

        private static byte[] Bytes(params int[] values)
        {
            var bytes = new byte[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < 0 || values[i] > 255)
                {
                    // the int data type is only a convenience
                    throw new NotSupportedException("Byte values must be between [0, 255].");
                }

                bytes[i] = (byte)values[i];
            }

            return bytes;
        }

        [Fact]
        public void BitProgression()
        {
            var writer = Writer();
            writer.WriteBit(1);
            writer.WriteBit(1);
            writer.WriteBit(0);
            writer.WriteBit(0);
            writer.WriteBit(1);
            writer.WriteBit(1);
            writer.WriteBit(0);
            writer.WriteBit(1);
            Assert.Equal(Bytes(0b11001101), writer.AsBytes());
        }

        [Fact]
        public void ByteProgression()
        {
            var writer = Writer();
            for (int i = 0; i < 8; i++)
            {
                writer.WriteBit(0);
            }

            for (int i = 0; i < 8; i++)
            {
                writer.WriteBit(1);
            }

            Assert.Equal(Bytes(0b00000000, 0b11111111), writer.AsBytes());
        }

        [Fact]
        public void WriteByteNotOnBoundary()
        {
            var writer = Writer();
            writer.WriteBit(1); // A
            writer.WriteBit(1); // A
            writer.WriteByte(0b00001111); // B
            Assert.Equal(Bytes(0b11_000011, 0b11_000000), writer.AsBytes());
            //                   AA BBBBBB    BB ------
        }

        [Fact]
        public void WriteBits()
        {
            Assert.Equal(Bytes(0b10000000), Writer().WriteBits(0b11110000, 1).AsBytes());
            Assert.Equal(Bytes(0b11000000), Writer().WriteBits(0b11110000, 2).AsBytes());
            Assert.Equal(Bytes(0b11100000), Writer().WriteBits(0b11110000, 3).AsBytes());
            Assert.Equal(Bytes(0b11110000), Writer().WriteBits(0b11110000, 4).AsBytes());
            Assert.Equal(Bytes(0b11110000), Writer().WriteBits(0b11110000, 5).AsBytes());
            Assert.Equal(Bytes(0b11110000), Writer().WriteBits(0b11110000, 6).AsBytes());
            Assert.Equal(Bytes(0b11110000), Writer().WriteBits(0b11110000, 7).AsBytes());
            Assert.Equal(Bytes(0b11110000), Writer().WriteBits(0b11110000, 8).AsBytes());
        }

        [Fact]
        public void Write_BS()
        {
            Assert.Equal(Bytes(0b00010101, 0b10000000, 0b01000000), Writer().Write_BS(342).AsBytes());
            //                   ||AAAAAA    AABBBBBB    BB------

            Assert.Equal(Bytes(0b01001010, 0b10000000), Writer().Write_BS(42).AsBytes());
            //                   ||AAAAAA    AA------

            Assert.Equal(Bytes(0b10000000), Writer().Write_BS(0).AsBytes());
            //                   ||------

            Assert.Equal(Bytes(0b11000000), Writer().Write_BS(256).AsBytes());
            //                   ||------
        }

        [Fact]
        public void Write_BL()
        {
            Assert.Equal(Bytes(0b00000000, 0b01000000, 0b01000000, 0b00000000, 0b00000000), Writer().Write_BL(257).AsBytes());
            //                   ||AAAAAA    AABBBBBB    BBCCCCCC    CCDDDDDD    DD------

            Assert.Equal(Bytes(0b10000000), Writer().Write_BL(0).AsBytes());
            //                   ||------

            Assert.Equal(Bytes(0b01000011, 0b11000000), Writer().Write_BL(15).AsBytes());
            //                   ||AAAAAA    AA------
        }

        [Fact]
        public void Write_BD()
        {
            Assert.Equal(Bytes(0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00001101, 0b11010000, 0b00000000), Writer().Write_BD(23.0).AsBytes());
            //                   ||ssssss    ssssssss    ssssssss    ssssssss    ssssssss    ssssssss    ssssssee    eeeeeeee    e^------

            Assert.Equal(Bytes(0b01000000), Writer().Write_BD(1.0).AsBytes());
            //                       ||------

            Assert.Equal(Bytes(0b10000000), Writer().Write_BD(0.0).AsBytes());
            //                   ||------
        }

        [Fact]
        public void Write_MC()
        {
            Assert.Equal(Bytes(0b10000010, 0b00100100), Writer().Write_MC(4610).AsBytes());
            Assert.Equal(Bytes(0b11101001, 0b10010111, 0b11100110, 0b00110101), Writer().Write_MC(112823273).AsBytes());
            Assert.Equal(Bytes(0b10000101, 0b01001011), Writer().Write_MC(-1413).AsBytes());
            Assert.Equal(Bytes(0b11000000, 0b00000000), Writer().Write_MC(64).AsBytes());
            Assert.Equal(Bytes(0b00000000), Writer().Write_MC(0).AsBytes());
        }

        [Fact]
        public void Write_MS()
        {
            Assert.Equal(Bytes(0b00110001, 0b11110100, 0b10001101, 0b00000000), Writer().Write_MS(4650033).AsBytes());
        }

        [Fact]
        public void Write_H()
        {
            Assert.Equal(Bytes(0b01010010, 0b00000101, 0b11100111), Writer().Write_H(new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, 0x05E7)).AsBytes());
        }

        [Fact]
        public void WriteCRC()
        {
            var writer = Writer();
            writer.StartCrcCalculation(0x2C8C);
            writer.Write_BS(3);
            writer.WriteBits(0b10000_000, 5);
            writer.AlignByte();
            Assert.Equal(0x5555, writer.CurrentCrcValue);
        }

        [Theory]
        [InlineData(12, new int[] { 0b00001100 })]
        [InlineData(540, new int[] { 0b00000010, 0b00011100 })]
        public void WriteSecondHeaderHandle(int handleValue, int[] expectedBits)
        {
            var id = 0;
            var writer = Writer();
            var handle = new DwgHandleReference(0, handleValue);
            handle.WriteSecondHeader(writer, id);

            var expectedPrefix = new int[] { expectedBits.Length, id };
            var realExpectedBits = expectedPrefix.Concat(expectedBits).Select(b => (byte)b).ToArray();
            Assert.Equal(realExpectedBits, writer.AsBytes());
        }
    }
}

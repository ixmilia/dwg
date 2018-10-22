using System;
using System.IO;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class BitRoundTripTests
    {
        private static void RoundTrip<T>(Action<BitWriter> writerAction, Func<BitReader, T> readerAction, T expected)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BitWriter(ms);
                writerAction(writer);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                var buffer = new byte[ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                var reader = new BitReader(buffer);
                var actual = readerAction(reader);

                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(256)]
        [InlineData(342)]
        public void RoundTrip_BS(short value)
        {
            RoundTrip(writer => writer.Write_BS(value), reader => reader.Read_BS(), value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(257)]
        public void RoundTrip_BL(int value)
        {
            RoundTrip(writer => writer.Write_BL(value), reader => reader.Read_BL(), value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(23.0)]
        public void RoundTrip_BD(double value)
        {
            RoundTrip(writer => writer.Write_BD(value), reader => reader.Read_BD(), value);
        }

        [Theory]
        [InlineData(4610)]
        [InlineData(112823273)]
        [InlineData(-1413)]
        public void RoundTrip_MC(int value)
        {
            RoundTrip(writer => writer.Write_MC(value), reader => reader.Read_MC(), value);
        }

        [Fact]
        public void RoundTrip_MS()
        {
            var value = 4650033;
            RoundTrip(writer => writer.Write_MS(value), reader => reader.Read_MS(), value);
        }

        [Fact]
        public void RoundTrip_H()
        {
            var value = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, 0x05E7);
            RoundTrip(writer => writer.Write_H(value), reader => reader.Read_H(), value);
        }
    }
}

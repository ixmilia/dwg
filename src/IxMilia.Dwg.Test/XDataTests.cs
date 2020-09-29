using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class XDataTests : AbstractReaderTests
    {
        protected static T Parse<T>(params int[] data) where T : DwgXDataItem
        {
            var reader = Bits(data);
            var item = (T)DwgXDataItem.Parse(reader);
            Assert.Equal(0, reader.RemainingBytes);
            return item;
        }

        protected static void RoundTrip<T>(T item) where T : DwgXDataItem
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BitWriter(ms);
                item.Write(writer);
                var data = writer.AsBytes();

                var reader = new BitReader(data);
                var roundTripped = DwgXDataItem.Parse(reader);
                Assert.Equal(item, roundTripped, new DwgXDataItemComparer());
            }
        }

        [Fact]
        public void ParseString()
        {
            var s = Parse<DwgXDataString>(
                0,                                              // type code
                4,                                              // length
                0, 30,                                          // code page
                'a', 'b', 'c', 'd'                              // data
            );
            Assert.Equal(30, s.CodePage);
            Assert.Equal("abcd", s.Value);
        }

        [Fact]
        public void RoundTripString()
        {
            RoundTrip(new DwgXDataString(30, "abcd"));
        }

        [Fact]
        public void ParseList()
        {
            var l = Parse<DwgXDataItemList>(
                2,                                              // type code
                0,                                              // opening marker
                    70,                                         // type code
                    4, 0,                                       // data
                    70,                                         // type code
                    5, 0,                                       // data
                2,                                              // type code
                1                                               // closing marker
            );
            Assert.Equal(2, l.Count);
            Assert.Equal(4, ((DwgXDataShort)l[0]).Value);
            Assert.Equal(5, ((DwgXDataShort)l[1]).Value);
        }

        [Fact]
        public void RoundTripList()
        {
            RoundTrip(new DwgXDataItemList()
            {
                new DwgXDataShort(4),
                new DwgXDataShort(5),
            });
        }

        [Fact]
        public void ParseLayerReference()
        {
            var l = Parse<DwgXDataLayerReference>(
                3,                                              // type code
                '0', '0', '0', '0', '0', '0', 'A', 'F'          // data
            );
            Assert.Equal(0xAF, l.Handle);
        }

        [Fact]
        public void RoundTripLayerReference()
        {
            RoundTrip(new DwgXDataLayerReference(0xAF));
        }

        [Fact]
        public void ParseBinary()
        {
            var b = Parse<DwgXDataBinaryChunk>(
                4,                                              // type code
                3,                                              // length
                1, 2, 3                                         // data
            );
            var expected = new byte[] { 1, 2, 3 };
            Assert.Equal(expected, b.Data);
        }

        [Fact]
        public void RoundTripBinary()
        {
            RoundTrip(new DwgXDataBinaryChunk(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public void ParseEntityReference()
        {
            var e = Parse<DwgXDataEntityReference>(
                5,                                              // type code
                '0', '0', '0', '0', '0', '0', 'A', 'F'          // data
            );
            Assert.Equal(0xAF, e.Handle);
        }

        [Fact]
        public void RoundTripEntityReference()
        {
            RoundTrip(new DwgXDataEntityReference(0xAF));
        }

        [Fact]
        public void ParseRealTriple()
        {
            var t = Parse<DwgXDataRealTriple>(
                10,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, // data
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x40
            );
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), t.Value);
        }

        [Fact]
        public void RoundTripRealTriple()
        {
            RoundTrip(new DwgXDataRealTriple(new DwgPoint(1.0, 2.0, 3.0)));
        }

        [Fact]
        public void ParseWorldSpacePosition()
        {
            var p = Parse<DwgXDataWorldSpacePosition>(
                11,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, // data
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x40
            );
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), p.Value);
        }

        [Fact]
        public void RoundTripWorldSpacePosition()
        {
            RoundTrip(new DwgXDataWorldSpacePosition(new DwgPoint(1.0, 2.0, 3.0)));
        }

        [Fact]
        public void ParseWorldSpaceDisplacement()
        {
            var d = Parse<DwgXDataWorldSpaceDisplacement>(
                12,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, // data
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x40
            );
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), d.Value);
        }

        [Fact]
        public void RoundTripWorldSpaceDisplacement()
        {
            RoundTrip(new DwgXDataWorldSpaceDisplacement(new DwgPoint(1.0, 2.0, 3.0)));
        }

        [Fact]
        public void ParseWorldDirection()
        {
            var d = Parse<DwgXDataWorldDirection>(
                13,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, // data
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x40
            );
            Assert.Equal(new DwgVector(1.0, 2.0, 3.0), d.Value);
        }

        [Fact]
        public void RoundTripWorldDirection()
        {
            RoundTrip(new DwgXDataWorldDirection(new DwgVector(1.0, 2.0, 3.0)));
        }

        [Fact]
        public void ParseReal()
        {
            var r = Parse<DwgXDataReal>(
                40,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F  // data
            );
            Assert.Equal(1.0, r.Value);
        }

        [Fact]
        public void RoundTripReal()
        {
            RoundTrip(new DwgXDataReal(1.0));
        }

        [Fact]
        public void ParseDistance()
        {
            var d = Parse<DwgXDataDistance>(
                41,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F  // data
            );
            Assert.Equal(1.0, d.Value);
        }

        [Fact]
        public void RoundTripDistance()
        {
            RoundTrip(new DwgXDataDistance(1.0));
        }

        [Fact]
        public void ParseScaleFactor()
        {
            var s = Parse<DwgXDataScaleFactor>(
                42,                                             // type code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F  // data
            );
            Assert.Equal(1.0, s.Value);
        }

        [Fact]
        public void RoundTripScaelFactor()
        {
            RoundTrip(new DwgXDataScaleFactor(1.0));
        }

        [Fact]
        public void ParseShort()
        {
            var s = Parse<DwgXDataShort>(
                70,                                             // type code
                143, 0                                          // data
            );
            Assert.Equal(143, s.Value);
        }

        [Fact]
        public void RoundTripShort()
        {
            RoundTrip(new DwgXDataShort(143));
        }

        [Fact]
        public void ParseLong()
        {
            var l = Parse<DwgXDataLong>(
                71,                                             // type code
                143, 0, 0, 0                                    // data
            );
            Assert.Equal(143, l.Value);
        }

        [Fact]
        public void RoundTripLong()
        {
            RoundTrip(new DwgXDataLong(143));
        }

        #region xdata item comparer
        private class DwgXDataItemComparer : IEqualityComparer<DwgXDataItem>
        {
            public bool Equals(DwgXDataItem x, DwgXDataItem y)
            {
                return x switch
                {
                    null when y is null => true, // (null, null)
                    _ when y is null => false, // ({ }, null)
                    null => false, // (null, { })
                    DwgXDataString s1 when y is DwgXDataString s2 => s1.CodePage == s2.CodePage && s1.Value == s2.Value,
                    DwgXDataItemList l1 when y is DwgXDataItemList l2 => l1.Count == l2.Count && l1.Zip(l2).All(pair => Equals(pair.First, pair.Second)),
                    DwgXDataLayerReference l1 when y is DwgXDataLayerReference l2 => l1.Handle == l2.Handle,
                    DwgXDataBinaryChunk b1 when y is DwgXDataBinaryChunk b2 => b1.Data.Length == b2.Data.Length && b1.Data.Zip(b2.Data).All(pair => pair.First == pair.Second),
                    DwgXDataEntityReference e1 when y is DwgXDataEntityReference e2 => e1.Handle == e2.Handle,
                    DwgXDataRealTriple t1 when y is DwgXDataRealTriple t2 => t1.Value == t2.Value,
                    DwgXDataWorldSpacePosition p1 when y is DwgXDataWorldSpacePosition p2 => p1.Value == p2.Value,
                    DwgXDataWorldSpaceDisplacement d1 when y is DwgXDataWorldSpaceDisplacement d2 => d1.Value == d2.Value,
                    DwgXDataWorldDirection d1 when y is DwgXDataWorldDirection d2 => d1.Value == d2.Value,
                    DwgXDataReal r1 when y is DwgXDataReal r2 => r1.Value == r2.Value,
                    DwgXDataDistance d1 when y is DwgXDataDistance d2 => d1.Value == d2.Value,
                    DwgXDataScaleFactor s1 when y is DwgXDataScaleFactor s2 => s1.Value == s2.Value,
                    DwgXDataShort s1 when y is DwgXDataShort s2 => s1.Value == s2.Value,
                    DwgXDataLong l1 when y is DwgXDataLong l2 => l1.Value == l2.Value,
                    _ => false,
                };
            }

            public int GetHashCode(DwgXDataItem obj)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    /// <summary>
    /// All tests in this class write raw bits (e.g., no pointers are bound) using as inspiration the examples given in the PDF spec.
    /// </summary>
    public class RawObjectWriteTests : AbstractReaderTests
    {
        private static IDictionary<string, short> ClassMap()
        {
            return new Dictionary<string, short>()
            {
                { "DICTIONARYVAR", 505 }
            };
        }

        protected static void WriteRaw(DwgObject obj, params int[] expected)
        {
            var expectedBytes = new byte[expected.Length];
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] < 0 || expected[i] > 255)
                {
                    // the int data type is only a convenience
                    throw new NotSupportedException("Byte values must be between [0, 255].");
                }

                expectedBytes[i] = (byte)expected[i];
            }

            using (var ms = new MemoryStream())
            {
                var writer = new BitWriter(ms);
                var classMap = ClassMap();
                var appIdMap = new Dictionary<string, DwgHandle>();
                obj.PrepareCommonValues();
                obj.WriteCoreRaw(writer, DwgVersionId.R14, classMap, appIdMap);
                var actual = writer.AsBytes();
                Assert.Equal(expectedBytes, actual);
            }
        }

        [Fact]
        public void WriteRawDictionaryVar()
        {
            var d = new DwgDictionaryVar();
            d.Handle = new DwgHandle(0x01EA);
            d.IntVal = 0;
            d.Str = "3";
            d._parentHandleReference = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0x00);
            d._reactorHandleReferences.Add(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0xA2));
            WriteRaw(d,
                0x12, 0x00,                                     // length
                0x3E, 0x40, 0x40, 0x80, 0x7A, 0xA7, 0x00, 0x00, // data
                0x00, 0x04, 0x04, 0x01, 0x01, 0x33, 0x40, 0x41,
                0xA2, 0x30,
                0xAC, 0xDA                                      // crc
            );
        }
    }
}

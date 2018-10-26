using System.IO;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileHeaderTests
    {
        [Fact]
        public void ParseSimpleFileHeader()
        {
            var data = new byte[]
            {
                (byte)'A', (byte)'C', (byte)'1', (byte)'0', (byte)'1', (byte)'4',
                0, 0, 0, 0, 0,
                42, // maintver
                1,
                0, 0, 0, 0, // image seeker
                0, 0, // unknown
                0x00, 0x01, // codepage
                0, 0, 0, 0, // no section locators
                0xF8, 0x63, // CRC
                0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5, // sentinel
                0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00,
            };
            var reader = new BitReader(data);
            var fileHeader = DwgFileHeader.Parse(reader);
            Assert.Equal(DwgVersionId.R14, fileHeader.Version);
            Assert.Equal(42, fileHeader.MaintenenceVersion);
            Assert.Equal(256, fileHeader.CodePage);
        }

        [Fact]
        public void WriteSimpleFileHeader()
        {
            var expected = new byte[]
            {
                (byte)'A', (byte)'C', (byte)'1', (byte)'0', (byte)'1', (byte)'4',
                0, 0, 0, 0, 0,
                42, // maintver
                1,
                0x78, 0x56, 0x34, 0x12, // image seeker
                0, 0, // unknown
                0x00, 0x01, // codepage
                6, 0, 0, 0, // 6 section locators
                0, 0, 0, 0, 0, 0, 0, 0, 0, // header locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // class section locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // object map locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // unknown 1 locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // unknown 2 locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // unknown 3 locator
                0x77, 0x4A, // CRC
                0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5, // sentinel
                0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00,
            };

            var fileHeader = new DwgFileHeader(DwgVersionId.R14, 42, 0x12345678, 256);
            using (var ms = new MemoryStream())
            {
                var writer = new BitWriter(ms);
                fileHeader.Write(writer);
                var actual = writer.AsBytes();
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < System.Math.Min(actual.Length, expected.Length); i++)
                {
                    sb.AppendLine($"{i}: {expected[i]:X} {actual[i]:X}");
                }
                //throw new System.Exception(sb.ToString());
                Assert.Equal(expected, actual);
            }
        }
    }
}

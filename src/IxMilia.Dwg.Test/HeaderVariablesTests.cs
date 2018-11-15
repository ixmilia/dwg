using System.IO;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class HeaderVariablesTests
    {
        internal static DwgVersionId[] AllVersions = new DwgVersionId[] { DwgVersionId.R13, DwgVersionId.R14 };

        [Fact]
        public void NextAvailableHandle()
        {
            var drawing = new DwgDrawing();
            Assert.True(drawing.Variables.NextAvailableHandle.IsEmpty);
            using (var ms = new MemoryStream())
            {
                drawing.Save(ms);
            }

            Assert.False(drawing.Variables.NextAvailableHandle.IsEmpty);
        }

        [Fact]
        public void RoundTrip()
        {
            foreach (var version in AllVersions)
            {
                using (var ms = new MemoryStream())
                {
                    // write it
                    var writer = new BitWriter(ms);
                    var variables = new DwgHeaderVariables();
                    variables.Write(writer, version);

                    // read it
                    var reader = new BitReader(writer.AsBytes());
                    var variables2 = DwgHeaderVariables.Parse(reader, version);

                    // this also verifies the CRC
                }
            }
        }
    }
}

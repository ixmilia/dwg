using System.IO;
using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileLayoutTests
    {
        [Fact]
        public void RoundTripDefaultFile()
        {
            using (var ms = new MemoryStream())
            {
                // write it
                var defaultFile = new DwgDrawing();
                Assert.Equal("0", defaultFile.Layers.Single().Value.Name);
                Assert.Equal("STANDARD", defaultFile.Styles.Single().Value.Name);
                Assert.Equal("CONTINUOUS", defaultFile.LineTypes.Single().Value.Name);
                Assert.True(ReferenceEquals(defaultFile.Layers["0"].LineType, defaultFile.LineTypes["CONTINUOUS"]));
                defaultFile.Save(ms);

                // rewind and load
                ms.Seek(0, SeekOrigin.Begin);
                var roundTrippedFile = DwgDrawing.Load(ms);
                Assert.Equal("0", roundTrippedFile.Layers.Single().Value.Name);
                Assert.Equal("STANDARD", roundTrippedFile.Styles.Single().Value.Name);
                Assert.Equal("CONTINUOUS", roundTrippedFile.LineTypes.Single().Value.Name);
                Assert.True(ReferenceEquals(roundTrippedFile.Layers["0"].LineType, roundTrippedFile.LineTypes["CONTINUOUS"]));
            }
        }
    }
}

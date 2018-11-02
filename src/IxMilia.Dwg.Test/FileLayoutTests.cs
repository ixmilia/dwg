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
                Assert.Equal("0", defaultFile.Layers.Single().Name);
                Assert.Equal("STANDARD", defaultFile.Styles.Single().Name);
                defaultFile.Save(ms);

                // rewind and load
                ms.Seek(0, SeekOrigin.Begin);
                var roundTrippedFile = DwgDrawing.Load(ms);
                Assert.Equal("0", roundTrippedFile.Layers.Single().Name);
                Assert.Equal("STANDARD", roundTrippedFile.Styles.Single().Name);
            }
        }
    }
}

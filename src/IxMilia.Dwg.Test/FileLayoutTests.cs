using System.IO;
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
                defaultFile.Save(ms);

                // rewind and load
                ms.Seek(0, SeekOrigin.Begin);
                var roundTrippedFile = DwgDrawing.Load(ms);
            }
        }
    }
}

using System.IO;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class HeaderVariablesTests
    {
        [Fact]
        public void ReadEntireFile()
        {
            var file = DwgDrawing.Load(Path.Combine("Drawings", "R14.dwg"));
        }
    }
}

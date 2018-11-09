using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileLayoutTests : TestBase
    {
        [Fact]
        public void RoundTripDefaultFile()
        {
            // write it
            var defaultFile = new DwgDrawing();
            Assert.Equal("0", defaultFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", defaultFile.Styles.Single().Value.Name);
            Assert.Equal("CONTINUOUS", defaultFile.LineTypes.Single().Value.Name);
            Assert.Equal("*ACTIVE", defaultFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(defaultFile.Layers["0"].LineType, defaultFile.LineTypes["CONTINUOUS"]));

            var roundTrippedFile = RoundTrip(defaultFile);
            Assert.Equal("0", roundTrippedFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", roundTrippedFile.Styles.Single().Value.Name);
            Assert.Equal("CONTINUOUS", roundTrippedFile.LineTypes.Single().Value.Name);
            Assert.Equal("*ACTIVE", roundTrippedFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(roundTrippedFile.Layers["0"].LineType, roundTrippedFile.LineTypes["CONTINUOUS"]));
        }
    }
}

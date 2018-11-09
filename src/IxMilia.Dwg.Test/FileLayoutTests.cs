using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileLayoutTests : TestBase
    {
        [Fact]
        public void RoundTripDefaultFile()
        {
            // validate default values
            var defaultFile = new DwgDrawing();
            Assert.Equal("0", defaultFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", defaultFile.Styles.Single().Value.Name);
            Assert.Equal("CONTINUOUS", defaultFile.LineTypes.Single().Value.Name);
            Assert.Equal("*ACTIVE", defaultFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(defaultFile.Layers["0"].LineType, defaultFile.LineTypes["CONTINUOUS"]));
            Assert.Equal(new[] { "ACAD", "ACAD_MLEADERVER" }, defaultFile.AppIds.Values.Select(a => a.Name));

            // valiate round-trip
            var roundTrippedFile = RoundTrip(defaultFile);
            Assert.Equal("0", roundTrippedFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", roundTrippedFile.Styles.Single().Value.Name);
            Assert.Equal("CONTINUOUS", roundTrippedFile.LineTypes.Single().Value.Name);
            Assert.Equal("*ACTIVE", roundTrippedFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(roundTrippedFile.Layers["0"].LineType, roundTrippedFile.LineTypes["CONTINUOUS"]));
            Assert.Equal(new[] { "ACAD", "ACAD_MLEADERVER" }, roundTrippedFile.AppIds.Values.Select(a => a.Name));
        }
    }
}

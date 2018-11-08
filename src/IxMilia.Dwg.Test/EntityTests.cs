using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class EntityTests : TestBase
    {
        [Fact]
        public void RoundTripLayerWithEntity()
        {
            var line = new DwgLine(new DwgPoint(1.0, 2.0, 3.0), new DwgPoint(4.0, 5.0, 6.0));
            var roundTrippedLine = (DwgLine)RoundTrip(line);
            Assert.Equal(line.P1, roundTrippedLine.P1);
            Assert.Equal(line.P2, roundTrippedLine.P2);
        }
    }
}

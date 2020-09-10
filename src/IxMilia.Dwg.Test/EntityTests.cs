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

        [Fact]
        public void RoundTripPolyline2DWithoutVertices()
        {
            var poly = new DwgPolyline2D();
            var roundTripped = (DwgPolyline2D)RoundTrip(poly);
            Assert.Empty(roundTripped.Vertices);
        }

        [Fact]
        public void RoundTripPolyline2DWithVertices()
        {
            var poly = new DwgPolyline2D(new DwgVertex2D(new DwgPoint(0.0, 0.0, 0.0)), new DwgVertex2D(new DwgPoint(10.0, 10.0, 0.0)));
            var roundTripped = (DwgPolyline2D)RoundTrip(poly);
            Assert.Equal(2, roundTripped.Vertices.Count);
            Assert.Equal(new DwgPoint(0.0, 0.0, 0.0), roundTripped.Vertices[0].Point);
            Assert.Equal(new DwgPoint(10.0, 10.0, 0.0), roundTripped.Vertices[1].Point);
        }

        [Fact]
        public void RoundTripPolyline3DWithoutVertices()
        {
            var poly = new DwgPolyline3D();
            var roundTripped = (DwgPolyline3D)RoundTrip(poly);
            Assert.Empty(roundTripped.Vertices);
        }

        [Fact]
        public void RoundTripPolyline3DWithVertices()
        {
            var poly = new DwgPolyline3D(new DwgVertex3D(new DwgPoint(0.0, 0.0, 0.0)), new DwgVertex3D(new DwgPoint(10.0, 10.0, 0.0)));
            var roundTripped = (DwgPolyline3D)RoundTrip(poly);
            Assert.Equal(2, roundTripped.Vertices.Count);
            Assert.Equal(new DwgPoint(0.0, 0.0, 0.0), roundTripped.Vertices[0].Point);
            Assert.Equal(new DwgPoint(10.0, 10.0, 0.0), roundTripped.Vertices[1].Point);
        }
    }
}

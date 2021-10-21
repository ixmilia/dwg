using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class ObjectTests : TestBase
    {
        [Fact]
        public void RoundTripGroup()
        {
            var g = new DwgGroup();
            g.Name = "the-name";
            g.IsSelectable = true;
            var o = new DwgGroup();
            g.Objects.Add(o);
            var roundTrippedGroup = (DwgGroup)RoundTrip(g);
            Assert.Equal(g.Name, roundTrippedGroup.Name);
            Assert.Equal(g.IsSelectable, roundTrippedGroup.IsSelectable);
            Assert.Single(g.Objects);
            Assert.Equal(g.Objects.Count, roundTrippedGroup.Objects.Count);
        }
    }
}

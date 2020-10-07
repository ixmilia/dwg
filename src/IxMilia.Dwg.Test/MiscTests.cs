using System.Collections.Generic;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class MiscTests
    {
        [Fact]
        public void EntityFlattenerWithChildItems()
        {
            // artificially assign handles
            var poly = new DwgPolyline2D(
                new DwgVertex2D(new DwgPoint(1.0, 1.0, 0.0)),
                new DwgVertex2D(new DwgPoint(2.0, 2.0, 0.0))
            );
            poly.Handle = new DwgHandle(1);
            poly.Vertices[0].Handle = new DwgHandle(2);
            poly.Vertices[1].Handle = new DwgHandle(3);
            poly.SeqEnd.Handle = new DwgHandle(4);
            var line = new DwgLine(
                new DwgPoint(3.0, 3.0, 0.0),
                new DwgPoint(4.0, 4.0, 0.0)
            );
            line.Handle = new DwgHandle(5);
            var entities = new List<DwgEntity>()
            {
                poly,
                line
            };
            var flattened = DwgEntityHelpers.FlattenAndAssignPointersForWrite(entities);

            // verify objects
            Assert.Equal(5, flattened.Count);
            Assert.Same(poly, flattened[0]);
            Assert.Same(poly.Vertices[0], flattened[1]);
            Assert.Same(poly.Vertices[1], flattened[2]);
            Assert.Same(poly.SeqEnd, flattened[3]);
            Assert.Same(line, flattened[4]);

            // verify entity pointers
            Assert.Equal(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0), poly.PreviousEntityHandle);
            Assert.Equal(line.Handle, poly.ResolveHandleReference(poly.NextEntityHandle));
            Assert.Equal(poly.Handle, line.ResolveHandleReference(line.PreviousEntityHandle));
            Assert.Equal(new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0), line.NextEntityHandle);
        }
    }
}

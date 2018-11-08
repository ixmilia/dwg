using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public abstract class TestBase
    {
        public static DwgDrawing RoundTrip(DwgDrawing drawing)
        {
            using (var ms = new MemoryStream())
            {
                drawing.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return DwgDrawing.Load(ms);
            }
        }

        public static DwgLayer RoundTrip(DwgLayer layer)
        {
            var drawing = new DwgDrawing();
            layer.LineType = drawing.LineTypes.Values.Single();
            drawing.Layers.Clear();
            drawing.Layers.Add(layer);
            var roundTrippedDrawing = RoundTrip(drawing);
            var roundTrippedLayer = roundTrippedDrawing.Layers.Values.Single();
            Assert.Equal(layer.Name, roundTrippedLayer.Name);
            return roundTrippedLayer;
        }

        public static DwgEntity RoundTrip(DwgEntity entity)
        {
            var layer = new DwgLayer() { Name = "test-layer" };
            layer.Entities.Add(entity);
            var roundTrippedLayer = RoundTrip(layer);
            var roundTrippedEntity = roundTrippedLayer.Entities.Single();
            return roundTrippedEntity;
        }
    }
}

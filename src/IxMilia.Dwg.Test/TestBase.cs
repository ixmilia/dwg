using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;

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

        public static DwgEntity RoundTrip(DwgEntity entity)
        {
            var drawing = new DwgDrawing();
            entity.Layer = drawing.CurrentLayer;
            drawing.ModelSpaceBlockRecord.Entities.Add(entity);
            var roundTrippedDrawing = RoundTrip(drawing);
            return roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
        }
    }
}

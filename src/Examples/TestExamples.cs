using IxMilia.Dwg;
using IxMilia.Dwg.Objects;
using Xunit;

namespace Examples
{
    public class EntitySamples
    {
        [Fact]
        public void WriteSingleLine()
        {
            var drawing = new DwgDrawing();
            drawing.FileHeader.Version = DwgVersionId.R14;
            
            // create a simple entity
            var line = new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(1.0, 1.0, 0.0));

            // all entities must have an assigned layer and the layer must already be part of the drawing
            // re-using the current layer ("0") for brevity
            line.Layer = drawing.CurrentLayer;

            // all entities must belong to a block and the model space block is probably what you want
            drawing.ModelSpaceBlockRecord.Entities.Add(line);

            drawing.SaveExample();
        }
    }
}

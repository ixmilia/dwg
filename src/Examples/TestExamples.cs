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

        [Fact]
        public void WriteMultipleLayers()
        {
            var drawing = new DwgDrawing();
            drawing.FileHeader.Version = DwgVersionId.R14;

            // each layer must have an assigned line type and the line type must already be part of the drawing
            // re-using the pre-defined "CONTINUOUS" line type for brevity
            var lineLayer = new DwgLayer("line-layer") { LineType = drawing.ContinuousLineType };
            var circleLayer = new DwgLayer("circle-layer") { LineType = drawing.ContinuousLineType };

            // add the layers to the drawing
            drawing.Layers.Add(lineLayer);
            drawing.Layers.Add(circleLayer);

            // add some entities to the drawing with the assigned layer
            var line = new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(1.0, 1.0, 0.0)) { Layer = lineLayer };
            var circle = new DwgCircle(new DwgPoint(0.0, 0.0, 0.0), 1.0) { Layer = circleLayer };
            drawing.ModelSpaceBlockRecord.Entities.Add(line);
            drawing.ModelSpaceBlockRecord.Entities.Add(circle);

            drawing.SaveExample();
        }

        [Fact]
        public void WriteCustomLineType()
        {
            var drawing = new DwgDrawing();
            drawing.FileHeader.Version = DwgVersionId.R14;

            // create a custom line type and add it to the file
            // this line type will have a dash of length 0.5 followed by a gap of length 0.25
            var dashed = new DwgLineType("dashed");
            dashed.DashInfos.Add(new DwgLineType.DwgLineTypeDashInfo(0.5));
            dashed.DashInfos.Add(new DwgLineType.DwgLineTypeDashInfo(0.25));
            drawing.LineTypes.Add(dashed);

            // now add a line using this line type
            var line = new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(10.0, 10.0, 0.0));
            line.Layer = drawing.CurrentLayer;
            line.LineType = dashed;
            drawing.ModelSpaceBlockRecord.Entities.Add(line);

            drawing.SaveExample();
        }

        [Fact]
        public void WriteBlock()
        {
            var drawing = new DwgDrawing();
            drawing.CurrentEntityLineType = drawing.ByLayerLineType;
            drawing.FileHeader.Version = DwgVersionId.R14;

            // create a block with a line from (0,0) to (1,1)
            var line = new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(1.0, 1.0, 0.0));
            line.Layer = drawing.CurrentLayer;

            var blockHeader = DwgBlockHeader.CreateBlockRecordWithName("my-block", drawing.CurrentLayer);
            blockHeader.Entities.Add(line);
            drawing.BlockHeaders.Add(blockHeader);

            // insert a copy of the block at location (3,3); the result is a line from (3,3) to (4,4)
            var insert = new DwgInsert(blockHeader);
            insert.Location = new DwgPoint(3.0, 3.0, 0.0);
            insert.Layer = drawing.CurrentLayer;

            drawing.ModelSpaceBlockRecord.Entities.Add(insert);

            drawing.SaveExample();
        }
    }
}

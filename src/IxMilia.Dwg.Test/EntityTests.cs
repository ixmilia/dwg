using System;
using System.IO;
using System.Linq;
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

        [Fact]
        public void RoundTripInsertWithoutAttributes()
        {
            var drawing = new DwgDrawing();

            var blockHeader = new DwgBlockHeader("some-block", new DwgBlock("some-block") { Layer = drawing.CurrentLayer }, new DwgEndBlock() { Layer = drawing.CurrentLayer });
            drawing.BlockHeaders.Add(blockHeader);

            var ins = new DwgInsert();
            ins.BlockHeader = blockHeader;
            ins.Layer = drawing.CurrentLayer;
            ins.Location = new DwgPoint(1.0, 2.0, 3.0);

            drawing.ModelSpaceBlockRecord.Entities.Add(ins);
            var roundTrippedDrawing = RoundTrip(drawing);

            var roundTripped = (DwgInsert)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.Equal("some-block", roundTripped.BlockHeader.Name);
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), roundTripped.Location);
            Assert.Empty(roundTripped.Attributes);
        }

        [Fact]
        public void RoundTripInsertWithAttributes()
        {
            var drawing = new DwgDrawing();

            var blockHeader = new DwgBlockHeader("some-block", new DwgBlock("some-block") { Layer = drawing.CurrentLayer }, new DwgEndBlock() { Layer = drawing.CurrentLayer });
            drawing.BlockHeaders.Add(blockHeader);

            var ins = new DwgInsert();
            ins.BlockHeader = blockHeader;
            ins.Layer = drawing.CurrentLayer;
            ins.Location = new DwgPoint(1.0, 2.0, 3.0);
            ins.Attributes.Add(new DwgAttribute("some-attribute"));

            drawing.ModelSpaceBlockRecord.Entities.Add(ins);
            var roundTrippedDrawing = RoundTrip(drawing);

            var roundTripped = (DwgInsert)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.Equal("some-block", roundTripped.BlockHeader.Name);
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), roundTripped.Location);
            Assert.Single(roundTripped.Attributes);
            Assert.Equal("some-attribute", roundTripped.Attributes[0].Value);
        }

        [Fact]
        public void RoundTripMInsertWithoutAttributes()
        {
            var drawing = new DwgDrawing();

            var blockHeader = new DwgBlockHeader("some-block", new DwgBlock("some-block") { Layer = drawing.CurrentLayer }, new DwgEndBlock() { Layer = drawing.CurrentLayer });
            drawing.BlockHeaders.Add(blockHeader);

            var ins = new DwgMInsert();
            ins.BlockHeader = blockHeader;
            ins.Layer = drawing.CurrentLayer;
            ins.Location = new DwgPoint(1.0, 2.0, 3.0);

            drawing.ModelSpaceBlockRecord.Entities.Add(ins);
            var roundTrippedDrawing = RoundTrip(drawing);

            var roundTripped = (DwgMInsert)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.Equal("some-block", roundTripped.BlockHeader.Name);
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), roundTripped.Location);
            Assert.Empty(roundTripped.Attributes);
        }

        [Fact]
        public void RoundTripMInsertWithAttributes()
        {
            var drawing = new DwgDrawing();

            var blockHeader = new DwgBlockHeader("some-block", new DwgBlock("some-block") { Layer = drawing.CurrentLayer }, new DwgEndBlock() { Layer = drawing.CurrentLayer });
            drawing.BlockHeaders.Add(blockHeader);

            var ins = new DwgMInsert();
            ins.BlockHeader = blockHeader;
            ins.Layer = drawing.CurrentLayer;
            ins.Location = new DwgPoint(1.0, 2.0, 3.0);
            ins.Attributes.Add(new DwgAttribute("some-attribute"));

            drawing.ModelSpaceBlockRecord.Entities.Add(ins);
            var roundTrippedDrawing = RoundTrip(drawing);

            var roundTripped = (DwgMInsert)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.Equal("some-block", roundTripped.BlockHeader.Name);
            Assert.Equal(new DwgPoint(1.0, 2.0, 3.0), roundTripped.Location);
            Assert.Single(roundTripped.Attributes);
            Assert.Equal("some-attribute", roundTripped.Attributes[0].Value);
        }

        [Fact]
        public void RoundTripSpline()
        {
            var spline = new DwgSpline();
            spline.SplineType = DwgSplineType.FitPointsOnly;
            spline.FitTolerance = 0.25;
            spline.StartTangentVector = new DwgVector(1.0, 2.0, 3.0);
            spline.EndTangentVector = new DwgVector(4.0, 5.0, 6.0);
            spline.FitPoints.Add(new DwgPoint(7.0, 8.0, 9.0));
            var roundTrippedSpline = (DwgSpline)RoundTrip(spline);
            Assert.Equal(spline.SplineType, roundTrippedSpline.SplineType);
            Assert.Equal(spline.FitTolerance, roundTrippedSpline.FitTolerance);
            Assert.Equal(spline.StartTangentVector, roundTrippedSpline.StartTangentVector);
            Assert.Equal(spline.EndTangentVector, roundTrippedSpline.EndTangentVector);
            Assert.Equal(spline.FitPoints.Single(), roundTrippedSpline.FitPoints.Single());
        }

        [Fact]
        public void RoundTripRegion()
        {
            var region = new DwgRegion();
            region.RawData.AddRange(new byte[]
            {
                0x01, 0x02, 0x03, 0x04
            });
            region.TrailingData.AddRange(new byte[]
            {
                0x05, 0x06
            });
            var roundTrippedRegion = (DwgRegion)RoundTrip(region);
            Assert.Equal(region.RawData, roundTrippedRegion.RawData);
            Assert.Equal(region.TrailingData, roundTrippedRegion.TrailingData);
            Assert.Equal(region.FinalBitCount, roundTrippedRegion.FinalBitCount);
            Assert.Equal(region.FinalByte, roundTrippedRegion.FinalByte);
        }

        [Fact]
        public void RoundTripSolid3D()
        {
            var solid = new DwgSolid3D();
            solid.RawData.AddRange(new byte[]
            {
                0x01, 0x02, 0x03, 0x04
            });
            solid.TrailingData.AddRange(new byte[]
            {
                0x05, 0x06
            });
            solid.FinalBitCount = 3;
            solid.FinalByte = 0x07;
            var roundTrippedSolid = (DwgSolid3D)RoundTrip(solid);
            Assert.Equal(solid.RawData, roundTrippedSolid.RawData);
            Assert.Equal(solid.TrailingData, roundTrippedSolid.TrailingData);
            Assert.Equal(solid.FinalBitCount, roundTrippedSolid.FinalBitCount);
            Assert.Equal(solid.FinalByte, roundTrippedSolid.FinalByte);
        }

        [Fact]
        public void CantWriteSolidWithDifferentZValues()
        {
            var solid = new DwgSolid()
            {
                FirstCorner = new DwgPoint(0.0, 0.0, 0.0),
                SecondCorner = new DwgPoint(0.0, 0.0, 0.0),
                ThirdCorner = new DwgPoint(0.0, 0.0, 0.0),
                FourthCorner = new DwgPoint(0.0, 0.0, 1.0), // this elevation is different
            };
            var drawing = new DwgDrawing();
            solid.Layer = drawing.CurrentLayer;
            drawing.ModelSpaceBlockRecord.Entities.Add(solid);
            using (var ms = new MemoryStream())
            {
                Assert.Throws<InvalidOperationException>(() => drawing.Save(ms));
            }
        }

        [Fact]
        public void CantWriteTraceWithDifferentZValues()
        {
            var trace = new DwgTrace()
            {
                FirstCorner = new DwgPoint(0.0, 0.0, 0.0),
                SecondCorner = new DwgPoint(0.0, 0.0, 0.0),
                ThirdCorner = new DwgPoint(0.0, 0.0, 0.0),
                FourthCorner = new DwgPoint(0.0, 0.0, 1.0), // this elevation is different
            };
            var drawing = new DwgDrawing();
            trace.Layer = drawing.CurrentLayer;
            drawing.ModelSpaceBlockRecord.Entities.Add(trace);
            using (var ms = new MemoryStream())
            {
                Assert.Throws<InvalidOperationException>(() => drawing.Save(ms));
            }
        }
    }
}

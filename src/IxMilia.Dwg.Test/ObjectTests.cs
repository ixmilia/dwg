﻿using System.Collections.Generic;
using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class ObjectTests : TestBase
    {
        [Theory]
        [MemberData(nameof(Entities))]
        public void RoundTripWellKnownEntities(DwgEntity entity)
        {
            var roundTripped = RoundTrip(entity);
            AssertEquivalent(entity, roundTripped);
        }

        [Fact]
        public void RoundTripGroup()
        {
            var g = new DwgGroup();
            g.Name = "the-name";
            g.IsSelectable = true;
            g.Objects.Add(new DwgGroup());
            var roundTrippedGroup = (DwgGroup)RoundTrip(g);
            Assert.Equal(g.Name, roundTrippedGroup.Name);
            Assert.Equal(g.IsSelectable, roundTrippedGroup.IsSelectable);
            Assert.Single(g.Objects);
            Assert.Equal(g.Objects.Count, roundTrippedGroup.Objects.Count);
        }

        [Fact]
        public void RoundTripIdBuffer()
        {
            var id = new DwgIdBuffer();
            id.Objects.Add(new DwgIdBuffer());
            var roundTrippedIdBuffer = (DwgIdBuffer)RoundTrip(id);
            Assert.Equal(id._unknownRc, roundTrippedIdBuffer._unknownRc);
            Assert.Single(id.Objects);
            Assert.Equal(id.Objects.Count, roundTrippedIdBuffer.Objects.Count);
        }

        [Fact]
        public void RoundTripEntityWithLineType()
        {
            var drawing = new DwgDrawing();
            var line = new DwgLine()
            {
                Layer = drawing.CurrentLayer,
                LineType = drawing.CurrentEntityLineType,
            };
            drawing.ModelSpaceBlockRecord.Entities.Add(line);
            var roundTrippedDrawing = RoundTrip(drawing);
            var roundTrippedLine = (DwgLine)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.NotNull(roundTrippedLine.LineType);
            Assert.Equal(drawing.CurrentEntityLineType.Name, roundTrippedLine.LineType.Name);
        }

        [Fact]
        public void RoundTripEntityWithCustomLineType()
        {
            var drawing = new DwgDrawing();
            var lineType = new DwgLineType("custom-line-type");
            lineType.DashInfos.Add(new DwgLineType.DwgLineTypeDashInfo(0.5));
            lineType.DashInfos.Add(new DwgLineType.DwgLineTypeDashInfo(0.25));
            drawing.LineTypes.Add(lineType);

            var line = new DwgLine()
            {
                Layer = drawing.CurrentLayer,
                LineType = lineType,
            };
            drawing.ModelSpaceBlockRecord.Entities.Add(line);

            var roundTrippedDrawing = RoundTrip(drawing);
            var roundTrippedLine = (DwgLine)roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Single();
            var roundTrippedLineType = roundTrippedLine.LineType;
            Assert.NotNull(roundTrippedLineType);
            Assert.Equal(2, roundTrippedLineType.DashInfos.Count);
            Assert.Equal(0.5, roundTrippedLineType.DashInfos[0].DashLength);
            Assert.Equal(-0.25, roundTrippedLineType.DashInfos[1].DashLength);
            Assert.Equal(new string(' ', 47), roundTrippedLineType.Description);
        }

        public static IEnumerable<object[]> Entities()
        {
            foreach (var entity in GetTestEntities())
            {
                yield return new object[] { entity };
            }
        }
    }
}

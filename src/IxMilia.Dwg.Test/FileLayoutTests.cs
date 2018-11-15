﻿using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileLayoutTests : TestBase
    {
        [Fact]
        public void ReadEntireFile()
        {
            var drawing = DwgDrawing.Load(Path.Combine("Drawings", "R14.dwg"));
            var line = (DwgLine)drawing.ModelSpaceBlockRecord.Entities.Single();
            Assert.Equal(DwgPoint.Origin, line.P1);
            Assert.Equal(new DwgPoint(10.0, 10.0, 0.0), line.P2);
        }

        [Fact]
        public void RoundTripDefaultFile()
        {
            // validate default values
            var defaultFile = new DwgDrawing();
            Assert.Equal("0", defaultFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", defaultFile.Styles.Single().Value.Name);
            Assert.Equal(2, defaultFile.BlockHeaders.Count);
            Assert.NotNull(defaultFile.BlockHeaders["*PAPER_SPACE"]);
            Assert.NotNull(defaultFile.BlockHeaders["*MODEL_SPACE"]);
            Assert.Equal(3, defaultFile.LineTypes.Count);
            Assert.NotNull(defaultFile.LineTypes["BYLAYER"]);
            Assert.NotNull(defaultFile.LineTypes["BYBLOCK"]);
            Assert.NotNull(defaultFile.LineTypes["CONTINUOUS"]);
            Assert.Equal("*ACTIVE", defaultFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(defaultFile.Layers["0"].LineType, defaultFile.LineTypes["CONTINUOUS"]));
            Assert.Equal(new[] { "ACAD", "ACAD_MLEADERVER" }, defaultFile.AppIds.Values.Select(a => a.Name));
            Assert.Equal("STANDARD", defaultFile.DimStyles.Single().Value.Name);
            Assert.True(ReferenceEquals(defaultFile.DimStyles.Single().Value.Style, defaultFile.Styles.Single().Value));

            // valiate round-trip
            var roundTrippedFile = RoundTrip(defaultFile);
            Assert.Equal("0", roundTrippedFile.Layers.Single().Value.Name);
            Assert.Equal("STANDARD", roundTrippedFile.Styles.Single().Value.Name);
            Assert.Equal(2, roundTrippedFile.BlockHeaders.Count);
            Assert.NotNull(roundTrippedFile.BlockHeaders["*PAPER_SPACE"]);
            Assert.NotNull(roundTrippedFile.BlockHeaders["*MODEL_SPACE"]);
            Assert.Equal(3, roundTrippedFile.LineTypes.Count);
            Assert.NotNull(roundTrippedFile.LineTypes["BYLAYER"]);
            Assert.NotNull(roundTrippedFile.LineTypes["BYBLOCK"]);
            Assert.NotNull(roundTrippedFile.LineTypes["CONTINUOUS"]);
            Assert.Equal("*ACTIVE", roundTrippedFile.ViewPorts.Single().Value.Name);
            Assert.True(ReferenceEquals(roundTrippedFile.Layers["0"].LineType, roundTrippedFile.LineTypes["CONTINUOUS"]));
            Assert.Equal(new[] { "ACAD", "ACAD_MLEADERVER" }, roundTrippedFile.AppIds.Values.Select(a => a.Name));
            Assert.Equal("STANDARD", roundTrippedFile.DimStyles.Single().Value.Name);
            Assert.True(ReferenceEquals(roundTrippedFile.DimStyles.Single().Value.Style, roundTrippedFile.Styles.Single().Value));
        }

        [Fact]
        public void CaseInsensitiveDictionaries()
        {
            var drawing = new DwgDrawing();
            Assert.True(ReferenceEquals(drawing.LineTypes["CONTINUOUS"], drawing.LineTypes["continuous"]));
        }
    }
}

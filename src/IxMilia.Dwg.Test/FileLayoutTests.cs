using System.IO;
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
            Assert.Equal(30, defaultFile.FileHeader.CodePage);
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
            Assert.Equal(30, roundTrippedFile.FileHeader.CodePage);
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
        public void RoundTripMultipleEntities()
        {
            var drawing = new DwgDrawing();
            drawing.ModelSpaceBlockRecord.Entities.Add(new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(10.0, 10.0, 0.0)) { Layer = drawing.CurrentLayer });
            drawing.ModelSpaceBlockRecord.Entities.Add(new DwgLine(new DwgPoint(5.0, 5.0, 0.0), new DwgPoint(15.0, 15.0, 0.0)) { Layer = drawing.CurrentLayer });
            var roundTrippedDrawing = RoundTrip(drawing);

            // verify handles
            var entities = roundTrippedDrawing.ModelSpaceBlockRecord.Entities;
            Assert.Equal(2, entities.Count);
            var l1 = (DwgLine)entities.First();
            var l2 = (DwgLine)entities.Last();
            Assert.Equal(l1.Handle.HandleOrOffset + 1, l2.Handle.HandleOrOffset);
            Assert.Equal(l1.PreviousEntityHandle, new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0));
            Assert.Equal(l1.NextEntityHandle, new DwgHandleReference(DwgHandleReferenceCode.HandlePlus1, 0));
            Assert.Equal(l2.PreviousEntityHandle, new DwgHandleReference(DwgHandleReferenceCode.HandleMinus1, 0));
            Assert.Equal(l2.NextEntityHandle, new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0));

            // verify data
            Assert.Equal(2, entities.Count);
            Assert.Equal(new DwgPoint(0.0, 0.0, 0.0), l1.P1);
            Assert.Equal(new DwgPoint(10.0, 10.0, 0.0), l1.P2);
            Assert.Equal(new DwgPoint(5.0, 5.0, 0.0), l2.P1);
            Assert.Equal(new DwgPoint(15.0, 15.0, 0.0), l2.P2);
        }

        [Fact]
        public void CaseInsensitiveDictionaries()
        {
            var drawing = new DwgDrawing();
            Assert.True(ReferenceEquals(drawing.LineTypes["CONTINUOUS"], drawing.LineTypes["continuous"]));
        }
    }
}

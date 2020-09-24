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
            VerifyDefaults(defaultFile);

            // valiate round-trip
            var roundTrippedFile = RoundTrip(defaultFile);
            VerifyDefaults(roundTrippedFile);

            void VerifyDefaults(DwgDrawing drawing)
            {
                Assert.Equal(30, drawing.FileHeader.CodePage);
                Assert.Equal("0", drawing.Layers.Single().Value.Name);
                Assert.Equal("STANDARD", drawing.Styles.Single().Value.Name);
                Assert.Equal(2, drawing.BlockHeaders.Count);
                Assert.NotNull(drawing.BlockHeaders.PaperSpace);
                Assert.NotNull(drawing.BlockHeaders.ModelSpace);
                Assert.Equal(3, drawing.LineTypes.Count);
                Assert.NotNull(drawing.LineTypes.ByLayer);
                Assert.NotNull(drawing.LineTypes.ByBlock);
                Assert.NotNull(drawing.LineTypes["CONTINUOUS"]);
                Assert.Equal("*ACTIVE", drawing.ViewPorts.Single().Value.Name);
                Assert.True(ReferenceEquals(drawing.Layers["0"].LineType, drawing.LineTypes["CONTINUOUS"]));
                Assert.Equal(new[] { "ACAD", "ACAD_MLEADERVER" }, drawing.AppIds.Values.Select(a => a.Name));
                Assert.Equal("STANDARD", drawing.DimStyles.Single().Value.Name);
                Assert.True(ReferenceEquals(drawing.DimStyles.Single().Value.Style, drawing.Styles.Single().Value));

                Assert.Equal(new[]
                {
                    "ACAD_GROUP",
                    "ACAD_LAYOUT",
                    "ACAD_MATERIAL",
                    "ACAD_MLEADERSTYLE",
                    "ACAD_MLINESTYLE",
                    "ACAD_PLOTSETTINGS",
                    "ACAD_SCALELIST",
                    "ACAD_TABLESTYLE",
                    "ACAD_VISUALSTYLE",
                    "ACDBHEADERROUNDTRIPXREC",
                    "ACDBVARIABLEDICTIONARY",
                }, drawing.NamedObjectDictionary.Keys.OrderBy(x => x));
            }
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

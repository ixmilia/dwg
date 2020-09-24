using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class ObjectIdentityTests : TestBase
    {
        [Fact]
        public void CaseInsensitiveDictionaries()
        {
            var drawing = new DwgDrawing();
            Assert.Same(drawing.LineTypes["CONTINUOUS"], drawing.LineTypes["continuous"]);
        }

        [Fact]
        public void WellKnownObjectsRetainIdentityInDefaultDrawing()
        {
            CheckWellKnownIdentities(new DwgDrawing());
        }

        [Fact]
        public void WellKnownObjectsRetainIdentityInRoundTrippedDrawing()
        {
            CheckWellKnownIdentities(RoundTrip(new DwgDrawing()));
        }

        [Fact]
        public void WellKnownObjectsRetainIdentityWhenChanged()
        {
            var dwg = new DwgDrawing();
            ReplaceWellKnownObjects(dwg);
            CheckWellKnownIdentities(dwg);
        }

        [Fact]
        public void WellKnownObjectsRetainIdentityWhenChangedInRoundTrippedDrawing()
        {
            var dwg = new DwgDrawing();
            ReplaceWellKnownObjects(dwg);
            CheckWellKnownIdentities(RoundTrip(dwg));
        }

        private static void ReplaceWellKnownObjects(DwgDrawing dwg)
        {
            dwg.ModelSpaceBlockRecord = DwgBlockHeader.CreateBlockRecordWithName(DwgBlockHeader.ModelSpaceBlockName, dwg.CurrentLayer);
            dwg.PaperSpaceBlockRecord = DwgBlockHeader.CreateBlockRecordWithName(DwgBlockHeader.PaperSpaceBlockName, dwg.CurrentLayer);
            dwg.ByLayerLineType = new DwgLineType(DwgLineTypeControlObject.ByLayerName);
            dwg.ByBlockLineType = new DwgLineType(DwgLineTypeControlObject.ByBlockName);
            dwg.CurrentEntityLineType = dwg.ByBlockLineType;
            dwg.ContinuousLineType = new DwgLineType(DwgLineTypeControlObject.ContinuousName);
        }

        private static void CheckWellKnownIdentities(DwgDrawing dwg)
        {
            Assert.Same(dwg.ModelSpaceBlockRecord, dwg.BlockHeaders.ModelSpace);
            Assert.Same(dwg.ModelSpaceBlockRecord, dwg.BlockHeaders[DwgBlockHeader.ModelSpaceBlockName]);

            Assert.Same(dwg.PaperSpaceBlockRecord, dwg.BlockHeaders.PaperSpace);
            Assert.Same(dwg.PaperSpaceBlockRecord, dwg.BlockHeaders[DwgBlockHeader.PaperSpaceBlockName]);

            Assert.Same(dwg.ByLayerLineType, dwg.LineTypes.ByLayer);
            Assert.Same(dwg.ByLayerLineType, dwg.LineTypes[DwgLineTypeControlObject.ByLayerName]);

            Assert.Same(dwg.ByBlockLineType, dwg.LineTypes.ByBlock);
            Assert.Same(dwg.ByBlockLineType, dwg.LineTypes[DwgLineTypeControlObject.ByBlockName]);

            Assert.Same(dwg.ContinuousLineType, dwg.LineTypes.Continuous);
            Assert.Same(dwg.ContinuousLineType, dwg.LineTypes[DwgLineTypeControlObject.ContinuousName]);
        }
    }
}

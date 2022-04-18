using System.IO;
using System.Linq;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class DefaultSettingsTests
    {
        [Fact]
        public void DefaultClassesArePopulatedInNewDrawing()
        {
            var drawing = new DwgDrawing();
            Assert.Equal(drawing.Classes.ToArray(), new[]
            {
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbDictionaryWithDefault", "ACDBDICTIONARYWDFLT", false, false),
                new DwgClassDefinition(0, 1153, "ObjectDBX Classes", "AcDbScale", "SCALE", false, false),
                new DwgClassDefinition(0, 4095, "ObjectDBX Classes", "AcDbVisualStyle", "VISUALSTYLE", false, false),
                new DwgClassDefinition(0, 1153, "ObjectDBX Classes", "AcDbMaterial", "MATERIAL", false, false),
                new DwgClassDefinition(0, 4095, "ObjectDBX Classes", "AcDbTableStyle", "TABLESTYLE", false, false),
                new DwgClassDefinition(0, 4095, "ACDB_MLEADERSTYLE_CLASS", "AcDbMLeaderStyle", "MLEADERSTYLE", false, false),
                new DwgClassDefinition(0, 1153, "SCENEOE", "AcDbSun", "SUN", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbDictionaryVar", "DICTIONARYVAR", false, false),
                new DwgClassDefinition(0, 1152, "ObjectDBX Classes", "AcDbCellStyleMap", "CELLSTYLEMAP", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbXrecord", "XRECORD", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbPolyline", "LWPOLYLINE", false, true),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbHatch", "HATCH", false, true),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbPlaceHolder", "ACDBPLACEHOLDER", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbLayout", "LAYOUT", false, false),
            });

            // and check they've been assigned the correct number during writing
            using var ms = new MemoryStream();
            drawing.Save(ms); // don't really care about the contents
            for (var i = 0; i < drawing.Classes.Count; i++)
            {
                Assert.Equal(500 + i, drawing.Classes[i].Number);
            }
        }
    }
}

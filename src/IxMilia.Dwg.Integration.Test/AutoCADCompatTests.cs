using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using IxMilia.Dwg.Objects;
using Xunit;

namespace IxMilia.Dwg.Integration.Test
{
    public class AutoCADCompatTests : CompatTestsBase
    {
        [AutoCADExistsFact]
        public void RoundTripEntities()
        {
            var drawing = new DwgDrawing();
            drawing.FileHeader.Version = DwgVersionId.R14;
            foreach (var entity in GetTestEntities())
            {
                entity.Layer = drawing.CurrentLayer;
                drawing.ModelSpaceBlockRecord.Entities.Add(entity);
            }

            var roundTrippedDrawing = RoundTripDrawingThroughAutoCad(drawing);
            Assert.Equal(drawing.ModelSpaceBlockRecord.Entities.Count, roundTrippedDrawing.ModelSpaceBlockRecord.Entities.Count);
            foreach (var entityPair in drawing.ModelSpaceBlockRecord.Entities.Zip(roundTrippedDrawing.ModelSpaceBlockRecord.Entities))
            {
                AssertEquivalent(entityPair.First, entityPair.Second);
            }
        }

        private IEnumerable<DwgEntity> GetTestEntities()
        {
            yield return new DwgArc(new DwgPoint(0.0, 0.0, 0.0), 1.0, 0.0, Math.PI);
            yield return new DwgCircle(new DwgPoint(1.0, 1.0, 0.0), 1.0);
            yield return new DwgLine(new DwgPoint(0.0, 0.0, 0.0), new DwgPoint(1.0, 1.0, 0.0));
        }

        private void AssertEquivalent(DwgEntity e1, DwgEntity e2)
        {
            Assert.Equal(e1.Color, e2.Color);
            Assert.Equal(e1.Layer.Name, e2.Layer.Name);
            Assert.Equal(e1.LineType?.Name, e2.LineType?.Name);
            Assert.Equal(e1.LineTypeScale, e1.LineTypeScale);
            switch ((e1, e2))
            {
                case (DwgArc a1, DwgArc a2):
                    Assert.Equal(a1.Center, a2.Center);
                    Assert.Equal(a1.Radius, a2.Radius);
                    Assert.Equal(a1.StartAngle, a2.StartAngle);
                    Assert.Equal(a1.EndAngle, a2.EndAngle);
                    Assert.Equal(a1.Thickness, a2.Thickness);
                    break;
                case (DwgCircle c1, DwgCircle c2):
                    Assert.Equal(c1.Center, c2.Center);
                    Assert.Equal(c1.Radius, c2.Radius);
                    Assert.Equal(c1.Thickness, c2.Thickness);
                    break;
                case (DwgLine l1, DwgLine l2):
                    Assert.Equal(l1.P1, l2.P1);
                    Assert.Equal(l1.P2, l2.P2);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported entity comparison: {e1.GetType().Name}/{e2.GetType().Name}");
            }
        }

        private DwgDrawing RoundTripDrawingThroughAutoCad(DwgDrawing drawing, [CallerMemberName] string testName = null)
        {
            var temp = new TestCaseDirectory($"test-{testName}");
            var inputFile = Path.Combine(temp.DirectoryPath, "input.dwg");
            var outputFile = Path.Combine(temp.DirectoryPath, "output.dwg");
            var scriptFile = Path.Combine(temp.DirectoryPath, "script.scr");
            drawing.Save(inputFile);

            var lines = new List<string>
            {
                "FILEDIA 0",
                $"OPEN \"{inputFile}\"",
                $"SAVEAS R14 \"{outputFile}\"",
                "FILEDIA 1",
                "QUIT Y"
            };
            File.WriteAllLines(scriptFile, lines);
            ExecuteAutoCadScript(scriptFile);

            var result = DwgDrawing.Load(outputFile);
            return result;
        }

        private void ExecuteAutoCadScript(string pathToScript)
        {
            WaitForProcess(AutoCADExistsFactAttribute.GetPathToAutoCad(), $"/b \"{pathToScript}\"", TimeSpan.FromSeconds(30));
            // TODO: kill all instances of senddmp.exe and fail if present
        }
    }
}

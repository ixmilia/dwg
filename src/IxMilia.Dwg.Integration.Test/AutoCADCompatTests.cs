using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
                $"SAVEAS {VersionIdToString(drawing.FileHeader.Version)} \"{outputFile}\"",
                "FILEDIA 1",
                "QUIT Y"
            };
            File.WriteAllLines(scriptFile, lines);
            ExecuteAutoCadScript(scriptFile);

            var result = DwgDrawing.Load(outputFile);
            return result;
        }

        private static string VersionIdToString(DwgVersionId version)
        {
            return version switch
            {
                DwgVersionId.R13 => "R13",
                DwgVersionId.R14 => "R14",
                _ => throw new NotSupportedException($"VersionId {version} is not supported"),
            };
        }

        private void ExecuteAutoCadScript(string pathToScript)
        {
            WaitForProcess(AutoCADExistsFactAttribute.GetPathToAutoCad(), $"/b \"{pathToScript}\"", TimeSpan.FromSeconds(30));
            // TODO: kill all instances of senddmp.exe and fail if present
        }
    }
}

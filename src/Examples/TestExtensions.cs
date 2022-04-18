using System.IO;
using System.Runtime.CompilerServices;
using IxMilia.Dwg;

namespace Examples
{
    internal static class TestExtensions
    {
        internal static void SaveExample(this DwgDrawing drawing, [CallerFilePath] string testFilePath = null, [CallerMemberName] string testName = null)
        {
            var testDirectory = Path.GetDirectoryName(testFilePath);
            var fullTestDirectory = Path.Combine(testDirectory, "SavedExamples");
            Directory.CreateDirectory(fullTestDirectory);

            var fileName = $"{testName}.dwg";
            var fullOutputPath = Path.Combine(fullTestDirectory, fileName);
            drawing.Save(fullOutputPath);
        }
    }
}

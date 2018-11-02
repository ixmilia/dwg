using System;
using System.IO;

namespace IxMilia.Dwg.Generator
{
    class Program
    {
        private const string GeneratedString = "Generated";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: requires one argument specifying the location of the root project.");
                Environment.Exit(1);
            }

            var projectDir = args[0];
            var outputDir = Path.Combine(projectDir, GeneratedString);

            Console.WriteLine($"Generating header variables into: {outputDir}");
            Console.WriteLine($"Generating objects into: {outputDir}");

            new HeaderVariablesGenerator(outputDir).Run();
            new ObjectGenerator(outputDir).Run();
        }
    }
}

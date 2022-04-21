using System;
using System.IO;

namespace IxMilia.Dwg.Generator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: requires one argument specifying the location of the root project.");
                Environment.Exit(1);
            }

            var outputDir = args[0];

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }

            Directory.CreateDirectory(outputDir);
            Console.WriteLine($"Generating header variables into: {outputDir}");
            Console.WriteLine($"Generating objects into: {outputDir}");

            new HeaderVariablesGenerator(outputDir).Run();
            new ObjectGenerator(outputDir).Run();
        }
    }
}

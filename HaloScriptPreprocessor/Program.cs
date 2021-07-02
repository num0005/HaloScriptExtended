/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;
using System.IO;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HaloScriptPreprocessor.Tests")]

namespace HaloScriptPreprocessor
{
    class Program
    {
        static string[] errorLevels =
        {
            "[Error] ",
            "[Warning] ",
            "[Informational] "
        };

        static ConsoleColor[] errorColors =
        {
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.Blue
        };


        static void ReportErrors(Error.Reporting reporting)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            Console.WriteLine($"{reporting.Messages.Count} diagnostic messages emitted!");
            int[] levelCount = { 0, 0, 0};
            foreach (Error.Message message in reporting.Messages)
            {
                levelCount[(int)message.Level]++;
                Console.ForegroundColor = errorColors[(int)message.Level];
                Console.WriteLine(errorLevels[(int)message.Level] + message.Content);
                if (message.Source is not null)
                {
                    string source = message.Source.Value.Match(
                        source => source.PrettyPrint(),
                        location => location.Formatted,
                        node =>
                        {
                            if (node.Source is not null)
                                return node.Source.Source.PrettyPrint();
                            else
                                return $"automatically generated code ¦ {node.ToString()}";
                        },
                        nodeNamed =>
                        {
                            if (nodeNamed.Source is not null)
                                return nodeNamed.Source.Source.PrettyPrint();
                            else
                                return $"automatically generated code: {nodeNamed.Name} ¦ {nodeNamed.ToString()}";
                        }
                    );
                    Console.WriteLine("in" + source);
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{levelCount[0]} fatal errors reported!");
            Console.WriteLine($"{levelCount[1]} warnings reported!");
            Console.WriteLine($"{levelCount[2]} informational messages!");

            // restore color
            Console.ForegroundColor = oldColor;
        }
        static void ProcessFile(IFileSystem fileSystem, string relativeFilePath)
        {
            IFileSystem.IFile? sourceFile = fileSystem.GetFile("hscx_scripts" + fileSystem.DirectorySeparator + relativeFilePath);
            if (sourceFile is null)
                throw new Exception("internal error!");

            Transpiler transpiler = new(fileSystem);

            if (transpiler.AddFile(sourceFile))
            {
                if (transpiler.RunPasses(Transpiler.Pass.Full))
                    transpiler.EmitCode(relativeFilePath);
            }

            ReportErrors(transpiler.ErrorReporting);
        }
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + "<scenario directory>");
                return;
            }
            string scenarioDirectory = args[0];
            PhysicalFileSystem fileSystem = new(scenarioDirectory);

            string sourceDirectory = Path.Combine(scenarioDirectory, "hscx_scripts");
            string outputDirectory = Path.Combine(scenarioDirectory, "scripts");

            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine($"\"{sourceDirectory}\" not found, no scripts to process!");
                return;
            }

            Directory.CreateDirectory(outputDirectory);

            Console.WriteLine($"Processing .hsc files in {sourceDirectory} and saving to {outputDirectory}");
            string[] files = Directory.GetFiles(sourceDirectory, "*.hsc");
            foreach (string file in files)
            {
                string relativeFilePath = Path.GetRelativePath(sourceDirectory, file);
                Console.WriteLine($"Processing {relativeFilePath}");

                ProcessFile(fileSystem, relativeFilePath);
            }

            Console.WriteLine("Done!");
        }
    }
}

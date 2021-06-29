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
        static void ProcessFile(IFileSystem fileSystem, string relativeFilePath)
        {
            IFileSystem.IFile? sourceFile = fileSystem.GetFile("hscx_scripts" + fileSystem.DirectorySeparator + relativeFilePath);

            Transpiler transpiler = new(fileSystem);
            if (sourceFile is not null)
                transpiler.AddFile(sourceFile);
            else
                throw new Exception("internal error!");

            transpiler.RunPasses(Transpiler.Pass.Full);

            transpiler.EmitCode(relativeFilePath);
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

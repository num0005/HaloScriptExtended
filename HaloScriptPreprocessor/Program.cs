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
        static void ProcessFile(string sourceDirectory, string outputDirectory, string file)
        {
            Parser.ASTBuilder builder = new(sourceDirectory, file);
            Interpreter.Interpreter interpreter = new(builder.Ast);
            Passes.ConstantGlobalPass constantGlobalPass = new(builder.Ast, interpreter);
            Passes.LoopUnrolling loopUnrolling = new(builder.Ast, interpreter);
            Passes.MacroExpansionPass macroExpansion = new(builder.Ast);
            Passes.CompileTimeEvaluationPass compileTimeEvaluationPass = new(builder.Ast, interpreter);
            constantGlobalPass.Run();
            loopUnrolling.Run();
            macroExpansion.Run();
            compileTimeEvaluationPass.Run();
            using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, file)))
            {
                Emitter.HaloScriptEmitter emitter = new(@writer, builder.Ast);
                emitter.Emit();
            }
        }
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + "<scenario directory>");
                return;
            }
            string scenarioDirectory = args[0];

            string sourceDirectory = Path.Combine(scenarioDirectory, "hscx_scripts");
            string outputDirectory = Path.Combine(scenarioDirectory, "scripts");

            Directory.CreateDirectory(outputDirectory);

            Console.WriteLine($"Processing .hsc files in {sourceDirectory} and saving to {outputDirectory}");
            string[] files = Directory.GetFiles(sourceDirectory, "*.hsc");
            foreach (string file in files)
            {
                string relativeFilePath = Path.GetRelativePath(sourceDirectory, file);
                Console.WriteLine($"Processing {relativeFilePath}");
                ProcessFile(sourceDirectory, outputDirectory, relativeFilePath);
            }

            Console.WriteLine("Done!");
        }
    }
}

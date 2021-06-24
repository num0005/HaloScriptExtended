using System;
using System.IO;

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
            constantGlobalPass.Run();
            loopUnrolling.Run();
            macroExpansion.Run();
            using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, file)))
            {
                Emitter.HaloScriptEmitter emitter = new(@writer, builder.Ast);
                emitter.Emit();
            }
        }
        static void Main(string[] args)
        {
            if (args.Length != 1)
                Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName + "<scenario directory>");
            string scenarioDirectory = args[0];

            string sourceDirectory = Path.Combine(scenarioDirectory, "hscx_scripts");
            string outputDirectory = Path.Combine(scenarioDirectory, "scripts");

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

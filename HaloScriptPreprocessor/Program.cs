using System;
using System.IO;

namespace HaloScriptPreprocessor
{
    class Program
    {
        static void Main(string[] args)
        {
            //string a_tutorial_mission = File.ReadAllText("01a_tutorial_mission.lisp");
            //Parser.ExpressionParser parser = new (a_tutorial_mission);
            //var parsed = parser.GetParsedExpressions();
            //parser = null;
            //Parser.ASTBuilder builder = new(parsed);
            Parser.ASTBuilder builder = new("", "test.hsc");
            //Parser.ASTBuilder builder = new("", "crash.lisp");
            StringWriter @string = new();
            Emitter.HaloScriptEmitter emitter = new(@string, builder.Ast);
            Interpreter.Interpreter interpreter = new(builder.Ast);
            Passes.ConstantGlobalPass constantGlobalPass = new(builder.Ast, interpreter);
            Passes.LoopUnrolling loopUnrolling = new(builder.Ast, interpreter);
            Passes.MacroExpansionPass macroExpansion = new(builder.Ast);
            constantGlobalPass.Run();
            loopUnrolling.Run();
            macroExpansion.Run();
            emitter.Emit();
            Console.WriteLine(@string.ToString());
        }
    }
}

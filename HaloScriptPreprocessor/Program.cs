using System;
using System.IO;

namespace HaloScriptPreprocessor
{
    class Program
    {
        const string test = @"(script dormant chapter_mirror
	(sleep ""30"")
	(cinematic_set_title title_1)
	(sleep 150)
	(hud_cinematic_fade 1 0.5)
	(cinematic_show_letterbox false)
)";
        static void Main(string[] args)
        {
            string a_tutorial_mission = File.ReadAllText("01a_tutorial_mission.lisp");
            Parser.ExpressionParser parser = new (a_tutorial_mission);
            var parsed = parser.GetParsedExpressions();
            parser = null;
            Parser.ASTBuilder builder = new(parsed);

            Console.WriteLine(parsed);
        }
    }
}

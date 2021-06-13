using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public enum ScriptType
    {
        Startup,
        Continuous,
        Dormant,
        Static,
        Stub,
        CommandScript,
        Macro, // hscx extension

        Invalid
    }

    public static class ScriptTypeMethods
    {
        public static ScriptType ParseScriptType(this string str)
        {
            switch (str)
            {
                case "startup":
                    return ScriptType.Startup;
                case "continuous":
                    return ScriptType.Continuous;
                case "dormant":
                    return ScriptType.Dormant;
                case "static":
                    return ScriptType.Static;
                case "stub":
                    return ScriptType.Stub;
                case "command_script":
                    return ScriptType.CommandScript;
                case "macro":
                    return ScriptType.Macro;
                default:
                    return ScriptType.Invalid;
            }
        }

        public static ScriptType ParseScriptType(this ReadOnlySpan<char> str)
        {
            if (str.SequenceEqual("startup"))
                return ScriptType.Startup;
            else if (str.SequenceEqual("continuous"))
                return ScriptType.Continuous;
            else if (str.SequenceEqual("dormant"))
                return ScriptType.Dormant;
            else if (str.SequenceEqual("static"))
                return ScriptType.Static;
            else if (str.SequenceEqual("Stub"))
                return ScriptType.Stub;
            else if (str.SequenceEqual("command_script"))
                return ScriptType.CommandScript;
            else if (str.SequenceEqual("macro"))
                return ScriptType.Macro;
            else
                return ScriptType.Invalid;
        }

        public static string ToSyntaxString(this ScriptType type)
        {
            switch (type)
            {
                case ScriptType.Startup:
                    return "startup";
                case ScriptType.Continuous:
                    return "continuous";
                case ScriptType.Dormant:
                    return "dormant";
                case ScriptType.Static:
                    return "static";
                case ScriptType.Stub:
                    return "stub";
                case ScriptType.CommandScript:
                    return "command_script";
                case ScriptType.Macro:
                    return "macro";
            }
            return "";
        }
    }
}

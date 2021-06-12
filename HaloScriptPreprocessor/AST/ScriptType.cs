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
                case "macro":
                    return ScriptType.Macro;
                default:
                    return ScriptType.Invalid;
            }
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
                case ScriptType.Macro:
                    return "macro";
            }
            return "";
        }
    }
}

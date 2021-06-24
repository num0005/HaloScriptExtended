using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    public class MacroExpansionPass : PassBase
    {
        public MacroExpansionPass(AST.AST ast) : base(ast) { }
        protected override void OnVisitCode(Code code)
        {
            if (code.Function.Value is Script script && script.Type == ScriptType.Macro)
            {
                if (script.Arguments is null)
                    throw new Exception("Invalid AST - macro without arguments!");
                if (script.Arguments.Count != code.Arguments.Count)
                    throw new Parser.InvalidExpression(code.Source.Source, "Wrong number of arguments!");
                List<Value> codeArgs = new(code.Arguments);
                Dictionary<Value, Value> mapping = new();
                for (int i = 0; i < codeArgs.Count; i++)
                    mapping[new(null, script.Arguments[i].name)] = codeArgs[i];

                code.Function = new Atom("begin");
                code.Arguments.Clear();
                foreach (Value value in script.Codes)
                {
                    Value rewrittenValue = value.Clone(code);
                    rewrittenValue.Rewrite(mapping);
                    code.Arguments.AddLast(rewrittenValue);
                }
            }
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, Node parent)
        {
            return false;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return script.Type == ScriptType.Macro;
        }

        protected override void OnVisitValue(Value value)
        {
        }

    }
}

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    class ConstantGlobalPass : PassBase
    {
        public ConstantGlobalPass(AST.AST ast, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
        }
        protected override void OnVisitCode(Code code)
        {
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument)
        {
            return false;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return global.IsConst;
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            if (value.Content.Value is Global global && global.IsConst)
            {
                Interpreter.Value? intGlobal = _interpreter.InteruptGlobal(global);
                if (intGlobal is null)
                    value.Content = global.Value.Content;
                else
                    value.Content = new AST.Atom(intGlobal.GetString(), value);
            }
        }

        private Interpreter.Interpreter _interpreter;
    }
}

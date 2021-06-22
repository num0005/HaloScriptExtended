using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    class CompileTimeEvaluationPass : PassBase
    {
        public CompileTimeEvaluationPass(AST.AST ast, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            Interpreter.Value? interuptedValue = _interpreter.InteruptValue(value);
            if (interuptedValue is not null)
                value.Content = new AST.Atom(interuptedValue.GetString(), value);
        }

        protected override void OnVisitCode(Code code)
        {
            if (code.FunctionSpan.SequenceEqual("begin") && code.Arguments.Count == 1)
            {

            }

        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        private Interpreter.Interpreter _interpreter;
    }
}

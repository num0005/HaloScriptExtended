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
            Interpreter.Value? interuptedValue = _interpreter.InterpretValue(value);
            if (interuptedValue is not null)
                value.Content = new AST.Atom(interuptedValue.GetString(), value);
            else if (value.Content.Value is Code code && SimplifyCode(code) is Code simpleCode)
                value.Content = simpleCode;
        }

        private Code? SimplifyCode(Code code)
        {
            if (code.FunctionSpan.SequenceEqual("begin") && code.Arguments.Count == 1
                    && code.Arguments.First().Content.Value is Code beginArg)
                return beginArg.Clone(code.ParentNode);

            return null;
        }

        protected override void OnVisitCode(Code code)
        {

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

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    class LoopUnrolling : PassBase
    {
        public LoopUnrolling(AST.AST ast) : base(ast) { }
        protected override void OnVisitCode(Code code)
        {
            throw new NotImplementedException();
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument)
        {
            throw new NotImplementedException();
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            throw new NotImplementedException();
        }
    }
}

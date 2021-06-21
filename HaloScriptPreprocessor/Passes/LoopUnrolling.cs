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
        protected override void VisitCode(Code code)
        {
            throw new NotImplementedException();
        }

        protected override bool VisitCodeArgument(LinkedListNode<Value> argument)
        {
            throw new NotImplementedException();
        }

        protected override bool VisitGlobal(Global global)
        {
            throw new NotImplementedException();
        }

        protected override bool VisitScript(Script script)
        {
            throw new NotImplementedException();
        }

        protected override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }
    }
}

using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Code : Node
    {
        public Code(Parser.Expression source, Atom function, LinkedList<Value> arguments) : base(source)
        {
            Function = function;
            Arguments = arguments;
        }

        public OneOf<Atom, Script> Function;
        public LinkedList<Value> Arguments;

        public ReadOnlySpan<char> FunctionSpan
        {
            get
            {
                if (Function.IsT0)
                    return Function.AsT0.ToSpan();
                if (Function.IsT1)
                    return Function.AsT1.Name.ToSpan();
                Debug.Fail("unreachable");
                return "";
            }
        }

        public override uint NodeCount
        {
            get
            {
                uint count = 1;
                foreach (var value in Arguments)
                {
                    count += value.NodeCount;
                }
                return count;
            }
        }

    }
}

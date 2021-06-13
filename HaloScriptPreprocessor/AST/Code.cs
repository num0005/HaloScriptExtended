using System;
using System.Collections.Generic;
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

        public Atom Function;
        public LinkedList<Value> Arguments;

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

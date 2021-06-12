using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    class Code : INode
    {
        public Atom Function;
        public LinkedList<Value> Arguments = new();

        public uint NodeCount
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    class Script
    {
        public ScriptType Type;
        public ValueType? ReturnValueType;
        public LinkedList<Code> Codes = new();
    }
}

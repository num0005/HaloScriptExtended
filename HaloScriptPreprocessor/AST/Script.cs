using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Script : NodeNamed
    {
        public Script(Parser.Expression source, ScriptType type,  Atom name, LinkedList<Value> code, ValueType? valueType = null) : base(source)
        {   
            Type = type;
            ScriptName = name;
            Codes = code;
            ReturnValueType = valueType;
        }

        public ScriptType Type;
        public ValueType? ReturnValueType;
        public Atom ScriptName;
        public LinkedList<Value> Codes = new();

        public override Atom Name => ScriptName;

        public override uint NodeCount {
            get
            {
                uint count = 1;
                foreach (var code in Codes)
                    count += code.NodeCount;
                return count;
            }
        }
    }
}

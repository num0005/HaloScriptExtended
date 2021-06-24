using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Global : NodeNamed
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Global(Parser.Value value) : base(value, null) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Global(Parser.Expression source, Atom name, ValueType type, Value value) : base(source, name)
        {
            ValueType = type;
            Value = value;
        }

        public ValueType ValueType;
        public Value Value;

        public bool IsConst;

        public override uint NodeCount => 1 + Value.NodeCount;

        public override Node Clone(Node? parent = null)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            Global clonedGlobal = new(Source);
#pragma warning restore CS8604 // Possible null reference argument.
            clonedGlobal._name = Name.Clone(clonedGlobal);
            clonedGlobal.ValueType = ValueType;
            clonedGlobal.Value = Value.Clone(clonedGlobal);
            clonedGlobal.IsConst = IsConst;

            return clonedGlobal;
        }

        public override void Rewrite(Dictionary<Value, Value> mapping)
        {
            Value.Rewrite(mapping);
        }
    }
}

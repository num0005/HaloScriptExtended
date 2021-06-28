/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.Collections.Generic;

namespace HaloScriptPreprocessor.AST
{
    public class Global : NodeNamed
    {
        private Global(Global global, Node? parent) : base(global) {
            ParentNode = parent;
            ValueType = global.ValueType;
            Value = global.Value.Clone(this);
            IsConst = global.IsConst;
        }
        public Global(Parser.Expression source, Atom name, ValueType type, Value value) : base(source, name)
        {
            ValueType = type;
            Value = value;
        }

        public ValueType ValueType;
        public Value Value;

        public bool IsConst;

        public override uint NodeCount => 1 + Value.NodeCount;

        public override Global Clone(Node? parent = null)
        {
            return new Global(this, parent);
        }

        public override void Rewrite(Dictionary<Value, Value> mapping)
        {
            if (mapping.ContainsKey(Value))
                Value = mapping[Value].Clone(Value.ParentNode);
            Value.Rewrite(mapping);
        }
    }
}

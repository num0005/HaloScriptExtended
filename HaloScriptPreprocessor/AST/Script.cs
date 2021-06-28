/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.Collections.Generic;
using System.Linq;

namespace HaloScriptPreprocessor.AST
{
    public class Script : NodeNamed
    {
        public Script(Script script, Node? parent) : base(script)
        {
            ParentNode = parent;
            ReturnValueType = script.ReturnValueType;
            Type = script.Type;
            if (script.Arguments is not null)
                Arguments = script.Arguments.ToList();
            foreach (Value code in script.Codes)
                Codes.AddLast(code.Clone(this));
        }
        public Script(Parser.Expression source, ScriptType type, Atom name, LinkedList<Value> code, ValueType? valueType = null, List<(ValueType type, Atom name)>? arguments = null) : base(source, name)
        {
            Type = type;
            Codes = code;
            ReturnValueType = valueType;
            Arguments = arguments;
        }

        public ScriptType Type;
        public ValueType? ReturnValueType;
        public Atom ScriptName => Name;
        public LinkedList<Value> Codes = new();
        public List<(ValueType type, Atom name)>? Arguments;

        public override uint NodeCount
        {
            get
            {
                uint count = 1;
                foreach (var code in Codes)
                    count += code.NodeCount;
                return count;
            }
        }

        public override Script Clone(Node? parent = null)
        {
            return new Script(this, parent);
        }

        public override void Rewrite(Dictionary<Value, Value> mapping)
        {
            LinkedListNode<Value>? arg = Codes.First;
            while (arg is not null)
            {
                if (mapping.ContainsKey(arg.Value))
                    arg.Value = mapping[arg.Value].Clone(this); // clone so parent is set correctly
                arg.Value.Rewrite(mapping);
                arg = arg.Next;
            }
        }
    }
}

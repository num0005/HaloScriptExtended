using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Global : NodeNamed
    {
        public Global(Parser.Expression source, Atom name, ValueType type, Value content) : base(source)
        {
            Name = name;
            ValueType = type;
            Content = content;
        }
        public override Atom Name { get; }

        public ValueType ValueType;
        public Value Content;

        public override uint NodeCount => 1 + Content.NodeCount;
    }
}

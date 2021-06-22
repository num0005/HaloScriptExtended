using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Value : Node
    {
        private Value(Parser.Value? source) : base(source) { }

        public Value(Parser.Atom source, Atom atom) : base(source)
        {
            Content = atom;
        }

        public Value(Parser.Expression source, Code code) : base(source)
        {
            Content = code;
        }

        public OneOf<Atom, Code, Global, Script> Content;

        public override uint NodeCount => Content.IsT1 ? Content.AsT1.NodeCount : 1;

        public override Value Clone(Node? parent = null)
        {
            Value value = new(Source);
            Content.Switch(
                atom => value.Content = atom.Clone(value),
                code => value.Content = code.Clone(value),
                global => value.Content = global,
                script => value.Content = script
            );

            return value;
        }
    }
}

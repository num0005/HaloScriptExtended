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
    }
}

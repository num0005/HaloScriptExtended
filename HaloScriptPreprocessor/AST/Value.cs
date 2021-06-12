using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    class Value : INode
    {
        public Value(Atom atom)
        {
            Content = atom;
        }

        public Value(Code code)
        {
            Content = code;
        }

        public readonly OneOf<Atom, Code> Content;

        public uint NodeCount => Content.IsT1 ? Content.AsT1.NodeCount : 1;
    }
}

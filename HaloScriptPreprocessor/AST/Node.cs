using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public abstract class Node
    {
        public Node(Parser.Value? source)
        {
            Source = source;
        }
        /// <summary>
        /// Number of nodes in the tree if <c>this</c> was the root
        /// </summary>
        public abstract uint NodeCount { get; }

        public readonly Parser.Value? Source;
    }

    public abstract class NodeNamed : Node
    {
        public NodeNamed(Parser.Value source) : base(source)
        {
        }
        public abstract Atom Name { get; }
    }
}

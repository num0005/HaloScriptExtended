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

        /// <summary>
        /// Parser value the <c>Node</c> was created from or <c>null</c> if it wasn't
        /// </summary>
        public readonly Parser.Value? Source;

        /// <summary>
        /// Parent node or null if root node (make sure to update this!)
        /// </summary>
        public Node? ParentNode;
    }

    public abstract class NodeNamed : Node
    {
        public NodeNamed(Parser.Value source) : base(source)
        {
            ParentNode = null;
        }
        public abstract Atom Name { get; }
    }
}

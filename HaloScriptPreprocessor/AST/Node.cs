/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.Collections.Generic;

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

        /// <summary>
        /// Clone this node, optionally setting a parent
        /// </summary>
        /// <param name="parent">New parent</param>
        /// <returns>New clone</returns>
        public abstract Node Clone(Node? parent = null);

        /// <summary>
        /// Rewrite terms in child nodes
        /// </summary>
        /// <param name="mapping">A mapping from the old term to the new term</param>
        public abstract void Rewrite(Dictionary<Value, Value> mapping);
    }

    public abstract class NodeNamed : Node
    {
        protected NodeNamed(NodeNamed other) : base(other.Source)
        {
            ParentNode = null;
            _name = other.Name.Clone(this);
        }
        public NodeNamed(Parser.Value source, Atom name) : base(source)
        {
            ParentNode = null;
            _name = name;
        }
        public Atom Name => _name;
        protected Atom _name;
    }
}

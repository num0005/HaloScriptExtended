using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    interface INode
    {
        /// <summary>
        /// Number of nodes in the tree if <c>this</c> was the root
        /// </summary>
        public uint NodeCount { get; }
    }
}

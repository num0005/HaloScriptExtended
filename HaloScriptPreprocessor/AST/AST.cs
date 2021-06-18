using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    class AST
    {
        public bool IsUserDefinedName(Atom nameAtom)
        {
            return _userNameMapping.ContainsKey(nameAtom.ToString());
        }

        public void Add(NodeNamed node)
        {
            _userNameMapping[node.Name.ToString()] = node;
        }

        public NodeNamed? Get(string name)
        {
            NodeNamed? node = null;
            _userNameMapping.TryGetValue(name, out node);
            return node;
        }

        private Dictionary<string, NodeNamed> _userNameMapping = new();
    }
}

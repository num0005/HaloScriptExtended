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

        public NodeNamed Get(string name)
        {
            return _userNameMapping[name];
        }

        private Dictionary<string, NodeNamed> _userNameMapping = new();
    }
}

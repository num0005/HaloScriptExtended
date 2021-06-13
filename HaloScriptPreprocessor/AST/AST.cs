using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    class NameMapping<Node> where Node : NodeNamed
    {
        public void Rename(string oldName, string newName)
        {
            Node value;
            Debug.Assert(_mapping.Remove(oldName, out value));
            value.Name.Value = newName;
            _mapping.Add(newName, value);
        }

        public bool Contains(string name)
        {
            return _mapping.ContainsKey(name);
        }

        public Node Get(string name)
        {
            return _mapping[name];
        }

        public void Add(Node node)
        {
            _mapping[node.Name.ToString()] = node;
        }

        private Dictionary<string, Node> _mapping = new();
    }
    class AST
    {
        public NameMapping<Global> UserGlobals = new();
        public NameMapping<Global> ConstantGlobals = new();
        public NameMapping<Script> UserFunctions = new();

        public bool IsUserDefinedName(Atom nameAtom)
        {
            return _userNameMapping.ContainsKey(nameAtom.ToString());
        }

        private Dictionary<string, NodeNamed> _userNameMapping = new();
    }
}

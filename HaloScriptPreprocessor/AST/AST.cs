using System.Collections.Generic;

namespace HaloScriptPreprocessor.AST
{
    public class AST
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

        public void Remove(string name)
        {
            _userNameMapping.Remove(name);
        }

        public IReadOnlyDictionary<string, NodeNamed> UserNameMapping => _userNameMapping;

        private Dictionary<string, NodeNamed> _userNameMapping = new();
    }
}

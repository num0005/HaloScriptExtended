using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    public abstract class PassBase
    {
        public PassBase(AST.AST ast)
        {
            _ast = ast;
        }
        /// <summary>
        /// visit a global, modify or remove it
        /// </summary>
        /// <param name="global"></param>
        /// <returns>Whatever the global should be removed</returns>
        protected abstract bool VisitGlobal(AST.Global global);

        private void VisitGlobalInternal(string name, AST.Global global)
        {
            if (RecordEnterNode(global)) // we visited this node already
                return;
            if (VisitGlobal(global))
                _removeList.Add(name);
            else
                VisitValueInternal(global.Value);
        }

        protected abstract void VisitValue(AST.Value value);

        private void VisitValueInternal(AST.Value value)
        {
            if (RecordEnterNode(value)) // we visited this node already
                return;
            VisitValue(value);
            value.Content.Switch(_ => { }, code => VisitCodeInternal(code), global => VisitGlobalInternal(global.Name.ToString(), global), script => VisitScriptInternal(script.Name.ToString(), script));
        }

        protected abstract void VisitCode(AST.Code code);
        private void VisitCodeInternal(AST.Code code)
        {
            if (RecordEnterNode(code)) // we visited this node already
                return;
            VisitCode(code);
            code.Function.Switch(_ => { }, script => VisitScriptInternal(script.Name.ToString(), script));
            foreach (AST.Value arg in code.Arguments)
                VisitValueInternal(arg);
        }

        protected abstract bool VisitScript(AST.Script script);
        private void VisitScriptInternal(string name, AST.Script script)
        {
            if (RecordEnterNode(script)) // we visited this node already
                return;
            if (VisitScript(script))
                _removeList.Add(name);
            else
                script.Codes.ToList().ForEach(value => VisitValueInternal(value));
        }

        /// <summary>
        /// Run a pass
        /// </summary>
        public void Run()
        {
            foreach (KeyValuePair<string, AST.NodeNamed> entry in _ast.UserNameMapping)
            {
                if (entry.Value is AST.Script script)
                    VisitScriptInternal(entry.Key, script);
                else if (entry.Value is AST.Global global)
                    VisitGlobalInternal(entry.Key, global);
            }

            foreach (string toRemove in _removeList)
                _ast.Remove(toRemove);
        }
        private bool RecordEnterNode(AST.Node node)
        {
            if (_visted.Contains(node))
                return true;
            _visted.Add(node);
            return false;
        }
        private readonly List<string> _removeList = new();
        private readonly HashSet<AST.Node> _visted = new();
        private readonly AST.AST _ast;
    }
}

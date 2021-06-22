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
        protected abstract bool OnVisitGlobal(AST.Global global);

        public void VisitGlobal(AST.Global global, string? name = null)
        {
            if (RecordEnterNode(global)) // we visited this node already
                return;
            if (name is null)
                name = global.Name.ToString();
            if (OnVisitGlobal(global))
                _removeList.Add(name);
            else
                VisitValue(global.Value);
        }

        protected abstract void OnVisitValue(AST.Value value);

        public void VisitValue(AST.Value value)
        {
            if (RecordEnterNode(value)) // we visited this node already
                return;
            OnVisitValue(value);
            value.Content.Switch(_ => { }, code => VisitCode(code), global => VisitGlobal(global, global.Name.ToString()), script => VisitScript(script, script.Name.ToString()));
        }


        private void VisitArgsInteral(LinkedList<AST.Value> arguments)
        {
            LinkedListNode<AST.Value>? arg = arguments.First;
            while (arg is not null)
            {
                bool remove = VisitCodeArgumentInternal(arg);
                if (remove)
                {
                    var nextArg = arg.Next;
                    arguments.Remove(arg);
                    arg = nextArg;
                }
                else
                {
                    arg = arg.Next;
                }
            }
        }
        protected abstract void OnVisitCode(AST.Code code);
        public void VisitCode(AST.Code code)
        {
            if (RecordEnterNode(code)) // we visited this node already
                return;
            OnVisitCode(code);
            code.Function.Switch(_ => { }, script => VisitScript(script));
            VisitArgsInteral(code.Arguments);
        }

        protected abstract bool OnVisitCodeArgument(LinkedListNode<AST.Value> argument);
        private bool VisitCodeArgumentInternal(LinkedListNode<AST.Value> argument)
        {
            bool remove = OnVisitCodeArgument(argument);
            if (!remove)
                VisitValue(argument.Value);
            return remove;
        }

        protected abstract bool OnVisitScript(AST.Script script);
        private void VisitScript(AST.Script script, string? name = null)
        {
            if (RecordEnterNode(script)) // we visited this node already
                return;
            if (name is null)
                name = script.Name.ToString();
            if (OnVisitScript(script))
                _removeList.Add(name);
            else
                VisitArgsInteral(script.Codes);
        }

        /// <summary>
        /// Run a pass
        /// </summary>
        public void Run()
        {
            foreach (KeyValuePair<string, AST.NodeNamed> entry in _ast.UserNameMapping)
            {
                if (entry.Value is AST.Script script)
                    VisitScript(script, entry.Key);
                else if (entry.Value is AST.Global global)
                    VisitGlobal(global, entry.Key);
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

using System.Collections.Generic;

namespace HaloScriptPreprocessor.Passes
{
    public abstract class PassBase
    {
        public PassBase(AST.AST ast)
        {
            _ast = ast;
        }
        /// <summary>
        ///  Called upon visiting a global before visiting the child nodes
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected abstract void OnVisitValue(AST.Value value);

        public void VisitValue(AST.Value value)
        {
            if (RecordEnterNode(value)) // we visited this node already
                return;
            OnVisitValue(value);
            if (VisitReferences)
                value.Content.Switch(_ => { }, code => VisitCode(code), global => VisitGlobal(global, global.Name.ToString()), script => VisitScript(script, script.Name.ToString()));
            else if (value.Content.Value is AST.Code code)
                VisitCode(code);
        }


        private void VisitArgsInteral(LinkedList<AST.Value> arguments, AST.Node parent)
        {
            LinkedListNode<AST.Value>? arg = arguments.First;
            while (arg is not null)
            {
                bool remove = VisitCodeArgumentInternal(arg, parent);
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
            VisitArgsInteral(code.Arguments, code);
        }

        protected abstract bool OnVisitCodeArgument(LinkedListNode<AST.Value> argument, AST.Node parent);
        private bool VisitCodeArgumentInternal(LinkedListNode<AST.Value> argument, AST.Node parent)
        {
            bool remove = OnVisitCodeArgument(argument, parent);
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
                VisitArgsInteral(script.Codes, script);
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
        private bool RecordEnterNode<Node>(Node node) where Node : AST.Node
        {
            if (_visted.Contains(node))
                return true;
            _visted.Add(node);
            return false;
        }
        private readonly List<string> _removeList = new();
        private readonly HashSet<AST.Node> _visted = new();
        private readonly AST.AST _ast;

        public bool VisitReferences = true;
    }
}

/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Passes
{
    public class MacroExpansionPass : PassBase
    {
        public MacroExpansionPass(AST.AST ast) : base(ast) { }

        List<Value>? expandMacro(Code code, Node parent)
        {
            if (code.Function.Value is not Script script || script.Type != ScriptType.Macro)
                return null;
            List<Value> expandedMacro = new();
            if (script.Arguments is null)
                throw new Exception("Invalid AST - macro without arguments!");
            if (script.Arguments.Count != code.Arguments.Count)
                throw new Parser.InvalidExpression(code.Source.Source, "Wrong number of arguments!");

            List<Value> codeArgs = new(code.Arguments);
            Dictionary<Value, Value> mapping = new();
            for (int i = 0; i < codeArgs.Count; i++)
                mapping[new(null, script.Arguments[i].name)] = codeArgs[i];

            foreach (Value value in script.Codes)
            {
                Value rewrittenValue = value.Clone(parent);
                rewrittenValue.Rewrite(mapping);
                expandedMacro.Add(rewrittenValue);
            }
            return expandedMacro;
        }
        protected override void OnVisitCode(Code code)
        {
            if (expandMacro(code, code) is List<Value> expanded)
            {
                code.Function = new Atom("begin");
                code.Arguments.Clear();
                foreach (Value value in expanded)
                    code.Arguments.AddLast(value);
            }
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, Node parent)
        {
            if (parent is not Script || argument.List is not LinkedList<Value>)
                return false;
            if (argument.Value.Content.Value is Code code && expandMacro(code, parent) is List<Value> expanded)
            {
                LinkedListNode<Value> lastNode = argument;
                foreach (Value value in expanded)
                    lastNode = argument.List.AddAfter(lastNode, value);
                return true; // remove the macro call
            }
            return false;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return script.Type == ScriptType.Macro;
        }

        protected override void OnVisitValue(Value value)
        {
        }

    }
}

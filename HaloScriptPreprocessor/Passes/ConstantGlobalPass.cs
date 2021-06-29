/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System.Collections.Generic;

namespace HaloScriptPreprocessor.Passes
{
    class ConstantGlobalPass : PassBase
    {
        public ConstantGlobalPass(AST.AST ast, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
        }
        protected override void OnVisitCode(Code code)
        {
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, AST.Node parent)
        {
            return false;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            if (!global.IsConst)
                return false;
            if (GetGlobalValueStrng(global) is null)
            {
                global.IsConst = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            if (value.Content.Value is Global global && global.IsConst)
            {
                if (GetGlobalValueStrng(global) is string globalStringValue)
                    value.Content = new Atom(globalStringValue, value);
            }
        }

        private string? GetGlobalValueStrng(Global global)
        {
            Interpreter.Value? intGlobal = _interpreter.InterpretGlobal(global);
            if (intGlobal is null)
                return null;
            else
                return intGlobal.GetString();
        }

        private Interpreter.Interpreter _interpreter;
    }
}

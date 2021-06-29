/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaloScriptPreprocessor.Passes
{
    class CompileTimeEvaluationPass : PassBase
    {
        public CompileTimeEvaluationPass(AST.AST ast, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            Interpreter.Value? interuptedValue = _interpreter.InterpretValue(value);
            if (interuptedValue is not null && interuptedValue.GetString() is string valueString)
                value.Content = new Atom(valueString, value);
            else if (value.Content.Value is Code code && SimplifyCode(code) is Code simpleCode)
                value.Content = simpleCode;
        }

        private Code? SimplifyCode(Code code)
        {
            return null;
            ReadOnlySpan<char> func = code.FunctionSpan;
            if (func.SequenceEqual("begin") && code.Arguments.Count == 1
                    && code.Arguments.First().Content.Value is Code beginArg)
                return beginArg.Clone(code.ParentNode);
            if (func.SequenceEqual("if") && code.Arguments.Count <= 3 && code.Arguments.Count >= 2)
            {
                Interpreter.Value? predicate = _interpreter.InterpretValue(code.Arguments.First());
                if (predicate is null)
                    return code;
                // if (predicate.GetBoolean() is true)


            }
            return code;
        }

        protected override void OnVisitCode(Code code)
        {

        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, AST.Node parent)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        private Interpreter.Interpreter _interpreter;
    }
}

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
            else if (value.Content.Value is Code code)
            {
                if (SimplifyCode(code) is Code newCode)
                    value.Content = newCode; // figure out what to do if we get null
                if (SimplifyCodeToValue(code, value.ParentNode) is Value newValue)
                    value.Content = newValue.Content;
            }
        }

        private Code? SimplifyCode(Code code)
        {
            ReadOnlySpan<char> func = code.FunctionSpan;
            if (func.SequenceEqual("begin") && code.Arguments.Count == 1
                    && code.Arguments.First().Content.Value is Code beginArg)
                return beginArg.Clone(code.ParentNode);
            return code;
        }

        private Value? SimplifyCodeToValue(Code code, Node? parent)
        {
            ReadOnlySpan<char> func = code.FunctionSpan;
            if (func.SequenceEqual("if") && code.Arguments.Count <= 3 && code.Arguments.Count >= 2)
            {
                List<Value> args = code.Arguments.ToList();
                Interpreter.Value? predicate = _interpreter.InterpretValue(args[0]);
                if (predicate is null)
                    return null;
                if (predicate.GetBoolean() is true)
                    return args[1].Clone(parent);
                // todo what should this return if there isn't an else expression?
                if (predicate.GetBoolean() is false && code.Arguments.Count == 3)
                    return args[2].Clone(parent);



            }
            return null;
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

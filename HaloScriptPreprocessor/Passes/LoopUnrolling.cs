/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HaloScriptPreprocessor.Passes
{
    class LoopUnrolling : PassBase
    {
        public LoopUnrolling(AST.AST ast, Error.Reporting reporting, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
            _reporting = reporting;
        }
        protected override void OnVisitCode(Code code)
        {
        }

        private bool NeedBegin(Node parent)
        {
            if (parent is Script)
                return false;
            if (parent is Code code)
                return !code.FunctionSpan.SequenceEqual("begin");
            return true;
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, AST.Node parent)
        {
            if (argument.Value.Content.Value is Code code && code.FunctionSpan.SequenceEqual("loop"))
            {
                List<Value> unrolledLoop = CreateUnrolledLoop(code, parent);

                if (unrolledLoop is null)
                    return false;

                bool needBegin = NeedBegin(parent);
                LinkedList<Value> argumentsList;
                LinkedListNode<Value>? previousArg;

                if (needBegin)
                {
                    code.Function = new Atom("begin", code);
                    code.Arguments.Clear();
                    previousArg = null;
                    argumentsList = code.Arguments;
                } else
                {
                    previousArg = argument;
                    if (argument.List is not null)
                        argumentsList = argument.List;
                    else
                        return false;
                }

                foreach (Value value in unrolledLoop)
                    previousArg = (previousArg is null) ? argumentsList.AddFirst(value) : argumentsList.AddAfter(previousArg, value);

                return !needBegin; // remove the node unless begin was used
            }

            return false;
        }

        private List<Value>? CreateUnrolledLoop(Code loopExpression, AST.Node parent)
        {
            Debug.Assert(loopExpression.FunctionSpan.SequenceEqual("loop"));
            if (loopExpression.Arguments.Count < 3)
            {
                _reporting.Report(Error.Level.Error, loopExpression, "Invalid loop expression, expecting (loop <counter> <start value> <end value> [expressions])");
                return null;
            }
            if (loopExpression.Arguments.First is not LinkedListNode<Value> loopCounter)
                throw new InvalidOperationException("Unexpected missing arg!");
            if (loopCounter.Next is not LinkedListNode<Value> startNode)
                throw new InvalidOperationException("Unexpected missing arg!");
            if (startNode.Next is not LinkedListNode<Value> endNode)
                throw new InvalidOperationException("Unexpected missing arg!");

            Func<Value, string, long?> evalBound = (Value value, string name) =>
            {
                Interpreter.Value? intValue = _interpreter.InterpretValue(value);
                if (intValue is null)
                {
                    _reporting.Report(Error.Level.Error, startNode.Value, $"Unable to evaluate {name} value for loop!");
                    return null;
                }
                if (intValue.GetLong() is not long longValue)
                {
                    _reporting.Report(Error.Level.Error, startNode.Value, $"Unable to evaluate loop {name} value as a long!");
                    return null;
                }
                return longValue;
            };

            if (evalBound(startNode.Value, "start") is not long startValue 
                    || evalBound(endNode.Value, "end") is not long endValue)
                return null;
            return CreateUnrolledLoop(endNode.Next, parent, loopCounter.Value, startValue, endValue);
        }

        private static List<Value> CreateUnrolledLoop(LinkedListNode<Value>? template, AST.Node parent, AST.Value loopCounter, long start, long end)
        {
            List<Value> values = new();
            for (long i = start; i < end; i++)
            {
                Value iteratorValue = new(null, new Atom(i.ToString()));
                LinkedListNode<Value>? templateValue = template;
                while (templateValue is not null)
                {
                    Value clonedValue = templateValue.Value.Clone(parent);
                    clonedValue.Rewrite(new() { [loopCounter] = iteratorValue });

                    values.Add(clonedValue);

                    templateValue = templateValue.Next;
                }
            }
            return values;
        }

        protected override bool OnVisitGlobal(Global global)
        {
            return false;
        }

        protected override bool OnVisitScript(Script script)
        {
            return false;
        }

        protected override void OnVisitValue(Value value)
        {
            if (value.Content.Value is Code code && code.FunctionSpan.SequenceEqual("loop"))
            {
                List<Value>? unrolledLoop = CreateUnrolledLoop(code, code);
                if (unrolledLoop is null)
                    return;

                // Replace the loop expression with a begin expression
                code.Function = new Atom("begin");
                code.Arguments = new(unrolledLoop);
            }
        }

        readonly private Interpreter.Interpreter _interpreter;
        readonly private Error.Reporting _reporting;
    }
}

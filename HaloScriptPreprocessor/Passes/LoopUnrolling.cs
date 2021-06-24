using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HaloScriptPreprocessor.Passes
{
    class LoopUnrolling : PassBase
    {
        public LoopUnrolling(AST.AST ast, Interpreter.Interpreter interpreter) : base(ast)
        {
            _interpreter = interpreter;
        }
        protected override void OnVisitCode(Code code)
        {
        }

        protected override bool OnVisitCodeArgument(LinkedListNode<Value> argument, AST.Node parent)
        {
            if (argument.Value.Content.Value is Code code && code.FunctionSpan.SequenceEqual("loop"))
            {
                LinkedListNode<Value> previousArg = argument;

                List<Value> unrolledLoop = CreateUnrolledLoop(code, parent);
                LinkedList<Value>? argumentsList = argument.List;
                if (argumentsList is null)
                    return false;
                foreach (Value value in unrolledLoop)
                    previousArg = argumentsList.AddAfter(previousArg, value);

                return true; // remove the node
            }

            return false;
        }

        private List<Value> CreateUnrolledLoop(Code loopExpression, AST.Node parent)
        {
            Debug.Assert(loopExpression.FunctionSpan.SequenceEqual("loop"));
            if (loopExpression.Arguments.First is not LinkedListNode<Value> loopCounter)
                throw new Exception(); // todo error reporting
            if (loopCounter.Next is not LinkedListNode<Value> startNode)
                throw new Exception(); // todo error reporting
            if (startNode.Next is not LinkedListNode<Value> endNode)
                throw new Exception(); // todo error reporting
            Interpreter.Value? start = _interpreter.InterpretValue(startNode.Value);
            Interpreter.Value? end = _interpreter.InterpretValue(endNode.Value);
            if (start is null || end is null)
                throw new Exception(); // todo error reporting
            if (start.GetLong() is not long startValue || end.GetLong() is not long endValue)
                throw new Exception(); // todo error reporting
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
                List<Value> unrolledLoop = CreateUnrolledLoop(code, code);
                // Replace the loop expression with a begin expression
                code.Function = new Atom("begin");
                code.Arguments = new(unrolledLoop);
            }
        }

        private Interpreter.Interpreter _interpreter;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HaloScriptPreprocessor.Interpreter
{
    public class Interpreter
    {
        public Interpreter(AST.AST ast)
        {
            _ast = ast;
        }
        /*
                Value? interuptScript(AST.Script script)
                {
                    if (script.Type == AST.ScriptType.Static || script.Type == AST.ScriptType.Macro)
                        return interuptUserFunction(code, script);
                }
        */
        public Value? InterpretGlobal(AST.Global global)
        {
            if (!global.IsConst)
                return null; // todo, if we are evaluating another global we can still interrupt
            return interpret(global,
                global => InterpretValue(global.Value)
                );
        }

        public Value? InterpretValue(AST.Value value)
        {
            return interpret(value, value => value.Content.Match(
                atom => new Value(atom),
                code => InterpretCode(code),
                global => InterpretGlobal(global),
                script => new Value(script.Name)
                ));
        }

        private bool? interpretBooleanValue(AST.Value value)
        {
            Value? result = InterpretValue(value);
            if (result is null || result.GetBoolean() is not bool boolResult)
                return null;
            return boolResult;
        }

        private float? interpretRealValue(AST.Value value)
        {
            Value? result = InterpretValue(value);
            if (result is null || result.GetFloat() is not float realResult)
                return null;
            return realResult;
        }

        private (float, float)? interpretBinaryFloatArguments(LinkedList<AST.Value> args)
        {
            var first = args.First;
            if (first is null)
                return null;
            var second = first.Next;
            if (second is null)
                return null;
            if (second.Next is not null)
                return null;
            float? a = interpretRealValue(first.Value);
            float? b = interpretRealValue(second.Value);
            if (a is null || b is null)
                return null;
            return ((float)a, (float)b);
        }
        private (Value, Value)? interpretBinaryArguments(LinkedList<AST.Value> args)
        {
            var first = args.First;
            if (first is null)
                return null;
            var second = first.Next;
            if (second is null)
                return null;
            if (second.Next is not null)
                return null;
            Value? a = InterpretValue(first.Value);
            Value? b = InterpretValue(second.Value);
            if (a is null || b is null)
                return null;
            return (a, b);
        }


        public Value? InterpretCode(AST.Code code)
        {
            return interpret(code, code =>
            {
                ReadOnlySpan<char> functionName = code.FunctionSpan;
                if (functionName.SequenceEqual("if"))
                {
                    if (code.Arguments.First is not LinkedListNode<AST.Value> first)
                        return null;
                    if (first.Next is not LinkedListNode<AST.Value> second)
                        return null;
                    LinkedListNode<AST.Value>? third = second.Next;
                    if (interpretBooleanValue(first.Value) is not bool condition)
                        return null;
                    if (condition)
                        return InterpretValue(second.Value);
                    else if (third is LinkedListNode<AST.Value> elseVal)
                        return InterpretValue(elseVal.Value);
                    else
                        return null;
                }
                else if (functionName.SequenceEqual("cond"))
                {
                    return null; // todo
                }
                else if (functionName.SequenceEqual("and"))
                {
                    bool? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        if (interpretBooleanValue(arg) is not bool value)
                            return null;
                        if (!value)
                            return new Value(false);
                        result ??= true;
                    }
                    return (result is null) ? null : new Value((bool)result);
                }
                else if (functionName.SequenceEqual("or"))
                {
                    bool? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        if (interpretBooleanValue(arg) is not bool value)
                            return null;
                        if (value)
                            return new Value(true);
                        result ??= false;
                    }
                    return (result is null) ? null : new Value((bool)result);
                }
                else if (functionName.SequenceEqual("+"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interpretRealValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? 0.0f) + value;
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("-"))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 - results.Item2);
                }
                else if (functionName.SequenceEqual("*"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interpretRealValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? 1.0f) * value;
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("/"))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 / results.Item2);
                }
                else if (functionName.SequenceEqual("min"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interpretRealValue(arg);
                        if (value is not float number)
                            return null;
                        result = MathF.Min((result ?? number), number);
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("max"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interpretRealValue(arg);
                        if (value is not float number)
                            return null;
                        result = MathF.Max((result ?? number), number);
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("="))
                {
                    if (interpretBinaryArguments(code.Arguments) is not (Value, Value) args)
                        return null;
                    if (args.Item1.IsEqual(args.Item2) is not bool equal)
                        return null;
                    return new Value(equal);
                }
                else if (functionName.SequenceEqual("!="))
                {
                    if (interpretBinaryArguments(code.Arguments) is not (Value, Value) args)
                        return null;
                    if (args.Item1.IsEqual(args.Item2) is not bool equal)
                        return null;
                    return new Value(!equal);
                }
                else if (functionName.SequenceEqual(">"))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 > results.Item2);
                }
                else if (functionName.SequenceEqual("<"))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 < results.Item2);
                }
                else if (functionName.SequenceEqual(">="))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 >= results.Item2);
                }
                else if (functionName.SequenceEqual("<="))
                {
                    (float, float)? args = interpretBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 <= results.Item2);
                }
                if (code.Function.Value is AST.Script userScript)
                {
                    if (userScript.Type == AST.ScriptType.Static || userScript.Type == AST.ScriptType.Macro)
                        return interpretUserFunction(code, userScript);
                    else if (userScript.Type == AST.ScriptType.Stub)
                        return null; // can't eval a stub
                    else
                        return null; // todo throw an error here?
                }
                return null;
            });
        }

        private Value? interpretUserFunction(AST.Code code, AST.Script userScript)
        {
            Debug.Assert(userScript.Type == AST.ScriptType.Static || userScript.Type == AST.ScriptType.Macro);
            if (userScript.Type == AST.ScriptType.Static)
            {
                if (code.Arguments.Count != 0)
                    return null; // todo throw an error here
                return null; // todo implement

            }
            else if (userScript.Type == AST.ScriptType.Macro)
            {
                return null; // todo implement
            }
            return null;
        }

        private Value? interpret<T>(T node, Func<T, Value?> func) where T : AST.Node
        {
            if (_valueCache.ContainsKey(node))
                return _valueCache[node];
            Value? result = func(node);
            _valueCache[node] = result;
            return result;
        }

        public bool IsInCache(AST.Node node)
        {
            return _valueCache.ContainsKey(node);
        }

        private readonly Dictionary<AST.Node, Value?> _valueCache = new();
        private readonly AST.AST _ast;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Interpreter
{
    class Interpreter
    {
        Interpreter(AST.AST ast)
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
        Value? interuptGlobal(AST.Global global)
        {
            return interupt(global,
                global => interuptValue(global.Value)
                );
        }

        Value? interuptValue(AST.Value value)
        {
            return interupt(value, value => value.Content.Match(
                atom => new Value(atom),
                code => interuptCode(code),
                global => interuptGlobal(global),
                script => new Value(script.Name)
                ));
        }

        bool? interuptBooleanValue(AST.Value value)
        {
            Value? result = interuptValue(value);
            if (result is null || result.GetBoolean() is not bool boolResult)
                return null;
            return boolResult;
        }

        float? interuptRealValue(AST.Value value)
        {
            Value? result = interuptValue(value);
            if (result is null || result.GetFloat() is not float realResult)
                return null;
            return realResult;
        }

        (float, float)? interuptBinaryFloatArguments(LinkedList<AST.Value> args)
        {
            var first = args.First;
            if (first is null)
                return null;
            var second = first.Next;
            if (second is null)
                return null;
            if (second.Next is not null)
                return null;
            float? a = interuptRealValue(first.Value);
            float? b = interuptRealValue(second.Value);
            if (a is null || b is null)
                return null;
            return ((float)a, (float)b);
        }
        (Value, Value)? interuptBinaryArguments(LinkedList<AST.Value> args)
        {
            var first = args.First;
            if (first is null)
                return null;
            var second = first.Next;
            if (second is null)
                return null;
            if (second.Next is not null)
                return null;
            Value? a = interuptValue(first.Value);
            Value? b = interuptValue(second.Value);
            if (a is null || b is null)
                return null;
            return (a, b);
        }


        Value? interuptCode(AST.Code code)
        {
            return interupt(code, code =>
            {
                ReadOnlySpan<char> functionName = code.FunctionSpan;
                if (functionName.SequenceEqual("if"))
                {
                    if (code.Arguments.First is not LinkedListNode<AST.Value> first)
                        return null;
                    if (first.Next is not LinkedListNode<AST.Value> second)
                        return null;
                    LinkedListNode<AST.Value>? third = second.Next;
                    if (interuptBooleanValue(first.Value) is not bool condition)
                        return null;
                    if (condition)
                        return interuptValue(second.Value);
                    else if (third is LinkedListNode<AST.Value> elseVal)
                        return interuptValue(elseVal.Value);
                    else
                        return null;
                } else if (functionName.SequenceEqual("cond"))
                {
                    return null; // todo
                } else if (functionName.SequenceEqual("and"))
                {
                    bool? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        bool? value = interuptBooleanValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? true) ? value : false;
                    }
                    return (result is null) ? null : new Value((bool)result);
                }
                else if (functionName.SequenceEqual("or"))
                {
                    bool? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        bool? value = interuptBooleanValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? false) ? true : value;
                    }
                    return (result is null) ? null : new Value((bool)result);
                }
                else if (functionName.SequenceEqual("+"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interuptRealValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? 0.0f) + value;
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("-"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interuptRealValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? 0.0f) - value;
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("*"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interuptRealValue(arg);
                        if (value is null)
                            return null;
                        result = (result ?? 1.0f) * value;
                    }
                    return (result is null) ? null : new Value((float)result);
                }
                else if (functionName.SequenceEqual("/"))
                {
                    (float, float)? args = interuptBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 / results.Item2);
                }
                else if (functionName.SequenceEqual("min"))
                {
                    float? result = null;
                    foreach (AST.Value arg in code.Arguments)
                    {
                        float? value = interuptRealValue(arg);
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
                        float? value = interuptRealValue(arg);
                        if (value is not float number)
                            return null;
                        result = MathF.Min((result ?? number), number);
                    }
                    return (result is null) ? null : new Value((float)result);
                } else if (functionName.SequenceEqual("="))
                {
                    if (interuptBinaryArguments(code.Arguments) is not (Value, Value) args)
                        return null;
                    if (args.Item1.IsEqual(args.Item2) is not bool equal)
                        return null;
                    return new Value(equal);
                }  else if (functionName.SequenceEqual("!="))
                {
                    if (interuptBinaryArguments(code.Arguments) is not (Value, Value) args)
                        return null;
                    if (args.Item1.IsEqual(args.Item2) is not bool equal)
                        return null;
                    return new Value(!equal);
                }
                else if (functionName.SequenceEqual(">"))
                {
                    (float, float)? args = interuptBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 > results.Item2);
                }
                else if (functionName.SequenceEqual("<"))
                {
                    (float, float)? args = interuptBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 < results.Item2);
                }
                else if (functionName.SequenceEqual(">="))
                {
                    (float, float)? args = interuptBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 >= results.Item2);
                }
                else if (functionName.SequenceEqual("<="))
                {
                    (float, float)? args = interuptBinaryFloatArguments(code.Arguments);
                    if (args is not (float, float) results)
                        return null;
                    return new Value(results.Item1 <= results.Item2);
                }
                if (code.Function.Value is AST.Script userScript)
                {
                    if (userScript.Type == AST.ScriptType.Static || userScript.Type == AST.ScriptType.Macro)
                        return interuptUserFunction(code, userScript);
                    else if (userScript.Type == AST.ScriptType.Stub)
                        return null; // can't eval a stub
                    else
                        return null; // todo throw an error here?
                }
                return null;
            });
        }

        private Value? interuptUserFunction(AST.Code code, AST.Script userScript)
        {
            Debug.Assert(userScript.Type == AST.ScriptType.Static || userScript.Type == AST.ScriptType.Macro);
            if (userScript.Type == AST.ScriptType.Static)
            {
                if (code.Arguments.Count != 0)
                    return null; // todo throw an error here
                if ()
                
            } else if (userScript.Type == AST.ScriptType.Macro)
            {
                return null; // todo implement
            }
            return null;
        }

        private Value? interupt<T>(T node, Func<T, Value?> func) where T : AST.Node
        {
            if (_valueCache.ContainsKey(node))
                return _valueCache[node];
            Value? result = func(node);
            _valueCache[node] = result;
            return result;
        }

        private bool isInCache(AST.Node node)
        {
            return _valueCache.ContainsKey(node);
        }

        private readonly Dictionary<AST.Node, Value?> _valueCache = new();
        private readonly AST.AST _ast;
    }
}
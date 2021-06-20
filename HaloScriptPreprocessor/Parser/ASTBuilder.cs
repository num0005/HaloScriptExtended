using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Parser
{
    class ASTBuilder
    {

        private struct expressionReader
        {
            public expressionReader(Expression expression)
            {
                Expression = expression;
                _index = 0;
            }

            public Value? Next()
            {
                if (_index == Expression.Values.Count)
                    return null;
                return Expression.Values[_index++];
            }

            public bool IsEoS()
            {
                return _index == Expression.Values.Count;
            }

            private Value NextExpectValue()
            {
                if (Next() is Value value)
                    return value;
                else
                    throw new UnexpectedExpression(Expression.Source, "Unexpected end of expression!");
            }

            public Expression NextExpectExpression(string message)
            {
                return NextExpectValue().ExpectExpression(message);
            }

            public Atom NextExpectAtom(string message)
            {
                return NextExpectValue().ExpectAtom(message);
            }

            public int Index => _index;

            public readonly Expression Expression;
            private int _index;
        }

        public ASTBuilder(string directory, string mainFile)
        {
            _directory = directory;
            _mainFile = mainFile;
            // parse expressions
            parseExpressions();
            // build AST from expressions
            build();

            resolve();
        }

        private void resolve()
        {
            foreach (KeyValuePair<string, AST.NodeNamed> entry in _ast.UserNameMapping)
            {
                NodeNamed rootNode = entry.Value;
                if (rootNode is Global global)
                    resolveValue(global.Value, global);
                if (rootNode is Script script)
                {
                    foreach (AST.Value value in script.Codes)
                        resolveValue(value, script);
                }
            }
        }

        private void resolveValue(AST.Value value, AST.Node parent)
        {
            value.ParentNode = parent;
            if (value.Content.IsT0)
            { // attempt to resolve the name to a global 
                NodeNamed? resolved = _ast.Get(value.Content.AsT0.ToString()); // todo, make this use span not a string
                if (resolved is Global global)
                    value.Content = global;
                else if (resolved is Script script)
                    value.Content = script;
            } else if (value.Content.IsT1)
            {
                resolveCode(value.Content.AsT1, value);
            }
        }

        private void resolveCode(AST.Code code, AST.Node parent)
        {
            code.ParentNode = parent;
            NodeNamed? resolved = _ast.Get(code.Function.AsT0.ToString()); // todo, make this use span not a string
            if (resolved is Global global)
                throw new UnexpectedAtom(code.Function.AsT0.Source.Source, "Globals are not functions!");
            else if (resolved is Script script)
                code.Function = script;
            foreach (AST.Value arg in code.Arguments)
                resolveValue(arg, code);
        }

        private void build()
        {
            ReadOnlySpan<char> globalSpan      = "global".AsSpan();
            ReadOnlySpan<char> constglobalSpan = "constglobal".AsSpan();
            ReadOnlySpan<char> scriptSpan      = "script".AsSpan();
            ReadOnlySpan<char> importSpan      = "import".AsSpan();

            foreach (Expression expression in _parsed.Expressions)
            {
                if (expression.Values.Count == 0)
                    throw new UnexpectedExpression(expression.Source, "Unexpected empty expression!");
                expressionReader reader = new(expression);

                Atom expressionType = reader.NextExpectAtom(
                    "Expecting \"global\", \"script\" or \"constglobal\" but got an expression!");
                   
                ReadOnlySpan<char> typeSpan = expressionType.Source.Span;

                if (typeSpan.SequenceEqual(globalSpan))
                {
                    // build global AST
                    addNamedNode(buildGlobal(expression, isConst: false));
                } else if (typeSpan.SequenceEqual(scriptSpan))
                {
                    // build script AST
                    addNamedNode(buildScript(ref reader));
                } else if (typeSpan.SequenceEqual(constglobalSpan))
                {
                    // build constant global AST
                    addNamedNode(buildGlobal(expression, isConst: true));
                } else if (typeSpan.SequenceEqual(importSpan))
                {
                    continue; // imports are already handled in parseExpressions
                } else
                {
                    throw new UnexpectedExpression(expressionType.Source, $"Expecting \"global\", \"script\" or \"constglobal\" but got \"{typeSpan.ToString()}\"!");
                }
            }
        }

        private void addNamedNode(NodeNamed node)
        {
            NodeNamed? existing = _ast.Get(node.Name.ToString());
            if (existing is null)
            {
                _ast.Add(node);
            } else if (existing is Script oldScript && node is Script newScript)
            {
                if (oldScript.ReturnValueType != newScript.ReturnValueType)
                    throw new UnexpectedExpression(node.Source.Source, "Script return types don't match!");
                if (newScript.Type == ScriptType.Stub && oldScript.Type == ScriptType.Static)
                    return; // ignore the stub
                if (newScript.Type == ScriptType.Static && oldScript.Type == ScriptType.Stub)
                {
                    _ast.Add(node);
                    return;
                }
                throw new UnexpectedExpression(node.Source.Source, "Only stub and static scripts can be overloaded");
            } else
            {
                throw new UnexpectedExpression(node.Source.Source, "Invalid name overload");
            }
        }

        private AST.Script buildScript(ref expressionReader reader)
        {
            Debug.Assert(reader.Expression.Values[0].Source.Contents == "script");
            if (reader.Expression.Values.Count < 4)
                throw new InvalidExpression(reader.Expression.Source, "Too short to be a script!");
            Atom typeAtom = reader.NextExpectAtom("Expecting an atom for script type!");
            ScriptType type = typeAtom.Source.Span.ParseScriptType();
            if (type == ScriptType.Invalid)
                throw new InvalidExpression(typeAtom.Source, $"Invalid script type \"{typeAtom.Source.Contents}\"!");
            switch (type)
            {
                case ScriptType.Continuous:
                case ScriptType.Dormant:
                case ScriptType.Startup:
                case ScriptType.CommandScript:
                {
                        Atom name = reader.NextExpectAtom("Expecting an atom for script name!");
                        return new Script(reader.Expression, type, buildAtom(name), buildCodeList(ref reader));
                    }
                case ScriptType.Stub:
                case ScriptType.Macro:
                case ScriptType.Static:
                    {
                        Atom returnTypeAtom = reader.NextExpectAtom("Expecting an atom for script return type!");
                        Atom name = reader.NextExpectAtom("Expecting an atom for script name!");
                        List<(AST.ValueType type, string name)>? arguments = null;
                        if (type == ScriptType.Macro)
                        {
                            Expression argumentsExpression = reader.NextExpectExpression("Expecting an arguments expression");
                            if (argumentsExpression.Values.Count % 2 != 0)
                                throw new InvalidExpression(argumentsExpression.Source, "Invalid arguments expression");
                            arguments = new();
                            expressionReader argumentsReader = new(argumentsExpression);
                            while (argumentsReader.Next() is Value next)
                            {
                                Atom valueAtom = next.ExpectAtom("");
                                Atom nameAtom = argumentsReader.NextExpectAtom("");
                                (AST.ValueType type, string name) arg = new(valueAtom.Value.ParseValueType(), nameAtom.Value);
                                arguments.Add(arg);
                            }
                        }
                        return new Script(reader.Expression, type, buildAtom(name), buildCodeList(ref reader), returnTypeAtom.Value.ParseValueType(), arguments);
                    }
                default:
                    throw new Exception("unreachable");
            }
        } 

        private LinkedList<AST.Value> buildCodeList(ref expressionReader reader)
        {
            LinkedList<AST.Value> list = new();
            for (int i = reader.Index; i < reader.Expression.Values.Count; i++)
            {
                Value currentValue = reader.Expression.Values[i];
                AST.Value value;
                if (i + 1 == reader.Expression.Values.Count && currentValue is Atom returnValue)
                    value = new(returnValue, buildAtom(returnValue));
                else
#pragma warning disable CS8604 // Possible null reference argument.
                    value = new(currentValue as Expression, buildCode(currentValue, null));
#pragma warning restore CS8604 // Possible null reference argument.
                list.AddLast(value);
            }
            return list;
        }

        private AST.Global buildGlobal(Expression expression, bool isConst)
        {
            if (expression.Values.Count != 4)
                throw new InvalidExpression(expression.Source, "Excepting a expression in the format \"(global <type> <name> <value>)\"!");

            Atom typeAtom = expression.Values[1].ExpectAtom("Expecting an atom for global type!");
            Atom nameAtom = expression.Values[2].ExpectAtom("Expecting an atom for global name!");

            Debug.Assert(expression.Values[0].Source.Contents == "global" || expression.Values[0].Source.Contents == "constglobal");

            AST.ValueType type = new (typeAtom.Value);
            AST.Global global = new(expression, buildAtom(nameAtom), type, buildValue(expression.Values[3]));
            global.IsConst = isConst;
            return global;
        }

        private AST.Value buildValue(Value value, AST.Atom? parentExpressionType = null)
        {
            if (value is Atom atom)
                return new(atom, buildAtom(atom));
            if (value is Expression expression)
                return new(expression, buildCode(expression, parentExpressionType));
            throw new InvalidOperationException();
        }

        private AST.Code buildCode(Value value, AST.Atom? parentExpressionType)
        {
            if (value is not Expression expression)
                throw new UnexpectedAtom(value.Source, "Expected code, got atom!");
            if (expression.Values.Count == 0)
                throw new UnexpectedExpression(expression.Source, "Unexpected empty expression!");

            expressionReader reader = new(expression);
            AST.Atom name;
            if (parentExpressionType is not null && parentExpressionType.ToSpan().SequenceEqual("cond"))
                name = new AST.Atom("if");
            else
                name = buildAtom(reader.NextExpectAtom("Expecting an atom for name!"));
            LinkedList<AST.Value> arguments = new();
            for (int i = reader.Index; i < expression.Values.Count; i++)
                arguments.AddLast(buildValue(expression.Values[i], name));
            return new Code(expression, name, arguments);
        }

        /// <summary>
        /// Build a AST atom from a parser atom
        /// </summary>
        /// <param name="atom"></param>
        /// <returns></returns>
        private AST.Atom buildAtom(Atom atom)
        {
            return new(atom);
        }

        /// <summary>
        /// Parse all source files into expressions
        /// </summary>
        private void parseExpressions()
        {
            // import the primary file
            importSourceFile(_mainFile);
            // import files through import directives
            handleImportDirectives();

            // we are done, lock the expressions!
            _parsed.Done();
        }

        /// <summary>
        /// Parse all import directives
        /// </summary>
        private void handleImportDirectives()
        {
            ReadOnlySpan<char> importSpan = "import".AsSpan();

            foreach (Expression expression in _parsed.Expressions)
            {
                if (expression.Values.Count == 0)
                    continue;
                if (expression.Values[0] is not Atom type)
                    continue;
                if (!type.Span.SequenceEqual(importSpan))
                    continue;

                // so the expression is an import directive
                if (expression.Values.Count != 2)
                    throw new InvalidExpression(expression.Source, "Excepting a expression in the format \"(import <filename>)\"!");

                if (expression.Values[1] is not Atom fileNameAtom)
                    throw new InvalidExpression(expression.Values[1].Source, "Filename should be an atom not an expression!");

                string fileName = fileNameAtom.Source.Contents;
                importSourceFile(fileName, expression); // add the new expressions (if any)
            }
        }

        /// <summary>
        /// Import a source file and add it to the index
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceExpression">Expression that caused the file to get added<</param>
        private void importSourceFile(string fileName, Expression? sourceExpression = null)
        {
            // only import each file once (maybe this should emit a warning?)
            if (_files.ContainsKey(fileName))
                return;
            SourceFile file = addSourceFile(fileName, sourceExpression);
            parseSourceFile(file);
        }

        private void parseSourceFile(SourceFile file)
        {
            ExpressionParser parser = new(_parsed, file);
            parser.Parse();
        }

        /// <summary>
        /// Add a source file to the source file index. Contents are read from disk.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="sourceExpression">Expression that caused the file to get added</param>
        /// <returns>The source file object</returns>
        private SourceFile addSourceFile(string fileName, Expression? sourceExpression)
        {
            string fsPath = Path.Combine(_directory, fileName);
            return addSourceFile(fileName, File.ReadAllText(fsPath), sourceExpression);
        }

        /// <summary>
        /// Add a source file and its data to the source file index
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="data">File contents</param>
        /// <param name="sourceExpression">Expression that caused the file to get added</param>
        /// <returns>The source file object</returns>
        private SourceFile addSourceFile(string fileName, string data, Expression? sourceExpression)
        {
            SourceFile file = new (Data: data, FileName: fileName, SourceExpression: sourceExpression);
            _files[fileName] = file;
            return file;
        }

        public AST.AST Ast => _ast;

        private readonly ParsedExpressions _parsed = new();

        private readonly Dictionary<string, SourceFile> _files = new();

        private readonly string _directory;

        private readonly string _mainFile;

        private readonly AST.AST _ast = new();
        
    }
}

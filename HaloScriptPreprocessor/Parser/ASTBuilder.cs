/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HaloScriptPreprocessor.Parser
{
    class ASTBuilder
    {
        /// <summary>
        /// Helper struct for parsing <c>Expression</c>'s  
        /// </summary>
        private struct expressionReader
        {
            public expressionReader(Expression expression)
            {
                Expression = expression;
                _index = 0;
            }

            /// <summary>
            /// Get the next value or null
            /// </summary>
            /// <returns></returns>
            public Value? Next()
            {
                if (_index == Expression.Values.Count)
                    return null;
                return Expression.Values[_index++];
            }

            /// <summary>
            /// Is the end of expression/stream reached?
            /// </summary>
            /// <returns></returns>
            public bool IsEoS()
            {
                return _index == Expression.Values.Count;
            }

            /// <summary>
            /// Get the next value or throws if EoS
            /// </summary>
            /// <param name="error">Error message to show</param>
            /// <returns>Value</returns>
            /// <exception cref="UnexpectedExpression"></exception>
            private Value NextExpectValue(string message)
            {
                if (Next() is Value value)
                    return value;
                else
                    throw new UnexpectedExpression(Expression.Source, "Unexpected end of expression: " + message);
            }

            /// <summary>
            /// Get an <c>Expression</c> throws an exception if the next <c>Value</c> is an atom or null.
            /// </summary>
            /// <param name="error">Error message to show</param>
            /// <returns>Atom</returns>
            /// <exception cref="UnexpectedExpression"></exception>
            public Expression NextExpectExpression(string message)
            {
                return NextExpectValue(message).ExpectExpression(message);
            }

            /// <summary>
            /// Get an <c>Atom</c> throws an exception if the next <c>Value</c> is an expressions or null.
            /// </summary>
            /// <param name="error">Error message to show</param>
            /// <returns>Atom</returns>
            /// <exception cref="UnexpectedExpression"></exception>
            /// <exception cref="UnexpectedAtom"></exception>
            public Atom NextExpectAtom(string message)
            {
                return NextExpectValue(message).ExpectAtom(message);
            }

            public int Index => _index;

            public readonly Expression Expression;
            private int _index;
        }

        public ASTBuilder(IFileSystem fileSystem, string directory, AST.AST ast, Error.Reporting reporting)
        {
            _fileSystem = fileSystem;
            _directory = directory;
            _ast = ast;
            _reporting = reporting;
        }

        /// <summary>
        /// Import a file and all files referenced using <c>(import "filename")</c>
        /// </summary>
        /// <param name="fileName"></param>
        public bool Import(string fileName)
        {
            _addedNamed.Clear();

            // parse expressions
            parseExpressions(fileName);
            if (_reporting.HasFatalErrors)
                return false;

            return BuildAndResolve();
        }

        /// <summary>
        /// Import a file and all files referenced using <c>(import "filename")</c>
        /// </summary>
        /// <param name="file"></param>
        public bool Import(IFileSystem.IFile file)
        {
            _addedNamed.Clear();

            // parse expressions
            parseExpressions(file);
            if (_reporting.HasFatalErrors)
                return false;

            return BuildAndResolve();
        }

        /// <summary>
        /// Resolve names to AST nodes if possible
        /// </summary>
        private void resolve()
        {
            foreach (AST.NodeNamed rootNode in _addedNamed)
            {
                if (rootNode is Global global)
                    resolveValue(global.Value, global);
                if (rootNode is Script script)
                {
                    foreach (AST.Value value in script.Codes)
                        resolveValue(value, script);
                }
            }
        }

        /// <summary>
        /// Resolve names inside a value to AST nodes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parent"></param>
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
            }
            else if (value.Content.IsT1)
            {
                resolveCode(value.Content.AsT1, value);
            }
        }

        /// <summary>
        /// Resolve names inside a code to AST nodes
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parent"></param>
        private void resolveCode(AST.Code code, AST.Node parent)
        {
            code.ParentNode = parent;
            NodeNamed? resolved = _ast.Get(code.Function.AsT0.ToString()); // todo, make this use span not a string
            if (resolved is Global global)
            {
                _reporting.Report(Error.Level.Error, code.Function.AsT0.Source.Source, $"Global \"{global.Name.ToString()}\" is being used as a function!");
            }
            else if (resolved is Script script)
                code.Function = script;
            foreach (AST.Value arg in code.Arguments)
                resolveValue(arg, code);
        }

        /// <summary>
        /// Build AST and resolve names within it
        /// </summary>
        /// <returns>Whatever everything succeeded</returns>
        private bool BuildAndResolve()
        {
            try
            {
                // build AST from expressions
                build();
                // Resolve the expressions
                resolve();
            } catch (UnexpectedExpression ex)
            {
                _reporting.Report(Error.Level.Error, ex.Expression, "Unexpected expression:" + ex.Message);
            } catch (UnexpectedAtom ex)
            {
                _reporting.Report(Error.Level.Error, ex.Expression, "Unexpected atom:" + ex.Message);
            } catch (InvalidExpression ex)
            {
                _reporting.Report(Error.Level.Error, ex.Expression, "Invalid expression:" + ex.Message);
            }

            return !_reporting.HasFatalErrors;
        }

        /// <summary>
        /// Build an AST from expressions
        /// </summary>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
        private void build()
        {
            ReadOnlySpan<char> globalSpan = "global".AsSpan();
            ReadOnlySpan<char> constglobalSpan = "constglobal".AsSpan();
            ReadOnlySpan<char> scriptSpan = "script".AsSpan();
            ReadOnlySpan<char> importSpan = "import".AsSpan();

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
                }
                else if (typeSpan.SequenceEqual(scriptSpan))
                {
                    // build script AST
                    addNamedNode(buildScript(ref reader));
                }
                else if (typeSpan.SequenceEqual(constglobalSpan))
                {
                    // build constant global AST
                    addNamedNode(buildGlobal(expression, isConst: true));
                }
                else if (typeSpan.SequenceEqual(importSpan))
                {
                    continue; // imports are already handled in parseExpressions
                }
                else
                {
                    throw new UnexpectedExpression(expressionType.Source, $"Expecting \"global\", \"script\" or \"constglobal\" but got \"{typeSpan.ToString()}\"!");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="UnexpectedExpression"></exception>
        private void addNamedNode(NodeNamed node)
        {
            NodeNamed? existing = _ast.Get(node.Name.ToString());
            if (existing is null)
            {
                _addedNamed.Add(node);
                _ast.Add(node);
            }
            else if (existing is Script oldScript && node is Script newScript)
            {
                if (oldScript.ReturnValueType != newScript.ReturnValueType)
                    throw new UnexpectedExpression(node.Source.Source, "Script return types don't match!");
                if (newScript.Type == ScriptType.Stub && oldScript.Type == ScriptType.Static)
                    return; // ignore the stub
                if (newScript.Type == ScriptType.Static && oldScript.Type == ScriptType.Stub)
                {
                    _addedNamed.Add(node);
                    _ast.Add(node);
                    return;
                }
                throw new UnexpectedExpression(node.Source.Source, "Only stub and static scripts can be overloaded");
            }
            else
            {
                throw new UnexpectedExpression(node.Source.Source, "Invalid name overload");
            }
        }

        /// <summary>
        /// Build a userscript from expressions
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
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
                        List<(AST.ValueType type, AST.Atom name)>? arguments = null;
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
                                (AST.ValueType type, AST.Atom name) arg = new(valueAtom.Value.ParseValueType(), buildAtom(nameAtom));
                                arguments.Add(arg);
                            }
                        }
                        return new Script(reader.Expression, type, buildAtom(name), buildCodeList(ref reader), returnTypeAtom.Value.ParseValueType(), arguments);
                    }
                default:
                    throw new Exception("unreachable");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
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

        /// <summary>
        /// Build an AST global
        /// </summary>
        /// <param name="expression">Expression to build from</param>
        /// <param name="isConst">Is this a constant global?</param>
        /// <returns>The global</returns>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
        private AST.Global buildGlobal(Expression expression, bool isConst)
        {
            if (expression.Values.Count != 4)
                throw new InvalidExpression(expression.Source, "Excepting a expression in the format \"(global <type> <name> <value>)\"!");

            Atom typeAtom = expression.Values[1].ExpectAtom("Expecting an atom for global type!");
            Atom nameAtom = expression.Values[2].ExpectAtom("Expecting an atom for global name!");

            Debug.Assert(expression.Values[0].Source.Contents == "global" || expression.Values[0].Source.Contents == "constglobal");

            AST.ValueType type = new(typeAtom.Value);
            AST.Global global = new(expression, buildAtom(nameAtom), type, buildValue(expression.Values[3]));
            global.IsConst = isConst;
            return global;
        }

        /// <summary>
        /// Build an AST value from Expressions
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentExpressionType"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
        private AST.Value buildValue(Value value, AST.Atom? parentExpressionType = null)
        {
            if (value is Atom atom)
                return new(atom, buildAtom(atom));
            if (value is Expression expression)
                return new(expression, buildCode(expression, parentExpressionType));
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Build code AST from expression
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentExpressionType">Parent expression function if any</param>
        /// <returns></returns>
        /// <exception cref="UnexpectedExpression"></exception>
        /// <exception cref="UnexpectedAtom"></exception>
        /// <exception cref="InvalidExpression"></exception>
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
        private void parseExpressions(string sourceFile)
        {
            // import the primary file
            importSourceFile(sourceFile);
            // import files through import directives
            handleImportDirectives();

            // we are done, lock the expressions!
            _parsed.Done();
        }

        private void parseExpressions(IFileSystem.IFile file)
        {
            // import the primary file
            importMainFile(file);
            // import files through import directives
            handleImportDirectives();

            // we are done, lock the expressions!
            _parsed.Done();
        }

        /// <summary>
        /// Parse all import directives
        /// </summary>
        /// <exception cref="InvalidExpression"> thrown if an invalid import directive is encountered </exception>
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
                {
                    _reporting.Report(Error.Level.Error, expression.Source, "Excepting a expression in the format \"(import <filename>)\"!");
                    continue;
                }

                if (expression.Values[1] is not Atom fileNameAtom)
                {
                    _reporting.Report(Error.Level.Error, expression.Values[1].Source, "Filename should be an atom not an expression!");
                    continue;
                }

                string fileName = fileNameAtom.Source.Contents;
                // add the new expressions (if any)
                if (!importSourceFile(fileName, expression))
                    _reporting.Report(Error.Level.Error, expression.Source, $"Failed to import: {fileName}");
            }
        }

        /// <summary>
        /// Import a source file and add it to the index
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceExpression">Expression that caused the file to get added<</param>
        /// <returns>Whatever the file was parsed successfully</returns>
        private bool importSourceFile(string fileName, Expression? sourceExpression = null)
        {
            // only import each file once (maybe this should emit a warning?)
            if (_files.ContainsKey(fileName))
                return true;
            if (_fileSystem.GetFile(_directory + _fileSystem.DirectorySeparator + fileName) is IFileSystem.IFile file)
            {
                return parseSourceFile(addSourceFile(file, sourceExpression));
            }
            else
            {
                if (sourceExpression is null)
                    _reporting.Report(Error.Level.Error, $"\"{fileName}\" was not found!");
                else
                    _reporting.Report(Error.Level.Error, sourceExpression.Source, $"\"{fileName}\" was not found!");
                return false;
            }
        }

        private bool importMainFile(IFileSystem.IFile file)
        {
            return parseSourceFile(addSourceFile(file));
        }

        /// <summary>
        /// Parse a <c>SourceFile</c>
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Whatever the file was parsed successfully</returns>
        private bool parseSourceFile(SourceFile file)
        {
            ExpressionParser parser = new(_parsed, file);
            try
            {
                parser.Parse();
                return true;
            } catch (UnexpectedAtom ex)
            {
                _reporting.Report(Error.Level.Error, ex.Expression, "Unexpected Atom: " + ex.Message);
            } catch (UnterminatedElement ex)
            {
                _reporting.Report(Error.Level.Error, new FileSourceLocation(ex.SourceLocation, file), "Unterminated element: " + ex.Message);
            } catch (UnexpectedCharactrerError ex)
            {
                _reporting.Report(Error.Level.Error, new FileSourceLocation(ex.SourceLocation, file), "Unexpected character: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Add a source file and its data to the source file index
        /// </summary>
        /// <param name="file">File object</param>
        /// <param name="sourceExpression">Expression that caused the file to get added</param>
        /// <returns>The source file object</returns>
        private SourceFile addSourceFile(IFileSystem.IFile file, Expression? sourceExpression = null)
        {
            SourceFile sourceFile = new(Data: file.Contents, FileName: file.Name, SourceExpression: sourceExpression);
            _files[file.Name] = sourceFile;
            return sourceFile;
        }

        public AST.AST Ast => _ast;

        private readonly ParsedExpressions _parsed = new();

        private readonly Dictionary<string, SourceFile> _files = new();

        private readonly AST.AST _ast;

        /// <summary>
        /// Nodes added during the last call to 
        /// </summary>
        private readonly List<AST.NodeNamed> _addedNamed = new();

        private readonly IFileSystem _fileSystem;
        private readonly string _directory;

        readonly private Error.Reporting _reporting;

    }
}

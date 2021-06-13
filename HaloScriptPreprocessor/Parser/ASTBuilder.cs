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
        public ASTBuilder(string directory, string mainFile)
        {
            _directory = directory;
            _mainFile = mainFile;
            // parse expressions
            parseExpressions();
            // build AST from expressions
            build();
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
                if (expression.Values[0] is not Atom expressionType)
                    throw new UnexpectedExpression(expression.Values[0].Source, "Expecting \"global\", \"script\" or \"constglobal\" but got an expression!");
                ReadOnlySpan<char> typeSpan = expressionType.Source.Span;
                if (typeSpan.SequenceEqual(globalSpan))
                {
                    // build global AST
                    _ast.UserGlobals.Add(buildGlobal(expression));
                } else if (typeSpan.SequenceEqual(scriptSpan))
                {
                    // build script AST
                    _ast.UserFunctions.Add(buildScript(expression));
                } else if (typeSpan.SequenceEqual(constglobalSpan))
                {
                    // build constant global AST
                    _ast.ConstantGlobals.Add(buildGlobal(expression));
                } else if (typeSpan.SequenceEqual(importSpan))
                {
                    continue; // imports are already handled in parseExpressions
                } else
                {
                    throw new UnexpectedExpression(expressionType.Source, $"Expecting \"global\", \"script\" or \"constglobal\" but got \"{typeSpan.ToString()}\"!");
                }
            }
        }

        private AST.Script buildScript(Expression expression)
        {
            Debug.Assert(expression.Values[0].Source.Contents == "script");
            if (expression.Values.Count < 4)
                throw new InvalidExpression(expression.Source, "Too short to be a script!");
            if (expression.Values[1] is not Atom typeAtom)
                throw new InvalidExpression(expression.Values[1].Source, "Expecting an atom for script type!");
            ScriptType type = typeAtom.Source.Span.ParseScriptType();
            if (type == ScriptType.Invalid)
                throw new InvalidExpression(expression.Values[1].Source, $"Invalid script type \"{typeAtom.Source.Contents}\"!");
            switch (type)
            {
                case ScriptType.Continuous:
                case ScriptType.Dormant:
                case ScriptType.Startup:
                case ScriptType.CommandScript:
                {
                        if (expression.Values[2] is not Atom name)
                            throw new InvalidExpression(expression.Values[1].Source, "Expecting an atom for script name!");
                        return new AST.Script(expression, type, buildAtom(name), buildCodeList(expression, 3));
                    }
                case ScriptType.Stub:
                case ScriptType.Static:
                    {

                        if (expression.Values[3] is not Atom name)
                            throw new InvalidExpression(expression.Values[1].Source, "Expecting an atom for script name!");
                        return new AST.Script(expression, type, buildAtom(name), buildCodeList(expression, 4));
                    }
                default:
                    throw new Exception("todo");
            }
        }

        private LinkedList<AST.Code> buildCodeList(Expression expression, int offset)
        {
            LinkedList<AST.Code> list = new();
            for (int i = offset; i < expression.Values.Count; i++)
                list.AddLast(buildCode(expression.Values[i]));
            return list;
        }

        private AST.Global buildGlobal(Expression expression)
        {
            if (expression.Values.Count != 4)
                throw new InvalidExpression(expression.Source, "Excepting a expression in the format \"(global <type> <name> <value>)\"!");
            if (expression.Values[1] is not Atom typeAtom)
                throw new InvalidExpression(expression.Values[1].Source, "Expecting an atom for global type!");
            if (expression.Values[2] is not Atom nameAtom)
                throw new InvalidExpression(expression.Values[1].Source, "Expecting an atom for global name!");

            Debug.Assert(expression.Values[0].Source.Contents == "global" || expression.Values[0].Source.Contents == "constglobal");


            AST.ValueType type = new (typeAtom.Value);
            AST.Global global = new(expression, buildAtom(nameAtom), type, buildValue(expression.Values[3]));
            return global;
        }

        private AST.Value buildValue(Value value)
        {
            if (value is Atom atom)
                return new(atom, buildAtom(atom));
            if (value is Expression expression)
                return new(expression, buildCode(expression));
            throw new InvalidOperationException();
        }

        private AST.Code buildCode(Value value)
        {
            if (value is not Expression expression)
                throw new UnexpectedAtom(value.Source, "Expected code, got atom!");
            if (expression.Values.Count == 0)
                throw new UnexpectedExpression(expression.Source, "Unexpected empty expression!");
            if (expression.Values[0] is not Atom name)
                throw new InvalidExpression(expression.Values[0].Source, "Expecting an atom for name!");
            LinkedList<AST.Value> arguments = new();
            for (int i = 1; i < expression.Values.Count; i++)
                arguments.AddLast(buildValue(expression.Values[i]));
            return new AST.Code(expression, buildAtom(name), arguments);
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

        private readonly ParsedExpressions _parsed = new();

        private readonly Dictionary<string, SourceFile> _files = new();

        private readonly string _directory;

        private readonly string _mainFile;

        private readonly AST.AST _ast = new();
    }
}

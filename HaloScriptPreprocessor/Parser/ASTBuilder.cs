using System;
using System.Collections.Generic;
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
            build();
        }

        private void build()
        {
            ReadOnlySpan<char> globalSpan      = "global".AsSpan();
            ReadOnlySpan<char> constglobalSpan = "constglobal".AsSpan();
            ReadOnlySpan<char> scriptSpan      = "script".AsSpan();
            foreach (Expression expression in _parsed.Expressions)
            {
                if (expression.Values.Count == 0)
                    throw new UnexpectedExpression(expression.Source, "Unexpected empty expression!");
                Value expressionType = expression.Values[0];
                if (expressionType is not Atom)
                    throw new UnexpectedExpression(expressionType.Source, "Expecting \"global\", \"script\" or \"constglobal\" but got an expression!");
                ReadOnlySpan<char> typeSpan = (expressionType as Atom).Source.Span;
                if (typeSpan.SequenceEqual(globalSpan))
                {
                    // build global AST
                } else if (typeSpan.SequenceEqual(scriptSpan))
                {
                    // build script AST
                } else if (typeSpan.SequenceEqual(constglobalSpan))
                {
                    // build constant global AST
                } else
                {
                    // error
                }
            }
        }

        private void buildScript(Expression expression)
        {
            //ReadOnlySpan<char> 
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
            List<Expression> removeList = new();
            foreach (Expression expression in _parsed.Expressions)
            {
                if (expression.Values.Count == 0)
                    continue;
                if (expression.Values[0] is not Atom)
                    continue;
                Atom type = expression.Values[0] as Atom;
                if (!type.Span.SequenceEqual(importSpan))
                    continue;

                // so the expression is an import directive
                if (expression.Values.Count != 2)
                    throw new InvalidExpression(expression.Source, "Excepting a expression in the format \"(import <filename>)\"!");

                if (expression.Values[1] is not Atom fileNameAtom)
                    throw new InvalidExpression(expression.Values[1].Source, "Filename should be an atom not an expression!");

                string fileName = fileNameAtom.Source.Contents;
                importSourceFile(fileName, expression); // add the new expressions (if any)
                removeList.Add(expression); // remember to remove this later
            }

            foreach (Expression expression in removeList)
                _parsed.RemoveExpression(expression);
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

        private Dictionary<string, SourceFile> _files;

        private string _directory;

        private string _mainFile;
    }
}

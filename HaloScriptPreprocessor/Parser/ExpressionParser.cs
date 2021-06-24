using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HaloScriptPreprocessor.Parser
{
    public class ExpressionParser
    {
        public ExpressionParser(ParsedExpressions parsedExpressions, SourceFile sourceFile)
        {
            _parsedExpressions = parsedExpressions;
            _sourceFile = sourceFile;
        }

        #region implementation

        public void Parse()
        {
            (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)? token;
            while ((token = nextToken()) is not null)
            {
                var type = token.Value.Type;
                if (type == TokenType.LeftBracket)
                {
                    Expression expression = new(CreatePartialSource(token.Value.Source.AsT1));
                    if (stackLength() == 0)
                        _parsedExpressions.AddExpression(expression);
                    pushExpression(expression);
                }
                else if (type == TokenType.RightBracket)
                {
                    if (stackLength() == 0)
                        throw new UnexpectedCharactrerError(token.Value.Source.AsT1, "Unexpected \"(\", no preceeding \"(\" to close");
#pragma warning disable CS8602
                    _currentExpression.Source.setEnd(token.Value.Source.AsT1);
#pragma warning restore CS8602
                    popExpression();
                }
                if (type == TokenType.Atomic || type == TokenType.AtomicQuote)
                {
                    ExpressionSource source = token.Value.Source.AsT0;
                    if (stackLength() == 0)
                        throw new UnexpectedAtom(source, $"Atomic expression \"{source.Contents}\" is not allowed as a top level expression");
                    pushAtom(new Atom(source, type == TokenType.AtomicQuote));
                }
            }

            if (_currentExpression is not null)
                throw new UnterminatedElement(_currentExpression.Source.Start, "Expression is not terminated!");
        }

        private void pushAtom(Atom atom)
        {
            Debug.Assert(_currentExpression is not null);
            _currentExpression.Values.Add(atom);
        }

        /// <summary>
        /// Push an expression onto the expression stack
        /// </summary>
        /// <param name="expression">Expression to push</param>
        private void pushExpression(Expression expression)
        {
            if (_currentExpression is not null)
            {
                _currentExpression.Values.Add(expression);
                _expressionsStack.Push(_currentExpression);
            }
            _currentExpression = expression;
        }

        /// <summary>
        /// Pop an expression from the expression stack
        /// </summary>
        private void popExpression()
        {
            Debug.Assert(_currentExpression is not null);
            if (_expressionsStack.Count > 0)
                _currentExpression = _expressionsStack.Pop();
            else
                _currentExpression = null;
        }

        /// <summary>
        /// Get expression stack length
        /// </summary>
        /// <returns>Number of stack entries</returns>
        private int stackLength()
        {
            Debug.Assert(_currentExpression is not null || _expressionsStack.Count == 0);
            if (_currentExpression is null)
                return 0;
            else
                return _expressionsStack.Count + 1;
        }

        private enum TokenType
        {
            Atomic,
            AtomicQuote,
            LeftBracket,
            RightBracket,
        }

#pragma warning disable CS8604

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)? nextToken()
        {
            string data = _sourceFile.Data;

            // token stuff
            SourceLocation? tokenStart = null; // used for multi character unquoted tokens
            SourceLocation? quoteStartLocation = null; // used for quoted tokens

            // helpers

            Func<bool> inToken = () => tokenStart is not null;
            Func<bool> inQuoteToken = () => quoteStartLocation is not null;
            Func<SourceLocation, SourceLocation, (TokenType, OneOf<ExpressionSource, SourceLocation>)> endToken = (start, end) =>
            {
                return (inQuoteToken() ? TokenType.AtomicQuote : TokenType.Atomic, CreateSource(start, end));
            };

            Func<bool, (TokenType, OneOf<ExpressionSource, SourceLocation>)> handleBracket = (bool left) =>
            {
                SourceLocation currentLocation = getLocation();
                if (inToken())
                {
                    // roll back the offset, and return the token, the next invocation will return the bracket
                    // this only handles the case of the bracket being immediately after the token, other cases are handles elsewhere
                    _currentOffset -= 1;
                    _currentColunm -= 1;
                    return endToken(tokenStart.Value, currentLocation);
                }
                else
                {
                    if (left)
                        return (TokenType.LeftBracket, currentLocation);
                    else
                        return (TokenType.RightBracket, new SourceLocation(_currentOffset + 1, _currentLine, _currentColunm + 1));
                }
            };

            while (_currentOffset + 1 != data.Length)
            {
                // get next character
                _currentOffset += 1;
                char next = data[_currentOffset];

                _currentColunm += 1;

                SourceLocation newlineStart;
                switch (next)
                {
                    case '\r':
                        if (_currentOffset + 1 == data.Length)
                            throw new UnexpectedCharactrerError(
                                new SourceLocation(_currentOffset + 1, _currentLine, _currentColunm + 1),
                                "Unexpected EOF after \\r/Carriage Return character, not a valid newline!");
                        if (data[_currentOffset + 1] != '\n')
                            throw new UnexpectedCharactrerError(
                                new SourceLocation(_currentOffset + 1, _currentLine, _currentColunm + 1),
                                "Unexpected character after \\r/Carriage Return character, not a valid newline!");
                        newlineStart = getLocation();
                        _currentOffset += 1;
                        _currentLine += 1;
                        _currentColunm = 0;
                        _inComment = false;
                        if (inToken())
                            return endToken(tokenStart.Value, newlineStart);
                        continue;
                    case '\n':
                        newlineStart = getLocation();
                        _currentLine += 1;
                        _currentColunm = 0;
                        _inComment = false;
                        if (inToken())
                            return endToken(tokenStart.Value, newlineStart);
                        continue;
                    case '"':
                        if (_inComment)
                            continue;
                        // check if the previous character allows a quote to start
                        if (!inQuoteToken() && _currentOffset != 0)
                        {
                            char previous = data[_currentOffset - 1];
                            switch (previous)
                            {
                                case ' ':
                                case '\t':
                                case '(':
                                case '"':
                                    break; // valid previous
                                default:
                                    continue; // not a quote
                            }
                        }
                        if (inToken())
                        {
                            Debug.Fail("TODO: emit a warning!");
                            continue;
                        }
                        if (inQuoteToken())
                        {
                            return endToken(quoteStartLocation.Value, new SourceLocation(_currentOffset + 1, _currentLine, _currentColunm + 1));
                        }
                        else
                        {
                            quoteStartLocation = getLocation();
                            continue;
                        }
                    case ';':
                        if (!inQuoteToken())
                        {
                            _inComment = true;
                            if (inToken())
                                return endToken(tokenStart.Value, getLocation());

                        }
                        continue;
                    case ' ':
                    case '\t':
                        if (!_inComment && inToken())
                            return (TokenType.Atomic, CreateSource(tokenStart.Value, getLocation()));
                        continue;
                    case '(':
                        if (_inComment || inQuoteToken())
                            continue;
                        return handleBracket(true);
                    case ')':
                        if (_inComment || inQuoteToken())
                            continue;
                        return handleBracket(false);
                    default:
                        if (!_inComment && !inToken() && !inQuoteToken())
                            tokenStart = getLocation();
                        continue;
                }
            }
            if (inQuoteToken())
            {
                throw new UnterminatedElement(quoteStartLocation.Value, "quote/string is not terminated!");
            }
            // reached EOF
            return null;
        }

#pragma warning restore CS8604


        /// <summary>
        /// Create a expression source from it's start and end
        /// </summary>
        /// <param name="start">start location</param>
        /// <param name="end">end location</param>
        /// <returns>Complete source for an expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ExpressionSource CreateSource(SourceLocation start, SourceLocation end)
        {
            return new ExpressionSource(_sourceFile, start, end);
        }

        /// <summary>
        /// Create a partial expression source that only has a start set
        /// </summary>
        /// <param name="start">Start location</param>
        /// <returns>Partial expression source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ExpressionSource CreatePartialSource(SourceLocation start)
        {
            return new ExpressionSource(_sourceFile, start);
        }

        /// <summary>
        /// Get the current location
        /// </summary>
        /// <returns>Location</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SourceLocation getLocation()
        {
            return new SourceLocation(_currentOffset, _currentLine, _currentColunm);
        }

        readonly private ParsedExpressions _parsedExpressions;

        readonly private SourceFile _sourceFile;

        // expression stack
        readonly private Stack<Expression> _expressionsStack = new();
        private Expression? _currentExpression = null;

        // tokenizer stuff

        int _currentOffset = -1;
        int _currentLine = 0;
        int _currentColunm = -1;

        bool _inComment = false; // are we currently inside a comment?
        #endregion
    }
}

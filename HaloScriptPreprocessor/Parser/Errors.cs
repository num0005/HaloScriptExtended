/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;

namespace HaloScriptPreprocessor.Parser
{
    class LexerError : Exception
    {
        public LexerError(SourceLocation location, string message) : base(message)
        {
            SourceLocation = location;
        }

        public readonly SourceLocation SourceLocation;
    }
    class UnexpectedCharactrerError : LexerError
    {
        public UnexpectedCharactrerError(SourceLocation location, string message) : base(location, message) { }
    }

    class UnterminatedElement : LexerError
    {
        public UnterminatedElement(SourceLocation location, string message) : base(location, message) { }
    }

    class ParseError : Exception
    {
        public ParseError(ExpressionSource source, string message) : base(message)
        {
            Expression = source;
        }

        public readonly ExpressionSource Expression;
    }

    /// <summary>
    /// Atom when one wasn't expected
    /// </summary>
    class UnexpectedAtom : ParseError
    {
        public UnexpectedAtom(ExpressionSource source, string message) : base(source, message) { }
    }

    /// <summary>
    /// Expression when one wasn't expected
    /// </summary>
    class UnexpectedExpression : ParseError
    {
        public UnexpectedExpression(ExpressionSource source, string message) : base(source, message) { }
    }

    /// <summary>
    /// Expression that was invalid
    /// </summary>
    class InvalidExpression : ParseError
    {
        public InvalidExpression(ExpressionSource source, string message) : base(source, message) { }
    }
}

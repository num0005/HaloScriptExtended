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

    class UnexpectedAtom : ParseError
    {
        public UnexpectedAtom(ExpressionSource source, string message) : base(source, message) { }
    }

    class UnexpectedExpression : ParseError
    {
        public UnexpectedExpression(ExpressionSource source, string message) : base(source, message) { }
    }
}

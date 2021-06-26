using HaloScriptPreprocessor.Parser;
using System;
using Xunit;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneOf;
using static HaloScriptPreprocessor.Parser.ExpressionParser;
using Microsoft.Toolkit.HighPerformance;

namespace HaloScriptPreprocessor.Tests.Parser
{
    public class ExpressionParserTests
    {
        public ExpressionParserTests()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "HaloScriptPreprocessor.Tests.Parser.test.hsc";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                _sourceFile = new(reader.ReadToEnd(), resourceName, null);
            }

            _expressionParser = new(_parsed, _sourceFile);
            @private = new(_expressionParser);
        }

        private readonly ParsedExpressions _parsed = new();
        private readonly SourceFile _sourceFile;
        private readonly ExpressionParser _expressionParser;
        private readonly PrivateObject @private;

        private (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)? nextToken()
        {
            return @private.Invoke("nextToken") as (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)?;
        }



        [Fact]
        public void TokenizerTestComments()
        {
            TokenType[] expectedTokens = {
                TokenType.AtomicQuote,
                TokenType.LeftBracket, TokenType.Atomic, TokenType.Atomic, TokenType.Atomic, TokenType.AtomicQuote, TokenType.RightBracket,
                TokenType.LeftBracket, TokenType.Atomic, TokenType.Atomic, TokenType.Atomic, TokenType.Atomic, TokenType.LeftBracket, TokenType.Atomic, TokenType.Atomic, TokenType.RightBracket,
                TokenType.LeftBracket
            };

            // check comments are handled correctly
            foreach (TokenType expected in expectedTokens)
            {
                (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)? next = nextToken();
                Assert.True(next.HasValue);
                Assert.Equal(expected, next.Value.Type);
            }

            // verify the print isn't excluded
            (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source)? print_token = nextToken();
            Assert.True(print_token.HasValue);
            Assert.Equal(TokenType.Atomic, print_token.Value.Type);
            Assert.Equal("print", print_token.Value.Source.AsT0.Contents);

            for (int i = 0; i < 38; i++)
                Assert.True(nextToken().HasValue);

            Assert.False(nextToken().HasValue);
        }

        [Fact]
        public void TokenizerTestGeneric()
        {
            while (nextToken() is (TokenType Type, OneOf<ExpressionSource, SourceLocation> Source))
            {
                switch (Type)
                {
                    case TokenType.Atomic:
                    case TokenType.AtomicQuote:
                        Assert.IsType<ExpressionSource>(Source.Value);
                        Assert.NotNull(Source.AsT0.End);
                        Assert.NotEmpty(Source.AsT0.Contents);
                        Assert.Equal(Source.AsT0.Span.ToString(), Source.AsT0.Contents);
                        if (Type == TokenType.AtomicQuote)
                            Assert.Equal(2, Source.AsT0.Span.Count('"'));
                        break;
                    case TokenType.LeftBracket:
                        Assert.IsType<SourceLocation>(Source.Value);
                        Assert.Equal('(', _sourceFile.Data[Source.AsT1.Offset]);
                        break;
                    case TokenType.RightBracket:
                        Assert.IsType<SourceLocation>(Source.Value);
                        Assert.Equal(')', _sourceFile.Data[Source.AsT1.Offset]);
                        break;
                }
            }

        }

        [Fact]
        public void Parse_Test()
        {
            _expressionParser.Parse();
            Assert.Null(@private.GetField("_currentExpression"));
            Assert.Equal(3, _parsed.Expressions.Count);
        }

    }
}

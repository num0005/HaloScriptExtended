using HaloScriptPreprocessor.AST;
using System;
using Xunit;

namespace HaloScriptPreprocessor.Tests.AST
{
    public class AtomTests
    {
        public AtomTests()
        {
            _fakeExpressionSource = new Parser.ExpressionSource(_fakeSourceFile, new(0, 1, 1), new(5, 1, 6));
            _ParserAtom = new(_fakeExpressionSource, false);

            _parentAtom = new("parent");
            _atom = new("child", _parentAtom);
            _atomFromParser = new(_ParserAtom, _parentAtom);
        }
        private readonly Parser.SourceFile _fakeSourceFile = new("fake_contents", "fake_name.hsc", null);
        private readonly Parser.ExpressionSource _fakeExpressionSource;

        private readonly Parser.Atom _ParserAtom;
        private readonly Atom _atomFromParser;
        private readonly Atom _atom;
        private readonly Atom _parentAtom;
        [Fact]
        public void ToSpan_StateUnderTest_ExpectedBehavior()
        {
            Assert.True(_atom.ToSpan().SequenceEqual("child"));
            Assert.True(_atomFromParser.ToSpan().SequenceEqual("fake_"));
        }

        [Fact]
        public void ToString_StateUnderTest_ExpectedBehavior()
        {
            Assert.Equal("child", _atom.ToString());
            Assert.Equal("parent", _parentAtom.ToString());
            Assert.Equal("fake_", _atomFromParser.ToString());
        }

        [Fact]
        public void Equals_StateUnderTest_ExpectedBehavior()
        {
            Assert.True(_atom.Equals(_atom));
            Assert.True(_atom.Equals(
                new("child", _parentAtom)));
            Assert.True(_atom.Equals(
                new("child")));
            Assert.False(_atom.Equals(
                new("NotHere")));
            Assert.False(_atom.Equals(null));
            Assert.False(_atom.Equals(_ParserAtom));
            Assert.False(_atom.Equals(_atomFromParser));
            Assert.False(_atom.Equals(_parentAtom));
        }

        [Fact]
        public void Clone_StateUnderTest_ExpectedBehavior()
        {
            Node? parent = null;

            // Act
            var result = _atom.Clone(
                parent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.ParentNode, parent);
            Assert.Equal(result.Source, _atom.Source);
            Assert.Equal(result.IsGenerated, _atom.IsGenerated);
            Assert.Equal(result.IsFromParser, _atom.IsFromParser);
            Assert.Equal(result.IsModified, _atom.IsModified);
            Assert.Equal(result.Value, _atom.Value);
        }

        [Fact]
        public void Rewrite_StateUnderTest_ExpectedBehavior()
        {
            // Act
            _atom.Rewrite(
                null);
        }

        [Fact]
        public void GetHashCode_StateUnderTest_ExpectedBehavior()
        {

            // Act
            var result = _atom.GetHashCode();
            var result1 = _atomFromParser.GetHashCode();
            var result2 = _parentAtom.GetHashCode();

            // Assert
            Assert.Equal(-255048021, result);
            Assert.Equal(1435490861, result1);
            Assert.Equal(2125089963, result2);
            Assert.Equal(result, _atom.GetHashCode());
        }
    }
}

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using Xunit;

namespace HaloScriptPreprocessor.Tests.AST
{
    public class GlobalTests
    {


        public GlobalTests()
        {
            _fakeExpressionSource = new Parser.ExpressionSource(_fakeSourceFile, new(0, 1, 1), new(5, 1, 6));
            _fakeExpression = new(_fakeExpressionSource);

            _global = new(_fakeExpression, new Atom("fake_global"), "string".ParseValueType(), new Value(null, new Atom("a_test_string")));
            _global.IsConst = true;
        }

        private readonly Parser.SourceFile _fakeSourceFile = new("fake_contents", "fake_name.hsc", null);
        private readonly Parser.ExpressionSource _fakeExpressionSource;
        private readonly Parser.Expression _fakeExpression;

        private readonly Global _global;

        [Fact]
        public void Clone_Test()
        {
            Node? parent = null;

            // Act
            Global result = _global.Clone(
                parent);

            Assert.Equal(_global.IsConst, result.IsConst);
            Assert.Equal(_global.Name, result.Name);
            Assert.Equal(_global.NodeCount, result.NodeCount);
            Assert.Equal(_global.ValueType, result.ValueType);
            Assert.Equal(_global.Value.Content.Index, result.Value.Content.Index);
            if (!_global.Value.Content.IsT1)
                Assert.Equal(_global.Value, result.Value);
            Assert.Equal(result, result.Value.ParentNode);
        }

        [Fact]
        public void Rewrite_Test()
        {
            Assert.ThrowsAny<Exception>(() => _global.Rewrite(null));

            var from = new Value(null, new Atom("a_test_string"));
            Assert.Equal(from, _global.Value);
            var to = new Value(null, new Atom("1"));
            var mapping = new Dictionary<Value, Value> { { from, to } };

            _global.Rewrite(mapping);

            Assert.Equal(to, _global.Value);
        }
    }
}

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using SourceFile = HaloScriptPreprocessor.Parser.SourceFile;
using ExpressionSource = HaloScriptPreprocessor.Parser.ExpressionSource;
using Expression = HaloScriptPreprocessor.Parser.Expression;

namespace HaloScriptPreprocessor.Tests.AST
{
    public class CodeTests
    {
        public CodeTests()
        {
            _fakeExpressionSource = new ExpressionSource(_fakeSourceFile, new(0, 1, 1), new(5, 1, 6));
            _fakeExpression = new(_fakeExpressionSource);

            _fakeExpressionSource2 = new ExpressionSource(_fakeSourceFile, new(4, 1, 5), new(7, 1, 8));
            _fakeExpression2 = new(_fakeExpressionSource2);


            _args.AddLast(new Value(null, new Atom("13")));
            _args.AddLast(new Value(null, new Atom("3")));
            _args.AddLast(new Value(null, new Atom("3")));
            _args.AddLast(new Value(null, new Atom("halo")));
            _code = new(_fakeExpression, new Atom("*"), _args);
        }

        private readonly SourceFile _fakeSourceFile = new("fake_contents", "fake_name.hsc", null);
        private readonly ExpressionSource _fakeExpressionSource;
        private readonly ExpressionSource _fakeExpressionSource2;
        private readonly Expression _fakeExpression;
        private readonly Expression _fakeExpression2;

        private readonly LinkedList<Value> _args = new();
        private readonly Code _code;
        [Fact]
        public void SetContents_Test()
        {
            Code other = new Code(_fakeExpression2, new Atom("fake_hsc"), new());

            Assert.ThrowsAny<Exception>(() => _code.SetContents(null));

            // Act
            _code.SetContents(
                other);

            // Assert
            Assert.NotEqual(other.Source, _code.Source);
            Assert.Equal(other.Arguments, _code.Arguments);
            Assert.Equal(other.Function, _code.Function);
            Assert.Equal(new Atom("fake_hsc"), _code.Function);
        }

        [Fact]
        public void Clone_Test()
        {
            Node? parent = null;

            // Act
            var result = _code.Clone(
                parent);

            // Assert
            Assert.Equal(parent, _code.ParentNode);
            Assert.Equal(_code.Source, result.Source);
            Assert.Equal(_code.NodeCount, result.NodeCount);
            Assert.Equal(_code.Function, result.Function);
            Assert.Equal(_code.Arguments.Count, result.Arguments.Count);

            List<Value> codeArgs = _code.Arguments.ToList();
            List<Value> clonedArgs = result.Arguments.ToList();

            foreach (Value value in clonedArgs)
                Assert.Equal(result, value.ParentNode);
            for (int i = 0; i < codeArgs.Count; i++)
            {
                Assert.NotNull(clonedArgs[i]);
                Assert.Equal(codeArgs[i].Source, clonedArgs[i].Source);
                Assert.Equal(codeArgs[i].NodeCount, clonedArgs[i].NodeCount);
                Assert.Equal(codeArgs[i].Content.Index, clonedArgs[i].Content.Index);
                codeArgs[i].Content.Switch(
                    atom => Assert.Equal(atom, clonedArgs[i].Content.AsT0),
                    code =>
                    {
                        Code otherCode = clonedArgs[i].Content.AsT1;
                        Assert.Equal(code.Function, otherCode.Function);
                        Assert.Equal(code.NodeCount, otherCode.NodeCount);
                    },
                    global => Assert.Equal(global, clonedArgs[i].Content.AsT2),
                    script => Assert.Equal(script, clonedArgs[i].Content.AsT3)
                    );
            }
            Assert.Equal(_code.NodeCount, result.NodeCount);
        }

        [Fact]
        public void Rewrite_Test()
        {
            Assert.ThrowsAny<Exception>(() => _code.Rewrite(null));

            var from = new Value(null, new Atom("halo"));
            var to = new Value(null, new Atom("1"));
            var mapping = new Dictionary<Value, Value> { { from, to } };

            _code.Rewrite(mapping);

            Value[] args = _code.Arguments.ToArray();
            Assert.Equal(0, args[3].Content.Index);
            Assert.Equal("1", args[3].Content.AsT0.ToString());
        }

        [Fact]
        public void NodeCount_Test()
        {
            Assert.Equal(5u, _code.NodeCount);
        }

        [Fact]
        public void FunctionSpan_Test()
        {
            Assert.True(_code.FunctionSpan.SequenceEqual("*"));
        }
    }
}

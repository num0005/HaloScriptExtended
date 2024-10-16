﻿/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using HaloScriptPreprocessor.Parser;
using System;
using System.Collections.Generic;
using Xunit;

using Atom = HaloScriptPreprocessor.AST.Atom;
using Value = HaloScriptPreprocessor.AST.Value;

namespace HaloScriptPreprocessor.Tests.AST
{
    public class ScriptTests
    {
        public ScriptTests()
        {
            _fakeExpressionSource = new ExpressionSource(_fakeSourceFile, new(0, 1, 1), new(5, 1, 6));
            _fakeExpression = new(_fakeExpressionSource);
            Atom scriptName = new Atom("fake_static");
            _script = new(_fakeExpression, ScriptType.Static, scriptName, new( new List<Value>{ new Value(null, new Atom("a_test_string")) }), "string".ParseValueType(), null);
            scriptName.ParentNode = _script;

            _ast.Add(_script);
        }

        private readonly HaloScriptPreprocessor.AST.AST _ast = new();
        private readonly SourceFile _fakeSourceFile = new("fake_contents", "fake_name.hsc", null);
        private readonly ExpressionSource _fakeExpressionSource;
        private readonly Expression _fakeExpression;
        private readonly Script _script;

        [Fact]
        public void Clone_Test()
        {
            Node parent = null;

            // Act
            var result = _script.Clone(
                parent);

            // Assert
            Assert.Equal(parent, result.ParentNode);
            Assert.Equal(_script.NodeCount, result.NodeCount);
            Assert.Equal(_script.Name, result.Name);
            Assert.Equal(_script.ReturnValueType, result.ReturnValueType);
            Assert.Equal(_script.Type, result.Type);
        }

        [Fact]
        public void NodeCount_Test()
        {
            Assert.Equal(2u, _script.NodeCount);
        }

        [Fact]
        public void Rewrite_Test()
        {
            Assert.ThrowsAny<Exception>(() => _script.Rewrite(null));

            var from = new Value(null, new Atom("a_test_string"));
            Assert.Equal(from, _script.Codes.First.Value);
            var to = new Value(null, new Atom("1"));
            var mapping = new Dictionary<Value, Value> { { from, to } };

            _script.Rewrite(mapping);

            Assert.Equal(to, _script.Codes.First.Value);
            Assert.Equal(_script, _script.Codes.First.Value.ParentNode);
        }
    }
}

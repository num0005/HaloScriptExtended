﻿/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using HaloScriptPreprocessor.Parser;
using System;
using Xunit;
using Atom = HaloScriptPreprocessor.AST.Atom;

namespace HaloScriptPreprocessor.Tests.AST
{
    public class ASTTests
    {
        public ASTTests()
        {
            _fakeExpressionSource = new ExpressionSource(_fakeSourceFile, new(0, 1, 1), new(5, 1, 6));
            _fakeExpression = new(_fakeExpressionSource);
            Atom scriptName = new Atom("fake_startup");
            _script = new(_fakeExpression, ScriptType.Startup, scriptName, new(), null, null);
            scriptName.ParentNode = _script;

            _ast.Add(_script);
        }

        private readonly HaloScriptPreprocessor.AST.AST _ast = new();
        private readonly SourceFile _fakeSourceFile = new("fake_contents", "fake_name.hsc", null);
        private readonly ExpressionSource _fakeExpressionSource;
        private readonly Expression _fakeExpression;
        private readonly Script _script; 

        [Fact]
        public void IsUserDefinedName_Test()
        {
            Atom nameAtom = new Atom("fake_startup");
            Atom otherNameAtom = new Atom("fake_nonexist");

            Assert.True(_ast.IsUserDefinedName(
                nameAtom));
            Assert.False(_ast.IsUserDefinedName(
                otherNameAtom));
            Assert.ThrowsAny<Exception>(() => _ast.IsUserDefinedName(
                null));
        }

        [Fact]
        public void Add_Test()
        {
            Atom scriptName = new Atom("another_fake");
            Script script = new(_fakeExpression, ScriptType.Dormant, scriptName, new(), null, null);
            _ast.Add(script);
            Assert.NotNull(_ast.Get("another_fake"));
            Assert.Equal(script, _ast.Get("another_fake"));
        }

        [Fact]
        public void Get_Test()
        {
            Assert.NotNull(_ast.Get("fake_startup"));
            Assert.Null(_ast.Get("fake_nonexist"));

            Assert.Equal(_script, _ast.Get("fake_startup"));
        }

        [Fact]
        public void Remove_Test()
        {
            Assert.True(_ast.Remove("fake_startup"));
            Assert.Null(_ast.Get("fake_startup"));
            Assert.False(_ast.Remove("fake_startup"));
            Assert.Null(_ast.Get("fake_startup"));
            Assert.Null(_ast.Get("fake_nonexist"));
            Assert.False(_ast.Remove("fake_nonexist"));
            Assert.Null(_ast.Get("fake_nonexist"));
        }
    }
}

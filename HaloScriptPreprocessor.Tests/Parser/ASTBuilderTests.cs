/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Xunit;

namespace HaloScriptPreprocessor.Tests.Parser
{
    public class ASTBuilderTests
    {
        public ASTBuilderTests()
        {
            _builder = new(testFS, "", new());
            _privateObject = new(_builder);
        }

        TestFileSystem testFS = new();
        private ASTBuilder _builder;
        private PrivateObject _privateObject;

        [Theory]
        [InlineData("HaloScriptPreprocessor.Tests.Interpreter.test.hsc", 7)]
        [InlineData("HaloScriptPreprocessor.Tests.Parser.test.hsc", 3)]
        public void BasicTest(string resourceFile, int namedCount)
        {
            _builder.Import(testFS.GetFile(resourceFile));
            // Assert
            Assert.Equal(namedCount, _builder.Ast.UserNameMapping.Count);
        }
    }
}

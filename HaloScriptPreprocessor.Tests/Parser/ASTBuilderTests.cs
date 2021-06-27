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
        private void Init(string resourceFile)
        {
            string testFile = ResourceHelper.Read(resourceFile);
            _builder = new(testFile);
            _privateObject = new(_builder);
        }
        

        /// <summary>
        /// Create an empty (more or less) ASTBuilder
        /// </summary>
        private void InitEmpty()
        {
            object[] args = { };
            Type[] typeParam = { };
            _privateObject = new(typeof(ASTBuilder), typeParam, args);
            _builder = _privateObject.Target as ASTBuilder;
        }

        private ASTBuilder _builder;
        private PrivateObject _privateObject;

        [Theory]
        [InlineData("HaloScriptPreprocessor.Tests.Interpreter.test.hsc", 7)]
        [InlineData("HaloScriptPreprocessor.Tests.Parser.test.hsc", 3)]
        public void BasicTest(string resourceFile, int namedCount)
        {
            // Act
            Init(resourceFile);

            // Assert
            Assert.Equal(namedCount, _builder.Ast.UserNameMapping.Count);
        }
    }
}

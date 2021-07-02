/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.Interpreter;
using Xunit;
using System.Linq;

namespace HaloScriptPreprocessor.Tests.Interpreter
{
    public class InterpreterTests
    {
        public InterpreterTests()
        {
            TestFileSystem testFS = new();
            HaloScriptPreprocessor.Parser.ASTBuilder builder = new(testFS, "", _ast, new());
            builder.Import(testFS.GetFile("HaloScriptPreprocessor.Tests.Interpreter.test.hsc"));
            _interpreter = new(builder.Ast);
        }
        private readonly HaloScriptPreprocessor.AST.AST _ast = new();
        private readonly HaloScriptPreprocessor.Interpreter.Interpreter _interpreter;

        private HaloScriptPreprocessor.AST.Global GetGlobal(string name)
        {
            var global = _ast.Get(name);
            Assert.IsType<HaloScriptPreprocessor.AST.Global>(global);
            return global as HaloScriptPreprocessor.AST.Global;
        }

        private HaloScriptPreprocessor.AST.Script GetScript(string name)
        {
            var script = _ast.Get(name);
            Assert.IsType<HaloScriptPreprocessor.AST.Script>(script);
            return script as HaloScriptPreprocessor.AST.Script;
        }

        private void CheckValueIsString(Value value)
        {
            Assert.NotNull(value.GetString());
            Assert.Null(value.GetBoolean());
            Assert.Null(value.GetFloat());
            Assert.Null(value.GetLong());
            Assert.Null(value.GetShort());
        }

        [Fact]
        public void InterpretGlobal_ConstantGlobalString()
        {
            var conststringGlobal = GetGlobal("conststring");

            var result = _interpreter.InterpretGlobal(conststringGlobal);
            Assert.NotNull(result);

            // check type
            CheckValueIsString(result);

            // check value
            Assert.Equal("A constglobal string!", result.GetString());
        }

        [Fact]
        public void InterpretGlobal_ConstantGlobalLong()
        {
            var global = GetGlobal("constlong");

            var result = _interpreter.InterpretGlobal(global);
            Assert.NotNull(result);

            // check value
            Assert.Equal(117, result.GetLong());
        }

        [Fact]
        public void InterpretGlobal_ConstantGlobalReal()
        {
            var global = GetGlobal("constreal");

            var result = _interpreter.InterpretGlobal(global);
            Assert.NotNull(result);

            // check value
            Assert.Equal(117.0f, result.GetFloat());
        }

        [Fact]
        public void InterpretGlobal_NonConstant()
        {
            Assert.Null(_interpreter.InterpretGlobal(GetGlobal("otherstring")));
            Assert.Null(_interpreter.InterpretGlobal(GetGlobal("otherlong")));
            Assert.Null(_interpreter.InterpretGlobal(GetGlobal("otherreal")));
        }

        [Fact]
        public void InterpretValue_Generic()
        {
            var global = GetGlobal("otherreal");
            var result = _interpreter.InterpretValue(global.Value);
            Assert.NotNull(result);
            Assert.Equal(117.0f, result.GetFloat());
        }

        [Fact]
        public void InterpretCode_Generic()
        {
            var global = GetGlobal("otherreal");
            var result = _interpreter.InterpretCode(global.Value.Content.AsT1);
            Assert.NotNull(result);
            Assert.Equal(117.0f, result.GetFloat());
        }

        [Fact]
        public void IsInCache()
        {
            var global = GetGlobal("conststring");
            Assert.False(_interpreter.IsInCache(global));
            _interpreter.InterpretGlobal(global);
            Assert.True(_interpreter.IsInCache(global));
        }

        /// <summary>
        /// Gets values from the <c>test_script</c> script so we can test the value functions, see <c>Test.hsc</c> for the indices
        /// </summary>
        /// <returns>Array of values from the script</returns>
        public HaloScriptPreprocessor.AST.Value[] GetTestValues()
        {
            return GetScript("test_script").Codes.ToArray();
        }

        [Fact]
        public void InterpretValue_mul()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[0]);
            Assert.NotNull(result);
            Assert.Equal(117.0f, result.GetFloat());

            Assert.Null(_interpreter.InterpretValue(values[4]));

            var result2 = _interpreter.InterpretValue(values[5]);
            Assert.NotNull(result2);
            Assert.Equal(234.0f, result2.GetFloat());
        }

        [Fact]
        public void InterpretValue_equ()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[1]);
            Assert.NotNull(result);
            Assert.Equal(false, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[2]);
            Assert.NotNull(result2);
            Assert.Equal(true, result2.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[3]));
        }

        [Fact]
        public void InterpretValue_and()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[6]);
            Assert.NotNull(result);
            Assert.Equal(false, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[7]);
            Assert.NotNull(result2);
            Assert.Equal(true, result2.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[8]));
        }

        [Fact]
        public void InterpretValue_or()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[9]);
            Assert.NotNull(result);
            Assert.Equal(true, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[10]);
            Assert.NotNull(result2);
            Assert.Equal(true, result2.GetBoolean());

            var result3 = _interpreter.InterpretValue(values[11]);
            Assert.NotNull(result3);
            Assert.Equal(true, result3.GetBoolean());

            var result4 = _interpreter.InterpretValue(values[12]);
            Assert.NotNull(result4);
            Assert.Equal(true, result4.GetBoolean());

            var result5 = _interpreter.InterpretValue(values[33]);
            Assert.NotNull(result5);
            Assert.Equal(false, result5.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[13]));
        }

        [Fact]
        public void InterpretValue_add()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[14]);
            Assert.NotNull(result);
            Assert.Equal(5.0f, result.GetFloat());

            var result2 = _interpreter.InterpretValue(values[15]);
            Assert.NotNull(result2);
            Assert.Equal(12.0f, result2.GetFloat());

            Assert.Null(_interpreter.InterpretValue(values[16]));
            Assert.Null(_interpreter.InterpretValue(values[17]));
        }

        [Fact]
        public void InterpretValue_sub()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[18]);
            Assert.NotNull(result);
            Assert.Equal(-1.0f, result.GetFloat());

            Assert.Null(_interpreter.InterpretValue(values[19]));
            Assert.Null(_interpreter.InterpretValue(values[20]));
            Assert.Null(_interpreter.InterpretValue(values[21]));
            Assert.Null(_interpreter.InterpretValue(values[22]));
        }

        [Fact]
        public void InterpretValue_div()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            Assert.Null(_interpreter.InterpretValue(values[23]));

            var result = _interpreter.InterpretValue(values[24]);
            Assert.NotNull(result);
            Assert.Equal(5.0f, result.GetFloat());

            Value result2 = _interpreter.InterpretValue(values[25]);
            Assert.NotNull(result2);
            Assert.NotNull(result2.GetFloat());
            Assert.True(float.IsInfinity(result2.GetFloat().Value));
        }

        [Fact]
        public void InterpretValue_min()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[26]);
            Assert.NotNull(result);
            Assert.Equal(0.0f, result.GetFloat());

            Assert.Null(_interpreter.InterpretValue(values[27]));
        }

        [Fact]
        public void InterpretValue_max()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[28]);
            Assert.NotNull(result);
            Assert.Equal(3.0f, result.GetFloat());

            Assert.Null(_interpreter.InterpretValue(values[29]));
        }

        [Fact]
        public void InterpretValue_neq()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[30]);
            Assert.NotNull(result);
            Assert.Equal(true, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[31]);
            Assert.NotNull(result2);
            Assert.Equal(false, result2.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[32]));
        }

        [Fact]
        public void InterpretValue_gtr()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[34]);
            Assert.NotNull(result);
            Assert.Equal(false, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[35]);
            Assert.NotNull(result2);
            Assert.Equal(false, result2.GetBoolean());

            var result3 = _interpreter.InterpretValue(values[36]);
            Assert.NotNull(result3);
            Assert.Equal(true, result3.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[37]));
        }

        [Fact]
        public void InterpretValue_lss()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[38]);
            Assert.NotNull(result);
            Assert.Equal(true, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[39]);
            Assert.NotNull(result2);
            Assert.Equal(false, result2.GetBoolean());

            var result3 = _interpreter.InterpretValue(values[40]);
            Assert.NotNull(result3);
            Assert.Equal(false, result3.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[41]));
        }

        [Fact]
        public void InterpretValue_geq()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[42]);
            Assert.NotNull(result);
            Assert.Equal(false, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[43]);
            Assert.NotNull(result2);
            Assert.Equal(true, result2.GetBoolean());

            var result3 = _interpreter.InterpretValue(values[44]);
            Assert.NotNull(result3);
            Assert.Equal(true, result3.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[45]));
        }

        [Fact]
        public void InterpretValue_leq()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[46]);
            Assert.NotNull(result);
            Assert.Equal(true, result.GetBoolean());

            var result2 = _interpreter.InterpretValue(values[47]);
            Assert.NotNull(result2);
            Assert.Equal(true, result2.GetBoolean());

            var result3 = _interpreter.InterpretValue(values[48]);
            Assert.NotNull(result3);
            Assert.Equal(false, result3.GetBoolean());

            Assert.Null(_interpreter.InterpretValue(values[49]));
        }

        [Fact]
        public void InterpretValue_begin()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[50]);
            Assert.NotNull(result);
            Assert.Equal(12, result.GetLong());

            Assert.Null(_interpreter.InterpretValue(values[51]));
        }

        [Fact]
        public void InterpretValue_if()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[52]);
            Assert.NotNull(result);
            Assert.Equal(5, result.GetLong());

            Assert.Null(_interpreter.InterpretValue(values[53]));

            var result2 = _interpreter.InterpretValue(values[54]);
            Assert.NotNull(result2);
            Assert.Equal(10, result2.GetLong());

            Assert.Null(_interpreter.InterpretValue(values[55]));
        }

        [Fact]
        public void InterpretValue_userScriptStatic()
        {
            HaloScriptPreprocessor.AST.Value[] values = GetTestValues();

            var result = _interpreter.InterpretValue(values[56]);
            Assert.NotNull(result);
            Assert.Equal("test_script_time!", result.GetString());
        }
    }
}

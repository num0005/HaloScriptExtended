using HaloScriptPreprocessor.Interpreter;
using System;
using Xunit;

namespace HaloScriptPreprocessor.Tests.Interpreter
{
    public class ValueTests
    {
        public ValueTests()
        {

        }

        private readonly Value _longValue = new(200000);
        private readonly Value _shortValue = new(2);
        private readonly Value _realValue = new(5.1f);
        private readonly Value _booleanValue = new(false);

        private readonly Value _longAtomValue = new(new HaloScriptPreprocessor.AST.Atom("200000"));
        private readonly Value _shortAtomValue = new(new HaloScriptPreprocessor.AST.Atom("2"));
        private readonly Value _realAtomValue = new(new HaloScriptPreprocessor.AST.Atom("5.1"));
        private readonly Value _booleanAtomValue = new(new HaloScriptPreprocessor.AST.Atom("false"));

        private readonly Value _stringAtomValue = new(new HaloScriptPreprocessor.AST.Atom("Hello world!"));
        private readonly Value _voidValue = new();
        [Fact]
        public void GetLong_Test()
        {
            Assert.Equal(200000, _longValue.GetLong());
            Assert.Equal(2, _shortValue.GetLong());
            Assert.Equal(5, _realValue.GetLong());
            Assert.Null(_booleanValue.GetLong());

            Assert.Equal(200000, _longAtomValue.GetLong());
            Assert.Equal(2, _shortAtomValue.GetLong());
            Assert.Equal(5, _realAtomValue.GetLong());
            Assert.Null(_booleanAtomValue.GetLong());

            Assert.Null(_stringAtomValue.GetLong());
            Assert.Null(_voidValue.GetLong());
        }

        [Fact]
        public void GetShort_Test()
        {
            Assert.Null(_longValue.GetShort());
            Assert.Equal((short)2, _shortValue.GetShort());
            Assert.Equal((short)5, _realValue.GetShort());
            Assert.Null(_booleanValue.GetShort());

            Assert.Null(_longAtomValue.GetShort());
            Assert.Equal((short)2, _shortAtomValue.GetShort());
            Assert.Equal((short)5, _realAtomValue.GetShort());
            Assert.Null(_booleanAtomValue.GetShort());

            Assert.Null(_stringAtomValue.GetShort());
            Assert.Null(_voidValue.GetShort());
        }

        [Fact]
        public void GetFloat_Test()
        {
            Assert.Equal(200000, _longValue.GetLong());
            Assert.Equal(2, _shortValue.GetLong());
            Assert.Equal(5, _realValue.GetLong());
            Assert.Null(_booleanValue.GetLong());

            Assert.Equal(200000, _longAtomValue.GetLong());
            Assert.Equal(2, _shortAtomValue.GetLong());
            Assert.Equal(5, _realAtomValue.GetLong());
            Assert.Null(_booleanAtomValue.GetLong());

            Assert.Null(_stringAtomValue.GetLong());
            Assert.Null(_voidValue.GetLong());
        }

        [Fact]
        public void GetBoolean_Test()
        {
            Assert.True(_longValue.GetBoolean());
            Assert.True(_shortValue.GetBoolean());
            Assert.True(_realValue.GetBoolean());
            Assert.False(_booleanValue.GetBoolean());

            Assert.True(_longAtomValue.GetBoolean());
            Assert.True(_shortAtomValue.GetBoolean());
            Assert.True(_realAtomValue.GetBoolean());
            Assert.False(_booleanAtomValue.GetBoolean());

            Assert.Null(_stringAtomValue.GetBoolean());
            Assert.Null(_voidValue.GetBoolean());
        }

        [Fact]
        public void IsNotString_Test()
        {
            Assert.True(_longValue.IsNotString());
            Assert.True(_shortValue.IsNotString());
            Assert.True(_realValue.IsNotString());
            Assert.True(_booleanValue.IsNotString());

            Assert.True(_longAtomValue.IsNotString());
            Assert.True(_shortAtomValue.IsNotString());
            Assert.True(_realAtomValue.IsNotString());
            Assert.True(_booleanAtomValue.IsNotString());

            Assert.False(_stringAtomValue.IsNotString());
            Assert.False(_voidValue.IsNotString());
        }

        [Fact]
        public void IsEqual_Test()
        {
            Value[] values = { _longValue, _shortValue, _realValue, _booleanValue };
            Value[] atomicValues = { _longAtomValue, _shortAtomValue, _realAtomValue, _booleanAtomValue, _stringAtomValue, _voidValue };
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < atomicValues.Length; j++)
                    Assert.Equal(i == j, values[i].IsEqual(atomicValues[j]) ?? false);
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < atomicValues.Length; j++)
                    Assert.Equal(i == j, atomicValues[j].IsEqual(values[i]) ?? false);
        }

        [Fact]
        public void IsEqual_EqualToSelf()
        {
            Value[] values = { _longValue, _shortValue, _realValue, _booleanValue,
            _longAtomValue, _shortAtomValue, _realAtomValue, _booleanAtomValue, _stringAtomValue, _voidValue };

            foreach (Value value in values)
                Assert.True(value.IsEqual(value));
        }

            [Fact]
        public void GetString_Test()
        {
            Assert.Equal("200000", _longValue.GetString());
            Assert.Equal("2", _shortValue.GetString());
            Assert.Equal("5.1", _realValue.GetString());
            Assert.Equal("false", _booleanValue.GetString());

            Assert.Equal("200000", _longAtomValue.GetString());
            Assert.Equal("2", _shortAtomValue.GetString());
            Assert.Equal("5.1", _realAtomValue.GetString());
            Assert.Equal("false", _booleanAtomValue.GetString());

            Assert.Equal("Hello world!", _stringAtomValue.GetString());
            Assert.Null(_voidValue.GetString());
        }
    }
}

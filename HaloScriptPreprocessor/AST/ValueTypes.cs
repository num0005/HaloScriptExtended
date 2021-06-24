namespace HaloScriptPreprocessor.AST
{
    // todo enum
    public class ValueType
    {
        internal ValueType(string value)
        {
            _value = value;
        }
        readonly string _value;

        override public string ToString()
        {
            return _value;
        }
    }

    public static class ValueTypeMethods
    {
        public static ValueType ParseValueType(this string str)
        {
            return new ValueType(str);
        }

        public static string ToSyntaxString(this ValueType type)
        {
            return type.ToString();
        }
    }
}

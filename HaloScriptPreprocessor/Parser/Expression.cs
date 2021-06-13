using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace HaloScriptPreprocessor.Parser
{
    public record SourceFile(string Data, string FileName, Expression? SourceExpression);

    /*

    /// <summary>
    /// Location in the source
    /// </summary>
    public record SourceLocation(
        int Offset, // Offset in the source string
        int Line, // line (used for debugging)
        int Column // column (used for debugging)
    );
    */

    public struct SourceLocation
    {
        public SourceLocation(int Offset, int Line, int Column)
        {
            this.Offset = Offset;
            this.Line = Line;
            this.Column = Column;
        }
        public readonly int Offset; // Offset in the source string
        public readonly int Line; // line (used for debugging)
        public readonly int Column; // column (used for debugging)
    }

    /// <summary>
    /// Source of a parsed expression
    /// </summary>
    public class ExpressionSource
    {
        internal ExpressionSource(SourceFile file, SourceLocation start, SourceLocation? end = null)
        {
            _file = file;
            Start = start;
            _end = end;
        }

        public readonly SourceLocation Start;
        public SourceLocation? End
        {
            get => _end;
        }

        public ReadOnlySpan<char> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(!_isPartial);
                return _file.Data.AsSpan().Slice(start: Start.Offset, length: (End.Value.Offset - Start.Offset));
            }
        }
        public string Contents
        {
            get
            {
                Debug.Assert(!_isPartial);
                return Span.ToString();
            }
        }

        /// <summary>
        /// Set the end of the source (only allowed if the source is partial)
        /// </summary>
        /// <param name="end"></param>
        internal void setEnd(SourceLocation end)
        {
            Debug.Assert(_isPartial);
            _end = end;
        }

        // is partial location? (end not set yet)
        private bool _isPartial
        {
            get => _end is null;
        }

        private SourceLocation? _end;
        private readonly SourceFile _file;
    }

    /// <summary>
    /// A value (expression or atomic)
    /// </summary>
    public class Value
    {
        public Value(ExpressionSource source)
        {
            Source = source;
        }
        public readonly ExpressionSource Source;
    }



    /// <summary>
    /// A ordered list of other values
    /// </summary>
    public class Expression : Value
    {
        public Expression(ExpressionSource source) : base(source) { }
        public readonly List<Value> Values = new();
    }

    /// <summary>
    /// Atomic value
    /// </summary>
    public class Atom : Value
    {
        public Atom(ExpressionSource source, bool isQuoted) : base(source)
        {
            IsQuoted = isQuoted;
        }

        public ReadOnlySpan<char> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsQuoted)
                {
                    var sourceSpan = Source.Span;
                    return sourceSpan.Slice(1, sourceSpan.Length - 1);
                }
                else
                {
                    return Source.Span;
                }
            }
        }
        public string Value
        {
            get => Span.ToString();
        }

        // is quoted
        public readonly bool IsQuoted;
    }

    public class ParsedExpressions
    {
        public IReadOnlyList<Expression> Expressions
        {
            get => expressions.AsReadOnly();
        }

        #region implementation detail

        /// <summary>
        /// Add a top level expression
        /// </summary>
        /// <param name="expression">Expression to add</param>
        internal void AddExpression(Expression expression)
        {
            Debug.Assert(!_done, "Attempting to add expression after parsing was completed!");
            expressions.Add(expression);
        }

        internal void RemoveExpression(Expression expression)
        {
            Debug.Assert(!_done, "Attempting to remove expression after parsing was completed!");
            expressions.Remove(expression);
        }

        internal void Done()
        {
            _done = true;
        }

        private List<Expression> expressions = new();

        // is parsing done?
        private bool _done = false;
        #endregion
    }

}

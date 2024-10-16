﻿/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace HaloScriptPreprocessor.Parser
{
    public record SourceFile(string Data, string FileName, Expression? SourceExpression);
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

        public string PrettyPrint()
        {
            return $"{Line}:{Column}";
        }

        public override string ToString()
        {
            return $"Line={Line}, Column={Column}, Offset={Offset}";
        }
    }

    public record FileSourceLocation(SourceLocation Location, SourceFile File)
    {
        public char Contents => File.Data[Location.Offset];

        public string Formatted => $"{File.FileName}|{Location.PrettyPrint()} ¦ {Contents}";
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
#pragma warning disable CS8629 // Nullable value type may be null.
                return _file.Data.AsSpan()[Start.Offset..End.Value.Offset];
#pragma warning restore CS8629 // Nullable value type may be null.
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

        public string FileName => _file.FileName;

        public string PrettyPrint()
        {
            return $"{FileName}|{Start.PrettyPrint()}..{End.Value.PrettyPrint()} ¦ {Contents}";
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

        /// <summary>
        /// Source of the value
        /// </summary>
        public readonly ExpressionSource Source;

        /// <summary>
        /// Get an <c>Atom</c> throws an exception if <c>Value</c> is an expressions.
        /// </summary>
        /// <param name="error">Error message to show</param>
        /// <returns>Atom</returns>
        /// <exception cref="UnexpectedExpression"></exception>
        public Atom ExpectAtom(string error)
        {
            if (this is not Atom atom)
                throw new UnexpectedExpression(this.Source, error);
            return atom;
        }

        /// <summary>
        /// Get an <c>Expression</c> throws an exception if <c>Value</c> is an atom.
        /// </summary>
        /// <param name="error">Error message to show</param>
        /// <returns>Expression</returns>
        /// <exception cref="UnexpectedAtom"></exception>
        public Expression ExpectExpression(string error)
        {
            if (this is not Expression expression)
                throw new UnexpectedAtom(this.Source, error);
            return expression;
        }
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
                    return sourceSpan.Slice(1, sourceSpan.Length - 2);
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

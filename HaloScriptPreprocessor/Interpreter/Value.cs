/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;

namespace HaloScriptPreprocessor.Interpreter
{
    public class Value
    {
        public Value()
        {
            Contents = new Void();
        }
        public Value(AST.Atom atom)
        {
            Contents = atom;
        }

        public Value(long @long)
        {
            Contents = @long;
        }

        public Value(short @short)
        {
            Contents = @short;
        }
        public Value(float real)
        {
            Contents = real;
        }

        public Value(bool boolean)
        {
            Contents = boolean;
        }

        /// <summary>
        /// HaloScript nothing
        /// </summary>
        public struct Void { };
        public OneOf.OneOf<Void, AST.Atom, long, short, float, bool> Contents;
        public long? GetLong()
        {
            return Contents.Match<long?>(
                _ => null,
                atom =>
                {
                    long result;
                    float real;
                    if (long.TryParse(atom.ToSpan(), out result))
                        return result;
                    else if (float.TryParse(atom.ToSpan(), out real))
                        return (long)real;
                    else
                        return null;
                },
                @long => @long,
                @short => @short,
                real => (long)real,
                _ => null
                );
        }

        public short? GetShort()
        {
            return Contents.Match<short?>(
                _ => null,
                atom =>
                {
                    short result;
                    float real;
                    long @long;
                    if (short.TryParse(atom.ToSpan(), out result))
                        return result;
                    else if (long.TryParse(atom.ToSpan(), out @long))
                        return null;
                    else if (float.TryParse(atom.ToSpan(), out real))
                        return (short)real;
                    else
                        return null;
                },
                @long =>
                {
                    if (@long > short.MaxValue || @long < short.MinValue)
                        return null;
                    return (short)@long;
                },
                @short => @short,
                real => (short)real,
                _ => null
                );
        }

        public float? GetFloat()
        {
            return Contents.Match<float?>(
                _ => null,
                atom =>
                {
                    float result;
                    if (float.TryParse(atom.ToSpan(), out result))
                        return result;
                    else
                        return null;
                },
                @long => @long,
                @short => @short,
                real => real,
                _ => null
                );
        }

        public bool? GetBoolean()
        {
            return Contents.Match<bool?>(
                _ => null,
                atom =>
                {
                    ReadOnlySpan<char> span = atom.ToSpan();
                    float real;
                    long @long;
                    bool boolean;
                    if (long.TryParse(span, out @long))
                        return @long != 0;
                    else if (float.TryParse(span, out real))
                        return real != 0.0f;
                    else if (bool.TryParse(span, out boolean))
                        return boolean;
                    else
                        return null;
                },
                @long => @long != 0,
                @short => @short != 0,
                real => real != 0.0f,
                boolean => boolean
                );
        }

        /// <summary>
        /// Check if the value can be represented as a boolean or number
        /// </summary>
        /// <returns></returns>
        public bool IsNotString()
        {
            return GetFloat() is not null || GetBoolean() is not null;
        }

        public bool? IsEqual(Value other)
        {
            if (this.Contents.Index == other.Contents.Index)
                return Contents.Value.Equals(other.Contents.Value);
            if (Contents.IsT0 || other.Contents.IsT0)
                return false;
            return Contents.Match<bool?>(
                _ => false,
                _ => other.IsEqual(this), // uno reverso
                @long =>
                {
                    var otherValue = other.GetLong();
                    if (otherValue is null)
                        return null;
                    return @long == otherValue;
                },
                @short =>
                {
                    var otherValue = other.GetShort();
                    if (otherValue is null)
                        return null;
                    return @short == otherValue;
                },
                @float =>
                {
                    var otherValue = other.GetFloat();
                    if (otherValue is null)
                        return null;
                    return @float == otherValue;
                },
                @bool =>
                {
                    var otherValue = other.GetBoolean();
                    if (otherValue is null)
                        return null;
                    return @bool == otherValue;
                }
            );
        }

        public string? GetString()
        {
            return Contents.Match<string?>(
                _ => null,
                atom => atom.ToString(),
                @long => @long.ToString(),
                @short => @short.ToString(),
                real => real.ToString(),
                boolean => boolean ? "true" : "false"
            );
        }
    }
}

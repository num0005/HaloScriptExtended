/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using OneOf;
using System;
using System.Collections.Generic;

namespace HaloScriptPreprocessor.AST
{
    public class Value : Node, IEquatable<Value>
    {
        private Value(Parser.Value? source) : base(source) { }

        public Value(Parser.Atom? source, Atom atom) : base(source)
        {
            Content = atom;
        }

        public Value(Parser.Expression source, Code code) : base(source)
        {
            Content = code;
        }

        public OneOf<Atom, Code, Global, Script> Content;

        public override uint NodeCount => Content.IsT1 ? Content.AsT1.NodeCount : 1;

        public override Value Clone(Node? parent = null)
        {
            Value value = new(Source);
            Content.Switch(
                atom => value.Content = atom.Clone(value),
                code => value.Content = code.Clone(value),
                global => value.Content = global,
                script => value.Content = script
            );

            value.ParentNode = parent;

            return value;
        }

        override public bool Equals(object? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (other is Value otherValue)
                return Equals(otherValue);
            return false;
        }

        public bool Equals(Value? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            if (Content.Index != other.Content.Index)
                return false;
            // code will always be treated as different for now, todo: change this
            return Content.Match(
                atom => other.Content.AsT0.Equals(atom),
                _ => false, global => global == other.Content.AsT2,
                script => script == other.Content.AsT3
            );
        }

        public override void Rewrite(Dictionary<Value, Value> mapping)
        {
            Content.Switch(
                _ => { },
                code => code.Rewrite(mapping),
                _ => { },
                _ => { }
            );
        }

        public override int GetHashCode()
        {
            return Content.Value.GetHashCode();
        }
    }
}

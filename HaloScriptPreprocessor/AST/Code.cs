using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.AST
{
    public class Code : Node
    {
        private Code(Parser.Value source) : base(source) {}
        public Code(Parser.Expression source, Atom function, LinkedList<Value> arguments) : base(source)
        {
            Function = function;
            Arguments = arguments;
        }

        public OneOf<Atom, Script> Function;
        public LinkedList<Value> Arguments = new();

        public ReadOnlySpan<char> FunctionSpan
        {
            get
            {
                if (Function.IsT0)
                    return Function.AsT0.ToSpan();
                if (Function.IsT1)
                    return Function.AsT1.Name.ToSpan();
                Debug.Fail("unreachable");
                return "";
            }
        }

        /// <summary>
        /// Set the contents from another <c>Code</c> node expect parent
        /// </summary>
        /// <param name="other"></param>
        public void SetContents(Code other)
        {
            this.Arguments = other.Arguments;
            this.Function = other.Function;
        }

        public override Code Clone(Node? parent = null)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            Code clonedCode = new(Source);
#pragma warning restore CS8604 // Possible null reference argument.
            LinkedList<Value> clonedArguments = new();
            foreach (Value arg in Arguments)
                clonedArguments.AddLast(arg.Clone(clonedCode));
            clonedCode.Arguments = clonedArguments;
            Function.Switch(atom => clonedCode.Function = atom.Clone(clonedCode), script => clonedCode.Function = script);
            return clonedCode;
        }

        public override void Rewrite(Dictionary<Value, Value> mapping)
        {
            LinkedListNode<Value>? arg = Arguments.First;
            while (arg is not null)
            {
                if (mapping.ContainsKey(arg.Value))
                    arg.Value = mapping[arg.Value].Clone(this); // clone so parent is set correctly
                arg.Value.Rewrite(mapping);
                arg = arg.Next;
            }
        }

        public override uint NodeCount
        {
            get
            {
                uint count = 1;
                foreach (var value in Arguments)
                {
                    count += value.NodeCount;
                }
                return count;
            }
        }

    }
}

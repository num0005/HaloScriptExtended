/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using HaloScriptPreprocessor.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HaloScriptPreprocessor.Emitter
{
    class HaloScriptEmitter
    {
        public HaloScriptEmitter(TextWriter textWriter, AST.AST ast)
        {
            _textWriter = textWriter;
            _ast = ast;
            _userNameMapping = _ast.UserNameMapping;
        }

        public void Emit()
        {
            foreach (KeyValuePair<string, AST.NodeNamed> entry in _userNameMapping)
            {
                if (entry.Value is AST.Script script)
                {
                    emitScript(script);
                }
                else if (entry.Value is AST.Global global)
                {
                    emitGlobal(global);
                }
                else
                {
                    Debug.Fail("unreachable!");
                }
            }
            _textWriter.Flush();
        }

        void emitScript(Script script)
        {
            Debug.Assert(script.Type != ScriptType.Macro);
            Debug.Assert(script.Type != ScriptType.Invalid);
            Debug.Assert(script.Type != ScriptType.Stub && script.Type != ScriptType.Static || script.ReturnValueType is not null);
            Debug.Assert(script.ReturnValueType is null || script.Type == ScriptType.Stub || script.Type == ScriptType.Static);

            enterRootExpression();
            emitAtom("script");
            emitAtom(script.Type.ToSyntaxString());
            if (script.ReturnValueType is AST.ValueType returnType)
                emitAtom(returnType.ToSyntaxString());
            emitAtom(script.Name);
            foreach (Value code in script.Codes)
                emitValue(code);
            exitExpression();
        }

        void emitGlobal(AST.Global global)
        {
            Debug.Assert(global.IsConst == false);
            enterRootExpression();
            emitAtom("global");
            emitAtom(global.ValueType.ToSyntaxString());
            emitAtom(global.Name);
            emitValue(global.Value);
            exitExpression();
        }

        void emitValue(AST.Value value)
        {
            value.Content.Switch(
                atom => emitAtom(atom),
                code => emitCode(code),
                global => emitAtom(global.Name),
                script => emitAtom(script.Name)
            );
        }

        void emitCode(AST.Code code)
        {
            enterExpression();
            if (code.ParentNode is Value value &&
                    value.ParentNode is Code parent && parent.FunctionSpan.SequenceEqual("cond"))
                Debug.Print("Found \"cond\" not print \"if\"");
            else
                emitAtom(code.FunctionSpan);
            foreach (Value arg in code.Arguments)
                emitValue(arg);
            exitExpression();
        }

        void emitAtom(AST.Atom atom)
        {
            atom.Value.Switch(
                parserAtom => emitAtom(parserAtom.Span),
                sAtom => emitAtom(sAtom)
            );
        }


        void emitAtom(string atom)
        {
            emitAtom(atom.AsSpan());
        }

        void emitAtom(ReadOnlySpan<char> atom)
        {
            bool needQuote = atom.Contains(' ');
            if (_needSpace)
                _textWriter.Write(' ');
            if (needQuote)
                _textWriter.Write('"');
            _textWriter.Write(atom);
            if (needQuote)
                _textWriter.Write('"');
            _needSpace = true;
        }

        void enterRootExpression()
        {
            Debug.Assert(_expressionDepth == -1);
            enterExpression();
        }

        void enterExpression()
        {
            _expressionDepth++;
            if (!_isFirstExpression)
                emitNewline();
            _textWriter.Write('(');
            _needSpace = false;
            _isFirstExpression = false;
        }

        void exitExpression()
        {
            _expressionDepth--;
            if (_expressionDepth == -1)
                emitNewline();
            _textWriter.Write(')');
        }

        void emitNewline()
        {
            if (!PrettyFormat)
                return;
            _textWriter.Write(_textWriter.NewLine);
            if (_expressionDepth > 0)
            {
                char[] indent = new char[_expressionDepth];
                Array.Fill(indent, '\t');
                _textWriter.Write(indent);
            }
        }

        bool PrettyFormat = true;

        private int _expressionDepth = -1;
        /// <summary>
        /// whatever the next atom should have a space before it
        /// </summary>
        private bool _needSpace = false;
        /// <summary>
        /// Is this the first expression?
        /// </summary>
        private bool _isFirstExpression = true;

        private readonly TextWriter _textWriter;
        private readonly AST.AST _ast;
        private readonly IReadOnlyDictionary<string, AST.NodeNamed> _userNameMapping;
    }
}

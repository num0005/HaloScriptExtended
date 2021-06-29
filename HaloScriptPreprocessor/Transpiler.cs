﻿/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;
using System.IO;

namespace HaloScriptPreprocessor
{
    public class Transpiler
    {
        public Transpiler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            _ASTBuilder = new(fileSystem, "hscx_scripts", AST);
            _interpreter = new(_ast);
        }

        /// <summary>
        /// Add a file to the AST
        /// </summary>
        /// <param name="file">Source file</param>
        public void AddFile(IFileSystem.IFile file)
        {
            _ASTBuilder.Import(file);
        }

        /// <summary>
        /// Emit the AST as HaloScript
        /// </summary>
        /// <param name="fileName">File to emit to</param>
        public void EmitCode(string fileName)
        {
            EmitCode(FileSystem.GetTextWriter("scripts" + FileSystem.DirectorySeparator + fileName));
        }

        /// <summary>
        /// Emit the AST as HaloScript
        /// </summary>
        /// <param name="textWritter">TextWritter to emit to</param>
        public void EmitCode(TextWriter textWritter)
        {
            Emitter.HaloScriptEmitter emitter = new(textWritter, AST);
            emitter.Emit();
        }

        [Flags]
        public enum Pass
        {
            /// <summary>
            /// Macro expansion pass
            /// </summary>
            Macro = 1,

            /// <summary>
            /// Loop expansion pass
            /// </summary>
            Loop = 2,

            /// <summary>
            /// Expand constant globals
            /// </summary>
            ConstantGlobal = 4,

            /// <summary>
            /// The passes needed to be able to export as HSC
            /// </summary>
            Mimimal = Macro | Loop | ConstantGlobal,

            /// <summary>
            /// Simplify the code
            /// </summary>
            ConstantEval = 8,

            Full = Mimimal | ConstantEval,

            All = Full
        };

        public void RunPasses(Pass passes)
        {
            Passes.ConstantGlobalPass constantGlobalPass = new(AST, Interpreter);
            Passes.LoopUnrolling loopUnrolling = new(AST, Interpreter);
            Passes.MacroExpansionPass macroExpansion = new(AST);
            Passes.CompileTimeEvaluationPass compileTimeEvaluationPass = new(AST, Interpreter);

            if (passes.HasFlag(Pass.Macro))
                macroExpansion.Run();
            if (passes.HasFlag(Pass.Loop))
                loopUnrolling.Run();
            if (passes.HasFlag(Pass.ConstantGlobal))
                constantGlobalPass.Run();
            if (passes.HasFlag(Pass.ConstantEval))
                compileTimeEvaluationPass.Run();
        }

        public IFileSystem FileSystem { get; }
        public AST.AST AST => _ast;
        public Interpreter.Interpreter Interpreter => _interpreter;

        readonly private AST.AST _ast = new();
        readonly private Parser.ASTBuilder _ASTBuilder;
        readonly private Interpreter.Interpreter _interpreter;
    }
}
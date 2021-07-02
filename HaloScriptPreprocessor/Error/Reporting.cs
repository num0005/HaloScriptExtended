/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.Collections.Generic;
using System.ComponentModel;

namespace HaloScriptPreprocessor.Error
{
    /// <summary>
    /// Error message
    /// </summary>
    public record Message(
        Level Level,
        string Content,
        OneOf.OneOf<Parser.ExpressionSource, Parser.FileSourceLocation, AST.Node, AST.NodeNamed>? Source
        );

    public class Reporting
    {
        /// <summary>
        /// Report a error/warning without a clear source file (avoid using this)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void Report(Level level, string message)
        {
            Report(new(level, message, null));
        }

        /// <summary>
        /// Report a error/warning
        /// </summary>
        /// <param name="level"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Report(Level level, AST.NodeNamed source, string message)
        {
            Report(new(level, message, source));
        }

        /// <summary>
        /// Report a error/warning
        /// </summary>
        /// <param name="level"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Report(Level level, AST.Node source, string message)
        {
            Report(new(level, message, source));
        }

        /// <summary>
        /// Report a error/warning
        /// </summary>
        /// <param name="level"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Report(Level level, Parser.ExpressionSource source, string message)
        {
            Report(new(level, message, source));
        }

        /// <summary>
        /// Report a error/warning
        /// </summary>
        /// <param name="level"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Report(Level level, Parser.FileSourceLocation source, string message)
        {
            Report(new(level, message, source));
        }

        /// <summary>
        /// Report a error/warning
        /// </summary>
        /// <param name="message"></param>
        public void Report(Message message)
        {
            if (message.Level == Level.Error)
                _hasFatalErrors = true;
            _messages.Add(message);
        }

        /// <summary>
        /// Get all messages
        /// </summary>
        public IReadOnlyList<Message> Messages => _messages;

        /// <summary>
        /// Have any fatal errors been reported?
        /// </summary>
        public bool HasFatalErrors => _hasFatalErrors;

        private readonly List<Message> _messages = new();
        private bool _hasFatalErrors = false;
    }
}

/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

namespace HaloScriptPreprocessor.Error
{
    public enum Level
    {
        /// <summary>
        /// A fatal error, compilation can't continue
        /// </summary>
        Error,

        /// <summary>
        /// A serious issue that should be fixed but compilation can continue
        /// </summary>
        Warning,

        /// <summary>
        /// Less important issues and other messages
        /// </summary>
        Informational
    }
}

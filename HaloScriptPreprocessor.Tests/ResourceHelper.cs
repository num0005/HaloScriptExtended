/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.IO;
using System.Reflection;

namespace HaloScriptPreprocessor.Tests
{
    internal static class ResourceHelper
    {
        /// <summary>
        /// Read a resource as a string
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>String representation</returns>
        public static string Read(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

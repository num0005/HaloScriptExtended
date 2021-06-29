/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.IO;

namespace HaloScriptPreprocessor
{
    public interface IFileSystem
    {
        public enum Status
        {
            /// <summary>
            /// A new file was created
            /// </summary>
            CreatedNew,
            /// <summary>
            ///  An existing file was replaced
            /// </summary>
            Replaced,

            /// <summary>
            /// File not found
            /// </summary>
            NoSuchFile,
        }
        public interface IFile
        {
            /// <summary>
            /// Name of the file, can include any character
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The contents of the file
            /// </summary>
            public string Contents { get; }
        }

        public interface IWritableFile : IFile
        {
            /// <summary>
            /// Read and write
            /// </summary>
            public new string Contents { get; set; }
        }

        /// <summary>
        /// Get a named file
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>File or <c>null</c> if not found/returns>
        public IFile? GetFile(string name);

        /// <summary>
        /// Change file contents or delete a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="contents">New contents or <c>null</c> to delete the file</param>
        /// <returns>Status code</returns>
        public Status Write(string fileName, string? contents);

        /// <summary>
        /// Get a TextWritter for <c>filename</c>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public TextWriter GetTextWriter(string filename);

        /// <summary>
        /// Directory separator
        /// </summary>
        public string DirectorySeparator { get; }
    }
}

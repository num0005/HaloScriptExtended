/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System;
using System.IO;

namespace HaloScriptPreprocessor.Tests
{
    class TestFileSystem : IFileSystem
    {
        class TestFile : IFileSystem.IFile
        {
            public TestFile(string name, string contents)
            {
                Name = name;
                Contents = contents;
            }
            public string Name {
                get;
            }

            public string Contents
            {
                get;
            }
        }
        public string DirectorySeparator => "";

        public IFileSystem.IFile GetFile(string name)
        {
            return new TestFile(name, ResourceHelper.Read(name));
        }

        public TextWriter GetTextWriter(string filename)
        {
            throw new NotImplementedException();
        }

        public IFileSystem.Status Write(string fileName, string contents)
        {
            throw new NotImplementedException();
        }
    }
}

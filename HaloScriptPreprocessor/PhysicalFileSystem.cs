/*
 Copyright (c) num0005. Some rights reserved
 Released under the MIT License, see LICENSE.md for more information.
*/

using System.IO;

namespace HaloScriptPreprocessor
{
    class PhysicalFileSystem : IFileSystem
    {
        public PhysicalFileSystem(string baseDirectory)
        {
            _baseDirectory = Path.GetFullPath(baseDirectory);
        }
        private class PhysicalFile : IFileSystem.IFile
        {
            public PhysicalFile(string name, string contents)
            {
                Name = name;
                Contents = contents;
            }

            public string Name { get; }

            public string Contents { get; }
        }
        public IFileSystem.IFile? GetFile(string name)
        {
            string readPath = TranslatePath(name);
            try
            {
                string fileContents = File.ReadAllText(readPath);
                string normalisedPath = Path.GetRelativePath(_baseDirectory, readPath);

                return new PhysicalFile(name: normalisedPath, contents: fileContents);
            } catch
            {
                return null;
            }
        }

        public IFileSystem.Status Write(string fileName, string? contents)
        {
            string path = TranslatePath(fileName);
            if (contents is null)
            {
                if (!File.Exists(path))
                    return IFileSystem.Status.NoSuchFile;
                File.Delete(path);
                return IFileSystem.Status.Replaced;
            } else
            {
                EnsureParentDirectoryExists(path);
                bool fileExists = File.Exists(fileName);
                File.WriteAllText(fileName, contents);
                return fileExists ? IFileSystem.Status.Replaced : IFileSystem.Status.CreatedNew;
            }
        }

        public TextWriter GetTextWriter(string fileName)
        {
            string path = TranslatePath(fileName);
            EnsureParentDirectoryExists(path);
            return new StreamWriter(path);
        }

        private string TranslatePath(string name)
        {
            return Path.Combine(_baseDirectory, name);
        }

        private void EnsureParentDirectoryExists(string path)
        {
            if (Path.GetDirectoryName(path) is string directoryPath)
                Directory.CreateDirectory(directoryPath);
        }

        private readonly string _baseDirectory;

        public string DirectorySeparator => Path.DirectorySeparatorChar.ToString();
    }
}

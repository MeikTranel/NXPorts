using Microsoft.Build.Utilities.ProjectCreation;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NXPorts.Tests.Infrastructure
{
    public class TestEnvironment : MSBuildTestBase
    {
        public TestEnvironment()
        {
            CurrentDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            );
        }

        public DirectoryInfo CurrentDirectory { get; private set; }

        public string CurrentDirectoryPath => CurrentDirectory.FullName;

        public static string GetApplicationDirectory()
        {
            return new Uri(Path.GetDirectoryName(Assembly.GetAssembly(typeof(TestEnvironment)).CodeBase)).LocalPath;
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath, string destinationPath)
        {
            File.Copy(Path.Combine(GetApplicationDirectory(), "TestFiles", relativeTestFilesPath), GetAbsolutePath(destinationPath));
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath)
        {
            CopyFileFromTestFiles(relativeTestFilesPath, relativeTestFilesPath);
        }

        /// <summary>
        /// Returns an absolute Path inside the test environment.
        /// </summary>
        /// <param name="path">a relative Path to a file or directory.</param>
        /// <returns>A rooted Path.</returns>
        public string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (Path.IsPathRooted(path))
                throw new ArgumentException("Cannot produce an absolute path inside the test environment if the given path is already absolute.", nameof(path));

            var absolutePath = Path.Combine(CurrentDirectoryPath, path);
            Debug.Assert(Path.IsPathRooted(absolutePath));
            return absolutePath;
        }
    }
}

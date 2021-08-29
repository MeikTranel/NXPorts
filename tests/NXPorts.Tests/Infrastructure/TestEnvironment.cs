using Buildalyzer;
using Buildalyzer.Environment;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NXPorts.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NXPorts.Tests.Infrastructure
{
    public class TestEnvironment : IDisposable
    {
        public DirectoryInfo CurrentDirectory { get; private set; }

        public string CurrentDirectoryPath => CurrentDirectory.FullName;

        public TestEnvironment()
        {
            CurrentDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            );
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

        /// <summary>
        /// Dynamically generates a DLL in the current working directory with a given set of syntax trees.
        /// </summary>
        /// <param name="assemblyName">The name of the DLL file and the underlying assembly.</param>
        /// <param name="csharpDocuments">A set of C# documents</param>
        /// <param name="platform">The target platform of the DLL</param>
        /// <returns>Returns True if the dynamic compilation succeeds.</returns>
        /// <remarks>The dynamic compilation will only work if the types used in the code to be compiled are also referenced in some way by the test AppContext.</remarks>
        public bool CreateTestDLL(string assemblyName, IEnumerable<string> csharpDocuments, Platform platform)
        {
            var syntaxTrees = (csharpDocuments == null) ? throw new ArgumentNullException(nameof(csharpDocuments)) : csharpDocuments.Select(doc => CSharpSyntaxTree.ParseText(doc));
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                platform: platform
            );
            var csc = CSharpCompilation.Create(assemblyName, syntaxTrees, GetRelevantReferences(), options);

            using (var dllFileStream = new FileStream(Path.Combine(CurrentDirectoryPath, assemblyName + ".dll"), FileMode.OpenOrCreate))
            using (var pdbFileStream = new FileStream(Path.Combine(CurrentDirectoryPath, assemblyName + ".pdb"), FileMode.OpenOrCreate))
            {
                var emitResult = csc.Emit(dllFileStream, pdbFileStream);
                return emitResult.Success;
            }
        }

        /// <summary>
        /// Dynamically generates a DLL in the current working directory with a given set of syntax trees,
        /// while defaulting to the current process platform as the target platform of the DLL.
        /// </summary>
        /// <param name="assemblyName">The name of the DLL file and the underlying assembly.</param>
        /// <param name="csharpDocuments">A set of C# documents</param>
        /// <returns>Returns True if the dynamic compilation succeeds.</returns>
        /// <remarks>The dynamic compilation will only work if the types used in the code to be compiled are also referenced in some way by the test AppContext.</remarks>
        public bool CreateTestDLL(string assemblyName, IEnumerable<string> csharpDocuments)
        {
            if (Environment.Is64BitProcess)
                return CreateTestDLL(assemblyName, csharpDocuments, Platform.X64);
            else
                return CreateTestDLL(assemblyName, csharpDocuments, Platform.X86);
        }

        public ProjectCreator SetupNXPortsProject(string projectFilePath, string targetFramework = "net48")
        {
            var dir = GetApplicationDirectory();
            File.WriteAllText(GetAbsolutePath("Directory.Build.props"), "<Project />");
            File.WriteAllText(GetAbsolutePath("Directory.Build.targets"), "<Project />");
            var feed = PackageFeed.Create(GetAbsolutePath("feed/"))
                .Package("NXPorts", "199.199.199")
                .FileText("build/NXPorts.targets", File.ReadAllText(Path.Combine(dir, "build", "NXPorts.targets")))
                .Save();
            return ProjectCreator.Templates.SdkCsproj(projectFilePath, targetFramework: targetFramework)
                .Property("RestoreSources", "./feed;$(RestoreSources)")
                .Property("NXPortsTaskAssemblyDirectory", dir + "\\")
                .Property("PlatformTarget", Environment.Is64BitProcess ? "x64" : "x86")
                .ItemPackageReference(feed.Packages.First())
                .ItemReference(new Uri(Assembly.GetAssembly(typeof(DllExportAttribute)).CodeBase).LocalPath);
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath, string destinationPath)
        {
            File.Copy(Path.Combine(GetApplicationDirectory(), "TestFiles", relativeTestFilesPath), GetAbsolutePath(destinationPath));
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath)
        {
            CopyFileFromTestFiles(relativeTestFilesPath, relativeTestFilesPath);
        }

        public (IAnalyzerResults AnalyzerResults, BuildOutput Log) Build(string projectFilePath, bool designTime = false, bool clean = true)
        {
            var projectAnalyzer = new AnalyzerManager().GetProject(projectFilePath);
            var logger = BuildOutput.Create();
            projectAnalyzer.AddBuildLogger(logger);
            projectAnalyzer.AddBinaryLogger(GetAbsolutePath("build.binlog"));
            var envOptions = new EnvironmentOptions()
            {
                DesignTime = designTime
            };
            envOptions.TargetsToBuild.Clear();
            if (clean)
                envOptions.TargetsToBuild.Add("Clean");
            envOptions.TargetsToBuild.Add("Build");
            var analyzerResults = projectAnalyzer.Build(
                envOptions
            );
            return (analyzerResults, logger);
        }


        private static string GetApplicationDirectory()
        {
            return new Uri(Path.GetDirectoryName(Assembly.GetAssembly(typeof(TestEnvironment)).CodeBase)).LocalPath;
        }

        /// <summary>
        /// Enumerate a list of <see cref="MetadataReference"/> for Roslyn to use for type resolutions
        /// </summary>
        private static IEnumerable<MetadataReference> GetRelevantReferences()
        {
            return new List<MetadataReference>(new[] {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DllExportAttribute).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))
            });
        }

        public void Dispose()
        {
            //TODO: Remove IDisposable
            GC.SuppressFinalize(this);
        }
    }
}

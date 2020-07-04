using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Buildalyzer;
using Buildalyzer.Environment;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NXPorts.Attributes;

namespace NXPorts.Tests.Infrastructure
{
    public class TestEnvironment : IDisposable
    {
        readonly string oldWorkingDirectory = Environment.CurrentDirectory;
        public TestEnvironment()
        {
            var testPWD = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            );
            Environment.CurrentDirectory = testPWD.FullName;
        }

        /// <summary>
        /// Dynamically generates a DLL in the current working directory with a given set of syntax trees.
        /// </summary>
        /// <param name="assemblyName">The name of the DLL file and the underlying assembly.</param>
        /// <param name="csharpDocuments">A set of C# documents</param>
        /// <param name="platform">The target platform of the DLL</param>
        /// <returns>Returns True if the dynamic compilation succeeds.</returns>
        /// <remarks>The dynamic compilation will only work if the types used in the code to be compiled are also referenced in some way by the test AppContext.</remarks>
        public bool CreateTestDLL(string assemblyName,IEnumerable<string> csharpDocuments, Platform platform)
        {
            var syntaxTrees = (csharpDocuments == null) ? throw new ArgumentNullException(nameof(csharpDocuments)) : csharpDocuments.Select(doc => CSharpSyntaxTree.ParseText(doc));
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                platform: platform
            );
            var csc = CSharpCompilation.Create(assemblyName, syntaxTrees, GetRelevantReferences(), options);

            using(var dllFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, assemblyName + ".dll"),FileMode.OpenOrCreate))
            using (var pdbFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, assemblyName + ".pdb"),FileMode.OpenOrCreate))
            {
                var emitResult = csc.Emit(dllFileStream,pdbFileStream);
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
        public bool CreateTestDLL(string assemblyName,IEnumerable<string> csharpDocuments)
        {
            if (Environment.Is64BitProcess)
                return CreateTestDLL(assemblyName, csharpDocuments, Platform.X64);
            else
                return CreateTestDLL(assemblyName, csharpDocuments, Platform.X86);
        }

        public ProjectCreator SetupNXPortsProject(string projectFilePath = "./test.csproj", string targetFramework = "net48")
        {
            string dir = GetApplicationDirectory();
            File.WriteAllText("Directory.Build.props", "<Project />");
            File.WriteAllText("Directory.Build.targets", "<Project />");
            return ProjectCreator.Templates.SdkCsproj(projectFilePath, targetFramework: targetFramework)
                .Property("NXPortsTaskAssemblyDirectory", dir + "\\")
                .Property("PlatformTarget", Environment.Is64BitProcess ? "x64" : "x86" )
                .ItemReference(new Uri(Assembly.GetAssembly(typeof(DllExportAttribute)).CodeBase).LocalPath)
                .Import(Path.Combine(dir, "Build", "NXPorts.targets"));
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath, string destinationPath)
        {
            File.Copy(Path.Combine(GetApplicationDirectory(), "TestFiles", relativeTestFilesPath), destinationPath);
        }

        public void CopyFileFromTestFiles(string relativeTestFilesPath)
        {
            CopyFileFromTestFiles(relativeTestFilesPath, relativeTestFilesPath);
        }

        public (AnalyzerResults AnalyzerResults, BuildOutput Log) Build(string projectFilePath, bool designTime = false)
        {
            var projectAnalyzer = new AnalyzerManager().GetProject(projectFilePath);
            var logger = BuildOutput.Create();
            projectAnalyzer.AddBuildLogger(logger);
            projectAnalyzer.AddBinaryLogger("build.binlog");
            var analyzerResults = projectAnalyzer.Build(
                new EnvironmentOptions()
                {
                    DesignTime = designTime
                }
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
                MetadataReference.CreateFromFile(typeof(NXPorts.Attributes.DllExportAttribute).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))
            });
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = oldWorkingDirectory;
            //Ideally i wanted to remove all files after the environment is disposed
            //but due to some hardships with unloading unmanaged assemblies loaded using
            //PInvoke we have to make due with what we have.
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var csc = CSharpCompilation.Create(assemblyName,syntaxTrees,GetCurrentReferencedAssemblies(),options);

            using(var dllFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory,assemblyName + ".dll"),FileMode.OpenOrCreate))
            using (var pdbFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory,assemblyName + ".pdb"),FileMode.OpenOrCreate))
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
                .ItemReference(new Uri(Assembly.GetAssembly(typeof(ExportAttribute)).CodeBase).LocalPath)
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
        private static string GetApplicationDirectory()
        {
            return new Uri(Path.GetDirectoryName(Assembly.GetAssembly(typeof(TestEnvironment)).CodeBase)).LocalPath;
        }

        /// <summary>
        /// Enumerates all currently referenced assemblies and produces MetadataReference items for them.
        /// </summary>
        /// <remarks>
        /// DIRTY HACK.... but i can't be bothered. Out of the scope of the project to provide nice runtime compilation capabilities.
        /// </remarks>
        private static IEnumerable<MetadataReference> GetCurrentReferencedAssemblies()
        {
#if NETCORE
            var runtimeAssemblyLocations = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(';');
#else
            var domainAssemblies = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            var runtimeAssemblyLocations = new HashSet<String>(
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
            );
            foreach (var ass in domainAssemblies)
            {
                runtimeAssemblyLocations.UnionWith(ass.GetReferencedAssemblies()
                    .Where(
                        a => {
                            try
                            {
                                Assembly.Load(a);
                                return true;
                            } catch
                            {
                                return false;
                            }
                        }
                    ).Select(a => Assembly.Load(a).Location)
                );
            }
#endif
            foreach (var assemblyPath in runtimeAssemblyLocations)
            {
                yield return MetadataReference.CreateFromFile(assemblyPath);
            }
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
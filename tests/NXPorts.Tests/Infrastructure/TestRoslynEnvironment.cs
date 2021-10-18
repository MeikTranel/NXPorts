using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NXPorts.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NXPorts.Tests.Infrastructure
{
    public class TestRoslynEnvironment : TestEnvironment
    {
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet;

namespace NXPorts
{
    public sealed class ExportAttributedAssembly : IDisposable
    {
        public IList<ExportDefinition> ExportDefinitions { get; }

        public ExportAttributedAssembly(string assemblyFilePath)
        {
            if (string.IsNullOrWhiteSpace(assemblyFilePath))
                throw new ArgumentNullException(nameof(assemblyFilePath));
            if (!File.Exists(assemblyFilePath))
                throw new ArgumentException("The given file path does not exist.", nameof(assemblyFilePath));

            var assemblyData = ReadAssemblyData(assemblyFilePath);
            Module = ProduceModuleDefinition(assemblyData);
            ExportDefinitions = new List<ExportDefinition>(RetrieveExportDefinitions());
        }

        public ModuleDefMD Module { get; private set; }

        private IEnumerable<ExportDefinition> RetrieveExportDefinitions()
        {
            var allTypes = this.Module.Types;
            foreach (var type in allTypes)
            {
                foreach(var method in type.Methods)
                {
                    if(method.CustomAttributes.IsDefined(new Attributes.ExportAttribute().GetType().FullName))
                    {
                        var attributeRef = method.CustomAttributes.Find(new Attributes.ExportAttribute().GetType().FullName);
                        var expDef = ExportDefinition.Create(method,attributeRef);
                        yield return expDef;
                    }
                }
            }
        }

        private static byte[] ReadAssemblyData(string assemblyFilePath)
        {
            try
            {
                return File.ReadAllBytes(assemblyFilePath);
            }
            catch (DirectoryNotFoundException E)
            {
                throw new InvalidOperationException($"Could not find the assembly at path '{assemblyFilePath}'.", E);
            }
            catch (Exception E)
            {
                throw new InvalidOperationException("Something went wrong while trying to load the assembly data.", E);
            }
        }

        private static ModuleDefMD ProduceModuleDefinition(byte[] assemblyData)
        {
            try
            {
                return ModuleDefMD.Load(assemblyData);
            }
            catch (Exception E)
            {
                throw new InvalidOperationException("DNLib encountered an exception while trying to load", E);
            }
        }

        #region IDisposable Support
        public void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(Module != null)
                {
                    Module.Dispose();
                    Module = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
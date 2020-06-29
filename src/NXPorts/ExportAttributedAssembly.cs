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
            try
            {
                Module = ModuleDefMD.Load(
                    assemblyFilePath,
                    new ModuleCreationOptions
                    {
                        TryToLoadPdbFromDisk = true
                    }
                );
            }
            catch (Exception E)
            {
                throw new InvalidOperationException("NXPorts encountered an exception while trying to load the source assembly.", E);
            }
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
                    if(method.CustomAttributes.IsDefined(new Attributes.DllExportAttribute().GetType().FullName))
                    {
                        var attributeRef = method.CustomAttributes.Find(new Attributes.DllExportAttribute().GetType().FullName);
                        var expDef = ExportDefinition.Create(method,attributeRef);
                        yield return expDef;
                    }
                }
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
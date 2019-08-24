using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.PE;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NXPorts.Attributes;
using System;
using System.Diagnostics;
using System.IO;
using SysInterop = System.Runtime.InteropServices;

namespace NXPorts
{
    public sealed class AssemblyExportWriterTask : Task
    {
        [Required]
        public string InputAssembly { get; private set; }

        [Required]
        [Output]
        public string OutputPath { get; private set; }


        public override bool Execute()
        {
            using (var expAttributedAssembly = new ExportAttributedAssembly(InputAssembly))
            {
                Write(expAttributedAssembly, OutputPath);                    
                return Log.HasLoggedErrors;
            }
        }

        public void Write(ExportAttributedAssembly sourceAssembly, string outputPath)
        {
            if(sourceAssembly == null)
                throw new ArgumentNullException(nameof(sourceAssembly));

            using (FileStream outputStream = File.OpenWrite(outputPath))
            {
                foreach (var exportDefinition in sourceAssembly.ExportDefinitions)
                {
                    var returnType = exportDefinition.MethodDefinition.MethodSig.RetType;
                    exportDefinition.MethodDefinition.ExportInfo = new MethodExportInfo(exportDefinition.Alias);
                    exportDefinition.MethodDefinition.MethodSig.RetType = new CModOptSig(
                        sourceAssembly.Module.CorLibTypes.GetTypeRef(
                            "System.Runtime.CompilerServices",
                            ResolveCallingConventionCompilerServicesType(exportDefinition.CallingConvention)
                        ),
                        returnType
                    );
                    exportDefinition.MethodDefinition.CustomAttributes.RemoveAll(typeof(ExportAttribute).FullName);
                }
                RemoveToxicDebuggableAttribute(sourceAssembly.Module);

                var moduleWriterOptions = new ModuleWriterOptions(sourceAssembly.Module);
                moduleWriterOptions.Cor20HeaderOptions.Flags = StrictenCor20HeaderFlags(moduleWriterOptions.Cor20HeaderOptions.Flags);
                moduleWriterOptions.Cor20HeaderOptions.Flags &= ~ComImageFlags.ILOnly;
                moduleWriterOptions.PEHeadersOptions.Characteristics |= Characteristics.Dll;

                sourceAssembly.Module.Write(outputStream, moduleWriterOptions);
            }
        }

        private static void RemoveToxicDebuggableAttribute(ModuleDefMD module)
        {
            var ca = module.Assembly.CustomAttributes.Find("System.Diagnostics.DebuggableAttribute");
            if (ca != null && ca.ConstructorArguments.Count == 1)
            {
                var arg = ca.ConstructorArguments[0];
                // VS' debugger crashes if value == 0x107, so clear EnC bit
                if (arg.Type.FullName == "System.Diagnostics.DebuggableAttribute/DebuggingModes" && arg.Value is int value && value == 0x107)
                {
                    arg.Value = value & ~(int)DebuggableAttribute.DebuggingModes.EnableEditAndContinue;
                    ca.ConstructorArguments[0] = arg;
                }
            }
        }

        private static ComImageFlags? StrictenCor20HeaderFlags(ComImageFlags? comImageFlags)
        {
            if (comImageFlags.HasValue && comImageFlags.Value.HasFlag(ComImageFlags.Bit32Preferred))
                return ( comImageFlags & ~ComImageFlags.Bit32Preferred ) | ComImageFlags.Bit32Required;
            else
                return comImageFlags;
        }

        private static string ResolveCallingConventionCompilerServicesType(
            SysInterop.CallingConvention callingConvention
        )
        {
            switch (callingConvention)
            {
                case SysInterop.CallingConvention.Cdecl:
                    return "CallConvCdecl";
                case SysInterop.CallingConvention.ThisCall:
                    return "CallConvThiscall";
                case SysInterop.CallingConvention.StdCall:
                    return "CallConvStdcall";
                case SysInterop.CallingConvention.FastCall:
                    return "CallConvFastcall";
                case SysInterop.CallingConvention.Winapi:
                default:
                    throw new NotSupportedException($"{callingConvention} is not supported for Reverse PInvoke!");
            }
        }
    }
}

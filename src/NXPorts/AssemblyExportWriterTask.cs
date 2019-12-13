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
            try
            {
                using (var expAttributedAssembly = new ExportAttributedAssembly(InputAssembly))
                {
                    Log.LogMessage(MessageImportance.Normal, $"Found {expAttributedAssembly.ExportDefinitions.Count} annotated method(s) ready for reweaving.");
                    Write(expAttributedAssembly, OutputPath);
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true, true, null);
            }
            return !Log.HasLoggedErrors;
        }

        public void Write(ExportAttributedAssembly sourceAssembly, string outputPath)
        {
            if (sourceAssembly == null)
                throw new ArgumentNullException(nameof(sourceAssembly));

            if (sourceAssembly.ExportDefinitions.Count > 0)
            {
                foreach (var exportDefinition in sourceAssembly.ExportDefinitions)
                {
                    RewriteAnnotatedMethod(sourceAssembly, exportDefinition);
                }

                Log.LogMessage(MessageImportance.Low, "Clearing assembly of incompatible assembly flags.");
                RemoveToxicDebuggableAttribute(sourceAssembly.Module);

                Log.LogMessage(MessageImportance.Low, "Adjusting PE32 header to reflect the reweaving changes to the assembly file.");
                var moduleWriterOptions = new ModuleWriterOptions(sourceAssembly.Module);
                moduleWriterOptions.Cor20HeaderOptions.Flags = StrictenCor20HeaderFlags(moduleWriterOptions.Cor20HeaderOptions.Flags);
                moduleWriterOptions.Cor20HeaderOptions.Flags &= ~ComImageFlags.ILOnly;
                moduleWriterOptions.PEHeadersOptions.Characteristics |= Characteristics.Dll;

                using (FileStream outputStream = File.OpenWrite(outputPath))
                {
                    Log.LogMessage(MessageImportance.Low, "Writing the new assembly file to disk...");
                    sourceAssembly.Module.Write(outputStream, moduleWriterOptions);
                    Log.LogMessage(MessageImportance.Normal, $"Successfully rewritten assembly at '{outputPath}'.");
                }
            } 
            else
            {
                Log.LogWarning("No method annotations for export reweaving were found.");
            }
        }

        private void RewriteAnnotatedMethod(ExportAttributedAssembly sourceAssembly, ExportDefinition exportDefinition)
        {
            var message = $"Reweaving method '{exportDefinition.MethodDefinition.FullName}' with alias '{exportDefinition.Alias}' and calling convention '{exportDefinition.CallingConvention}'";
            if (exportDefinition.TryApproximateMethodSourcePosition(out var sourcePosition))
            {
                Log.LogMessage(
                    subcategory: null, code: null, helpKeyword: null,
                    file: sourcePosition.FilePath, lineNumber: sourcePosition.Line ?? 0, columnNumber: sourcePosition.Column ?? 0, endLineNumber: 0, endColumnNumber: 0,
                    MessageImportance.Low, message
                );
            }
            else
            {
                Log.LogMessage(MessageImportance.Low, message);
            }
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

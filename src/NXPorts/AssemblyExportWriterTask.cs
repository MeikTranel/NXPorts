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
using System.Linq;
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

        [Required]
        public bool AllowCaseSensitiveDuplicates { get; private set; }


        public AssemblyExportWriterTask()
        {
            TaskResources = Properties.Resources.ResourceManager;
        }

        public override bool Execute()
        {
            try
            {
                using (var expAttributedAssembly = new ExportAttributedAssembly(InputAssembly))
                {
                    if (!ValidateAttributedAssembly(expAttributedAssembly))
                    {
                        Log.LogErrorFromResources("Log_ValidationFailAborting");
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.Normal, $"Found {expAttributedAssembly.ExportDefinitions.Count} annotated method(s) ready for reweaving.");
                        Write(expAttributedAssembly, OutputPath);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true, true, null);
            }
            return !Log.HasLoggedErrors;
        }

        #region Validation
        private bool ValidateAttributedAssembly(ExportAttributedAssembly expAttributedAssembly)
        {
            return !AssemblyContainsDuplicateExportAliases(expAttributedAssembly) &&
                    PassesCaseInsensitiveAliasesCheck(expAttributedAssembly);
        }

        private bool PassesCaseInsensitiveAliasesCheck(ExportAttributedAssembly expAttributedAssembly)
        {
            var groupedExports = expAttributedAssembly.ExportDefinitions.GroupBy(def => def.Alias, StringComparer.InvariantCultureIgnoreCase);
            if (groupedExports.Any(d => d.Count() >= 2))
            {
                foreach (var group in groupedExports.Where(d => d.Count() >= 2))
                {
                    if (AllowCaseSensitiveDuplicates)
                        Log.LogWarningWithCodeFromResources(Diagnostics.DuplicateAliasesWithDifferentCaps.MessageResourceKey, group.Key);
                    else
                        Log.LogErrorWithCodeFromResources(Diagnostics.DuplicateAliases.MessageResourceKey, group.Key);
                }
                if (AllowCaseSensitiveDuplicates)
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }
        }

        private bool AssemblyContainsDuplicateExportAliases(ExportAttributedAssembly expAttributedAssembly)
        {
            var groupedExports = expAttributedAssembly.ExportDefinitions.GroupBy(def => def.Alias, StringComparer.InvariantCulture);
            if (groupedExports.Any(d => d.Count() >= 2))
            {
                foreach (var group in groupedExports.Where(d => d.Count() >= 2))
                {
                    Log.LogErrorWithCodeFromResources(Diagnostics.DuplicateAliases.MessageResourceKey, group.Key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Reweaving/Writing
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

                Log.LogMessageFromResources(MessageImportance.Low, "Log_ClearingFlags");
                RemoveToxicDebuggableAttribute(sourceAssembly.Module);

                Log.LogMessageFromResources(MessageImportance.Low, "Log_AdjustingPE32Header");
                var moduleWriterOptions = new ModuleWriterOptions(sourceAssembly.Module)
                {
                    WritePdb = true
                };
                moduleWriterOptions.Cor20HeaderOptions.Flags = StrictenCor20HeaderFlags(moduleWriterOptions.Cor20HeaderOptions.Flags);
                moduleWriterOptions.Cor20HeaderOptions.Flags &= ~ComImageFlags.ILOnly;
                moduleWriterOptions.PEHeadersOptions.Characteristics |= Characteristics.Dll;

                using (var outputStream = File.OpenWrite(outputPath))
                {
                    Log.LogMessageFromResources(MessageImportance.Low, "Log_WritingToDisk");
                    sourceAssembly.Module.Write(outputStream, moduleWriterOptions);
                    Log.LogMessageFromResources(MessageImportance.Normal, "Log_SuccessWrittenAt", outputPath);
                }
            }
            else
            {
                Log.LogWarningWithCodeFromResources("Diag_NoMethodAnnotationsFoundMessage");
            }
        }

        private void RewriteAnnotatedMethod(ExportAttributedAssembly sourceAssembly, ExportDefinition exportDefinition)
        {
            var message = string.Format(Properties.Resources.Log_ReweavingMethod, exportDefinition.MethodDefinition.FullName, exportDefinition.Alias, exportDefinition.CallingConvention);
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
            exportDefinition.MethodDefinition.CustomAttributes.RemoveAll(typeof(DllExportAttribute).FullName);
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
                return (comImageFlags & ~ComImageFlags.Bit32Preferred) | ComImageFlags.Bit32Required;
            else
                return comImageFlags;
        }

        private static string ResolveCallingConventionCompilerServicesType(
            SysInterop.CallingConvention callingConvention
        )
        {
            return callingConvention switch
            {
                SysInterop.CallingConvention.Cdecl => "CallConvCdecl",
                SysInterop.CallingConvention.ThisCall => "CallConvThiscall",
                SysInterop.CallingConvention.StdCall => "CallConvStdcall",
                SysInterop.CallingConvention.FastCall => "CallConvFastcall",
                _ => throw new NotSupportedException(string.Format(Properties.Resources.Log_UnsupportedCallingConvention, callingConvention)),
            };
        }
        #endregion
    }
}

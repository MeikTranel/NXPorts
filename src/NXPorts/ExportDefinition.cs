using dnlib.DotNet;
using System;
using System.Linq;
using SysInterop = System.Runtime.InteropServices;

namespace NXPorts
{
    // TODO: Make internal again
    public sealed class ExportDefinition : IEquatable<ExportDefinition>
    {
        public MethodDef MethodDefinition { get; }

        public string Alias { get; private set; }

        public SysInterop.CallingConvention CallingConvention { get; private set; }

        ExportDefinition(MethodDef methodDefinition, SysInterop.CallingConvention callingConvention, string alias)
        {
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
            CallingConvention = callingConvention;
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }

        public static ExportDefinition Create(MethodDef method, CustomAttribute attributeRef)
        {
            var alias = method.Name;
            if(TryGetCustomExportAlias(attributeRef, out var customAlias))
            {
                alias = customAlias;
            }
            var callingConvention = GetCallingConvention(attributeRef);
            return new ExportDefinition(method, callingConvention, alias);
        }

        static SysInterop.CallingConvention GetCallingConvention(CustomAttribute attributeRef)
        {
            var callingConvCA = attributeRef.ConstructorArguments.Where(ca => ca.Type.FullName == typeof(SysInterop.CallingConvention).FullName).First();
            return (SysInterop.CallingConvention) callingConvCA.Value;
        }

        static bool TryGetCustomExportAlias(CustomAttribute attributeRef, out string customAlias)
        {
            customAlias = "";
            //TODO: Find better / more safe solutions
            try {
                var CA = attributeRef.ConstructorArguments.Where(ca => ca.Type.TypeName == "String").First();
                var constructorArgumentValue = CA.Value.ToString();
                if(!string.IsNullOrEmpty(constructorArgumentValue))
                {
                    customAlias = constructorArgumentValue;
                    return true;
                }
            } catch {}
            return false;
        }

        /// <remarks>
        ///   PE32 identifies exports by the ordinal value.
        ///   A export name is not explicitly required and only explicitly referenced through
        ///   an additional lookup table identifying the ordinal with a 0-terminated ASCII string.
        ///   Until someone explicitly expresses the wish to have unnamed exports,
        ///   we assume that 100% of all consumers want their exports to be named, which makes the
        ///   export name the de facto identifier.
        /// </remarks>
        public bool Equals(ExportDefinition other)
        {
            return this.Alias.Equals(other.Alias);
        }

        public bool TryApproximateMethodSourcePosition(out SourcePosition sourcePosition)
        {
            sourcePosition = null;
            var firstILInstruction = this.MethodDefinition.Body.Instructions.First();
            var sequencePoint = firstILInstruction.SequencePoint;
            if (sequencePoint != null) {
                sourcePosition = new SourcePosition(
                    filePath: sequencePoint.Document.Url,
                    line: sequencePoint.StartLine - 1,
                    column: sequencePoint.StartColumn
                );
                return true;
            } else {
                return false;
            }
        }
    }
}

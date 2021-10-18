using dnlib.DotNet;
using System;
using System.Linq;
using SysInterop = System.Runtime.InteropServices;

namespace NXPorts
{
    public sealed class ExportDefinition : IEquatable<ExportDefinition>
    {
        public MethodDef MethodDefinition { get; }

        public string Alias { get; private set; }

        public SysInterop.CallingConvention CallingConvention { get; private set; }

        private ExportDefinition(MethodDef methodDefinition, SysInterop.CallingConvention callingConvention, string alias)
        {
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
            CallingConvention = callingConvention;
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }

        public static ExportDefinition Create(MethodDef method, CustomAttribute attributeRef)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));
            if (attributeRef is null)
                throw new ArgumentNullException(nameof(attributeRef));

            var alias = method.Name;
            if (TryGetCustomExportAlias(attributeRef, out var customAlias))
            {
                alias = customAlias;
            }
            var callingConvention = GetCallingConvention(attributeRef);
            return new ExportDefinition(method, callingConvention, alias);
        }

        private static SysInterop.CallingConvention GetCallingConvention(CustomAttribute attributeRef)
        {
            var callingConvCA = attributeRef.ConstructorArguments.First(ca => ca.Type.FullName == typeof(SysInterop.CallingConvention).FullName);
            return (SysInterop.CallingConvention)callingConvCA.Value;
        }

        private static bool TryGetCustomExportAlias(CustomAttribute attributeRef, out string customAlias)
        {
            customAlias = "";
            var CA = attributeRef.ConstructorArguments.First(ca => ca.Type.TypeName == "String");
            if (CA.Value is not null)
            {
                customAlias = CA.Value.ToString();
                return true;
            }
            return false;
        }


        public bool TryApproximateMethodSourcePosition(out SourcePosition sourcePosition)
        {
            sourcePosition = null;
            var firstILInstruction = MethodDefinition.Body.Instructions.First();
            var sequencePoint = firstILInstruction.SequencePoint;
            if (sequencePoint != null)
            {
                sourcePosition = new SourcePosition(
                    filePath: sequencePoint.Document.Url,
                    line: sequencePoint.StartLine - 1,
                    column: sequencePoint.StartColumn
                );
                return true;
            }
            else
            {
                return false;
            }
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
            if (other is null)
            {
                return false;
            }
            return Alias.Equals(other.Alias, StringComparison.InvariantCultureIgnoreCase);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ExportDefinition);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

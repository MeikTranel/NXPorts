using System;
using System.Runtime.InteropServices;

namespace NXPorts.Attributes
{
    [AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ExportAttribute : Attribute
    {
        public string Alias { get; private set; }

        public CallingConvention CallingConvention { get; private set; }

        public ExportAttribute(string alias = null, CallingConvention callingConvention = CallingConvention.Cdecl)
        {
            this.Alias = alias;
            this.CallingConvention = callingConvention;
        }
    }
}

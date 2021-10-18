using System;
using System.Runtime.InteropServices;

namespace NXPorts.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DllExportAttribute : Attribute
    {
        public string Alias { get; private set; }

        public CallingConvention CallingConvention { get; private set; }

        public DllExportAttribute(string alias = null, CallingConvention callingConvention = CallingConvention.Cdecl)
        {
            Alias = alias;
            CallingConvention = callingConvention;
        }
    }
}

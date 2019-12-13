using Microsoft.Build.Framework;
using System.Diagnostics;

[assembly: DebuggerDisplay("{Message,nq}",Target = typeof(BuildEventArgs))]
[assembly: DebuggerDisplay("W: {Message,nq}",Target = typeof(BuildWarningEventArgs))]
[assembly: DebuggerDisplay("E: {Message,nq}",Target = typeof(BuildErrorEventArgs))]
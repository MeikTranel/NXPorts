using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


[assembly: DebuggerDisplay("{Message,nq}", Target = typeof(BuildEventArgs))]
[assembly: DebuggerDisplay("W: {Message,nq}", Target = typeof(BuildWarningEventArgs))]
[assembly: DebuggerDisplay("E: {Message,nq}", Target = typeof(BuildErrorEventArgs))]

// Suppressions

[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Does this make you angry, Mr. FSharp Dev?")]

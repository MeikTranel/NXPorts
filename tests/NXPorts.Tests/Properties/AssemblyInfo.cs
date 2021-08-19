using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

//Debugger viz config
[assembly: DebuggerDisplay("{Message,nq}", Target = typeof(BuildEventArgs))]
[assembly: DebuggerDisplay("W: {Message,nq}", Target = typeof(BuildWarningEventArgs))]
[assembly: DebuggerDisplay("E: {Message,nq}", Target = typeof(BuildErrorEventArgs))]

// Suppressions
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Does this make you angry, Mr. FSharp Dev?")]

// Speed up test execution by parallelizing at method level
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

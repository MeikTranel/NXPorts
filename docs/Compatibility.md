# Compatibility

To determine compatibility, we first have to differentiate between three kinds of compatibilities:

- The ability to compile using NXPort exposed types (TargetFramework of the package itself)
- The ability to reweave the assembly to eventually have exports registered in the PE32 container (MSBuild NETFX/NETCore Runtimes)
- The ability to actually use the PInvoke APIs exposed by the reweave process (Platform Invocation support provided by the dotnet runtime)

## The Package

The package itself currently targets .NET Standard 1.1, because that's the lowest version that
exposes `System.Runtime.InteropServices.CallingConvention`. There's no particular reason why we
need this type specifically (we could have rolled our own), but the existence of a type we use
was convenient. The compatibility is way more dependent on platform support anyways.

## The Build Tasks

The MSBuild Tasks at the core of the reweave process support both .NET Framework as well as .NET Core
versions of the MSBuild runtime. Any 2.0+ install of the dotnet sdk, as well as any mildly recent install
of Visual Studio 2017 or higher will suffice here.

## The Platform Support

PInvoking `NXPorts` exposed exports is guaranteed to work with .NET Framework versions upwards of .NET Framework 4.
.NET Framework is of course Windows-only.
Support for .NET Core is questionable. You will be able to `LoadLibrary` and resolve the function pointer successfully.
But when invoking the delegate chances are that you will be greeted with something like a `SEHException` or something
similar.
With .NET Core 3.0 there has been significant work towards enabling PInvoke and COM interop. As of right now there is no
first class support for our scenario yet.
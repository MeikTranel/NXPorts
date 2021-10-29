# NXPorts

[![CI](https://github.com/MeikTranel/NXPorts/workflows/CI/badge.svg)](https://github.com/MeikTranel/NXPorts/actions)
[![codecov](https://codecov.io/gh/MeikTranel/NXPorts/branch/master/graph/badge.svg?token=DPMCO9NGN5)](https://codecov.io/gh/MeikTranel/NXPorts)

A MSBuild-integrated library/tool to expose entrypoints in .NET assemblies to the platform invocation system or short `PInvoke`.
It allows you to build .NET libraries that can be called from any development platform that supports `PInvoke`, including **C++**, **C**, **Rust**, **Delphi**, **Python** and so on...

## Getting Started

To get started you only have to add the `NXPorts` package to your project. That's it. `NXPorts` will automatically generate exports as
part of your build now. Some samples are available [here](https://github.com/MeikTranel/NXPorts/blob/master/samples).

### Prerequisites

This software itself will run on any system that supports developing .NET software - it does **not** have any dependencies on
`ildasm.exe`, `ilasm.exe` or any other windows-only libraries.

Keep in mind that `Reverse PInvoke` - calling PInvoke entrypoints **in .NET assemblies** - is only officially supported
in the .NET Framework. It may work in other runtimes as well, but your mileage may vary.

See [Compatibility Docs](https://github.com/MeikTranel/NXPorts/blob/master/docs/Compatibility.md) for more info.

### Sample

The following code will result in an exported function named `SampleExportFunc`:

```CSharp
public static class Exports {
    [DllExport(alias:"SampleExportFunc")]
    public static double Add(double a, double b)
    {
        return a + b;
    }
}
```

The following code will result in an exported function named `Sum`:

```CSharp
public static class Exports {
    [DllExport]
    public static double Sum(double a, double b)
    {
        return a + b;
    }
}
```

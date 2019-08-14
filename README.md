# NXPorts

A MSBuild-integrated library/tool to expose entrypoints in .NET assemblies to the platform invocation system or short `PInvoke`.
It allows you to build .NET libraries that can be called from any development platform that supports `PInvoke`, including **C++**, **C**, **Rust**, **Delphi**, **Python** and so on...

## Status

| Channel | Build | Package |
|---------|-------|---------|
| Canary  |[![Build Status](https://dev.azure.com/tranelmeik/NXPorts/_apis/build/status/Canary%20Builds?branchName=master)](https://dev.azure.com/tranelmeik/NXPorts/_build/latest?definitionId=3&branchName=master)|         |
| Release |[![Build Status](https://dev.azure.com/tranelmeik/NXPorts/_apis/build/status/Releases?branchName=master)](https://dev.azure.com/tranelmeik/NXPorts/_build/latest?definitionId=2&branchName=master)|         |

## Getting Started

To get started you only have to add the `NXPorts` package to your project. That's it. `NXPorts` will automatically generate exports as
part of your build now.

### Prerequisites

This software itself will run on any system that supports developing .NET software - it does **not** have any dependencies on
`ildasm.exe`, `ilasm.exe` or any other windows-only libraries.

Keep in mind that `Reverse PInvoke` - calling PInvoke entrypoints **in .NET assemblies** - is only officially supported
in the .NET Framework. It may work in other runtimes as well, but your mileage may vary.

### Sample

The following code will result in an exported function named `SampleExportFunc`.

```CSharp
public static class Exports {
  [NXPorts.Attributes.Export(alias:"SampleExportFunc")]
  public static double Add(double a, double b)
  {
      return a + b;
  }
}
```

## Building from Source

To build packages by yourself all you have to do is the dotnet cli like this:

```shell
  dotnet pack
```

This will restore, build and pack all available projects. Testing this project is just as simple:

```shell
  dotnet test
```

More information about the release process can be found [here](./docs/Releasing-NXPorts.md).

## Contributing

I am more than happy to take in PRs.
Documentation regarding Marshalling, PE32 or PInvoke (especially on non-NETFX runtimes) counts
just as much as a valuable contribution as any other code PR.

To me writing issues is also just as valuable. I may not list every issue reporter in the release notes, but
especially during the initial development timeline knowing the shortcomings of the software is absolutely invaluable.

### Guidelines

* Please consider writing an issue first
* Please target PRs towards the `master` branch

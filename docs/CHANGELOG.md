## Changes in v

* **[BUGFIX]** #65 Fixed a bug where a subsequent build would result in a failed removal of the NXPorts.Attributes.dll (which is obsolete once the dll has been reweaved)
* **[FEATURE]** #76 Added diagnostics to detect and warn/error on duplicate export definitions
  * Due to being technically legal exports that differ only in capitalization now throw a warning.
  * If you want to guard against those cases as well, set `NXPortsAllowCaseSensitiveDuplicates` to `false`.
* **Updated Dependencies**:
  * **dnlib** [v3.3.1 -> v3.3.5](https://github.com/0xd4d/dnlib/compare/v3.3.1...v3.3.5)

## Changes in v1.0

The changes that went into this release are primarily aimed at resolving feedback received from users as well as improve upon accessibility.
This release probably does not achieve 1:1 feature parity with UnmanagedExports, but it should cover the 99% use-case.

* **[BREAKINGCHANGE]** #7 Renamed `ExportAttribute` to `DllExportAttribute`
* **[BUGFIX]** #16 Fixed symbols not matching after reweaving
* **[BUGFIX]** #17 Fixed incremental building issues resulting in `No method annotations found...` warnings

## Changes in v0.2

This release and future minor releases will be focused on getting a initial design spec and a drop-in replacement-esque status with the RGiesecke.DLLExport.
Once we have a solid base, we will bump to v1 without any breaking changes from the last v0.X release.

* #13 Fixed export writing task confusing success and failure (Thanks, @filmor)
* #10 Fixed a bug where loading a NXPorts enabled project in designtime build mode (e.g. Visual Studio) would be blocked by a failing AssemblyExportWriterTask
* #14 NXPorts now removes the obsolete NXPorts.Attributes.dll from the build output
* Significantly increased logging output to support diagnosing issues when using NXPorts.
* Added a warning informing the user when NXPorts did not find any annotated method.
* #12 NXPorts now throws an error whenever it detects an incompatible project configuration in regards to target platform.

## Changes in v0.1

This release and future minor releases will be focused on getting a initial design spec and a drop-in replacement-esque status with the RGiesecke.DLLExport.
Once we have a solid base, we will bump to v1 without any breaking changes from the last v0.X release.

* Ability to re-weave the IL of a given DLL to generate VTFixup entries for methods decorated with NXPorts.Attributes.ExportAttribute
* Ability to use custom names for your export functions
* NXPorts will automatically remove the Prefer32Bit flag and change it to require 32bit
* NXPorts can be added via NuGet package and the export reweave process happens on build
* Attributes used to mark the desired exports will now be removed, thus removing NXPorts.Attributes.dll from the assembly dependencies.

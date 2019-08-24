<!-- markdownlint-disable MD041 -->
This release and future minor releases will be focused on getting a initial design spec and a drop-in replacement-esque status with the `RGiesecke.DLLExport`.
Once we have a solid base, we will bump to v1 without any breaking changes from the last v0.X release.

* Ability to re-weave the IL of a given DLL to generate VTFixup entries for methods decorated with `NXPorts.Attributes.ExportAttribute`
* Ability to use custom names for your export functions
* NXPorts will automatically remove the Prefer32Bit flag and change it to require 32bit
* NXPorts can be added via NuGet package and the export reweave process happens on build
* Attributes used to mark the desired exports will now be removed, thus removing NXPorts.Attributes.dll from the assembly dependencies.
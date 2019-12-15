<!-- markdownlint-disable MD041 -->
This release and future minor releases will be focused on getting a initial design spec and a drop-in replacement-esque status with the `RGiesecke.DLLExport`.
Once we have a solid base, we will bump to v1 without any breaking changes from the last v0.X release.

* #13 Fixed export writing task confusing success and failure (Thanks, @filmor)
* #10 Fixed a bug where loading a NXPorts enabled project in designtime build mode (e.g. Visual Studio) would be blocked by a failing AssemblyExportWriterTask
* #14 NXPorts now removes the obsolete `NXPorts.Attributes.dll` from the build output
* Significantly increased logging output to support diagnosing issues when using NXPorts.
* Added a warning informing the user when NXPorts did not find any annotated method.
* #12 NXPorts now throws an error whenever it detects an incompatible project configuration in regards to target platform.
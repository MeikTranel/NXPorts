<!-- markdownlint-disable MD041 -->
* **[BUGFIX]** #65 Fixed a bug where a subsequent build would result in a failed removal of the NXPorts.Attributes.dll (which is obsolete once the dll has been reweaved)
* **[FEATURE]** #76 Added diagnostics to detect and warn/error on duplicate export definitions
  * Due to being technically legal exports that differ only in capitalization now throw a warning.
  * If you want to guard against those cases as well, set `NXPortsAllowCaseSensitiveDuplicates` to `false`.

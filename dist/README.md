# Preview distributables

`LoginVSI.MultiMonitor.dll` is the intentional dependency-free `netstandard2.0` Preview distributable produced by `build.ps1`. Stage the reviewed file at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` for DLL-backed or integrated workload experiments.

The DLL has passed the repository's local build and pure-logic tests. It has not yet been validated for Login Enterprise Script Editor loading, Login Enterprise runtime execution, interactive multi-display placement, a full scenario, or VDI.

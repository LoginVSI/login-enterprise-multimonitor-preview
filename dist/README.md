# Preview distributables

`LoginVSI.MultiMonitor.dll` is the intentional dependency-free `netstandard2.0` Preview distributable produced by `build.ps1`. For the unsupported Preview workflow, upload the reviewed file to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance, then run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` to stage it at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` on the target.

The DLL has passed the repository's local build and pure-logic tests. Login Enterprise 6.8.6 Script Editor/Standalone Engine loaded and invoked it for actual two-display placement on August 18, 2026. A later Desktop Connector Application Test proved appliance ScriptContent delivery and simple regression-harness platform execution. The canonical Prepare -> Open/Place -> Close source is generated/not runtime-proven; representative scenario behavior, broader compatibility, and VDI remain unvalidated.

# Preview distributables

`LoginVSI.MultiMonitor.dll` is the intentional dependency-free `netstandard2.0` Preview distributable produced by `build.ps1`. For the unsupported Preview workflow, upload the reviewed file to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance, then run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` to stage it at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` on the target.

The DLL has passed the repository's local build and pure-logic tests. It has not yet been validated for Login Enterprise Script Editor loading, Login Enterprise runtime execution, interactive multi-display placement, a full scenario, or VDI.

# Preview distributables

`LoginVSI.MultiMonitor.dll` is the intentional dependency-free `netstandard2.0` Preview distributable produced by `build.ps1`. For the unsupported Preview workflow, upload the reviewed file to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance, then run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` to stage it at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` on the target.

The DLL has passed repository build/pure-logic checks and Login Enterprise 6.8.6 runtime testing for actual two-display placement, appliance delivery, state continuity, and the canonical Prepare -> Open/Place -> Close flow. Office/Knowledge Worker adaptations, broader compatibility, and VDI remain partner-lab pending or unvalidated as documented.

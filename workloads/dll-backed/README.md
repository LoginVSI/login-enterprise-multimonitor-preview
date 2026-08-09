# DLL-backed sequential proof

These workloads reproduce the script-only Notepad/Paint then Command Prompt/Edge sequence through reflection, without a compile-time reference to the helper assembly:

- `01-Initialize-Notepad-Paint.cs`
- `02-Continue-Cmd-Edge.cs`

Stage `dist/LoginVSI.MultiMonitor.dll` at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` before execution. The workloads use documented `FileExists`, then ordinary compatible `Assembly.LoadFrom`, and pass `IWindow.NativeWindowHandle` to `MultiMonitorPlacer`.

No supported automatic DLL distribution contract was established by the supplied documentation, so none is invented here. Script Editor must validate compilation and loading for each file; a real sequential scenario must validate continuity. Status: generated/not validated in Login Enterprise.

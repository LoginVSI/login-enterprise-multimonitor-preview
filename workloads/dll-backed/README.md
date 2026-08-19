# DLL-backed preparation and sequential proof

These files provide the unsupported Preview staging step and reproduce the script-only sequence through reflection without a compile-time reference to the helper assembly:

- `00-Prepare-MultiMonitor.cs`
- `01-Initialize-Notepad-Paint.cs`
- `02-Continue-Edge.cs`

For Script Editor/Standalone Engine testing, place the DLL in that engine's local ScriptContent directory. For the future platform test, upload `dist/LoginVSI.MultiMonitor.dll` to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance. The prepare workload copies `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`; it stages no application and consumes no monitor destination.

`ForceRefreshMultiMonitorDll` defaults to `false`. A missing local DLL is always staged. Existing plus `false` retains it without copying. Existing plus `true` removes it, verifies removal, copies the configured ScriptContent file, and verifies the destination. Set `true` only for an intentional update and return it to `false` afterward where appropriate. Replacing the ScriptContent file alone does not replace existing target-local copies while the toggle is false.

Consumers use documented `FileExists`, then ordinary compatible `Assembly.LoadFrom`, and pass the intended base `IWindow.NativeWindowHandle` to `MultiMonitorPlacer`. The initializer uses documented `START`/`MainWindow` for Notepad because raw `ShellExecute` tracked a short-lived PID in actual testing; it retains the existing Paint launch/`FindWindow` flow that placed successfully. The continuation uses `START`/`MainWindow` for the simple Edge proof for the same durable-window reason. CMD was removed because Windows Terminal hosted the visible UI on the tested configuration and Login Enterprise could not find the requested standalone `cmd` top-level window. A missing local DLL aborts with instructions to run the prepare workload. Consumers never copy or force-refresh it.

This is an unsupported Preview mechanism, not a formal distribution/update contract. Login Enterprise 6.8.6 Script Editor/Standalone Engine runtime-proved prepare compilation, initial staging, forced refresh, DLL loading, durable Notepad and Edge windows, two-monitor Notepad/Paint/Edge round robin, state continuation across separate standalone executions, and missing-state recovery on August 18, 2026. The default-retain branch, appliance delivery, actual platform sequence, and integrated secondary-window non-consumption still require validation.

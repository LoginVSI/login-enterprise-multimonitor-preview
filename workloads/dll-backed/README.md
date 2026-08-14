# DLL-backed preparation and sequential proof

These files provide the unsupported Preview staging step and reproduce the script-only sequence through reflection without a compile-time reference to the helper assembly:

- `00-Prepare-MultiMonitor.cs`
- `01-Initialize-Notepad-Paint.cs`
- `02-Continue-Cmd-Edge.cs`

Upload `dist/LoginVSI.MultiMonitor.dll` to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance. The prepare workload copies `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`; it stages no application and consumes no monitor destination.

`ForceRefreshMultiMonitorDll` defaults to `false`. A missing local DLL is always staged. Existing plus `false` retains it without copying. Existing plus `true` removes it, verifies removal, copies the appliance file, and verifies the destination. Set `true` only for an intentional update and return it to `false` afterward where appropriate. Replacing the appliance file alone does not replace existing target-local copies while the toggle is false.

Consumers use documented `FileExists`, then ordinary compatible `Assembly.LoadFrom`, and pass the intended base `IWindow.NativeWindowHandle` to `MultiMonitorPlacer`. Their process-only Notepad/Paint selectors, titled Command Prompt selector, and newly observed top-level Edge selector still require runtime proof of splash avoidance and HWND durability; no selector is guessed without safer application evidence. A missing local DLL aborts with instructions to run the prepare workload. Consumers never copy or force-refresh it.

This is an unsupported Preview mechanism, not a formal distribution/update contract. Script Editor must validate the prepare workload's missing/retain/refresh cases and compilation/loading for every file; a real sequential scenario must validate continuity and secondary-window non-consumption. Status: generated/not validated in Login Enterprise.

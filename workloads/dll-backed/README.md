# DLL-backed Preview workloads

## Canonical current flow

1. `00-Prepare-MultiMonitor.cs` stages or retains `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` without consuming a destination.
2. `01-Open-Place-Applications.cs` verifies and reflection-loads that DLL, resets state once for a fresh demonstration run, resolves one durable/base window each for Notepad, Paint, and Microsoft Edge, and calls `PlaceNext` exactly once per application.
3. `02-Close-Applications.cs` closes only a sole matching base window for each application. It skips ambiguous matches and does not load the DLL, allocate, reset, or touch `%TEMP%\LoginPI\MultiMonitor\state.txt`.

This canonical flow passed in the recorded Login Enterprise 6.8.6 Desktop Connector Application Test, including serial Prepare -> Open/Place -> Close execution, two-monitor placement/state, scenario-controlled handoff, and bounded cleanup. Do not infer broader runtime compatibility from that named result.

For its next Application Test, configure `Leave application running` as follows:

- Prepare: off/not relevant.
- Open/Place: on.
- Close: off.

Continuous Test and Load Test also provide per-workload `Run once`. Preserve the scenario's intended one-time preparation/open/cleanup behavior instead of blindly copying Application Test settings.

One workload has one associated `TARGET`. Edge uses the runtime-proven `START(processName: "msedge")`/`MainWindow` path. Modern Notepad's raw `ShellExecute` PID was not durable, so this combined demonstration uses the compatible `.NET Process.Start` pattern already preserved in repository evidence, then independently requires one durable Notepad window. Paint retains its proven `ShellExecute` plus `FindWindows(className: "Win32 Window:MSPaintApp", processName: "mspaint")` path. All three applications must be absent at preflight so later cleanup can be bounded. This ownership and cross-workload survival model passed in the recorded canonical test; other environments still require validation.

## Proven regression harness

The previous runtime-proven files are retained unchanged under `regression/`:

- `regression/01-Initialize-Notepad-Paint.cs`
- `regression/02-Continue-Edge.cs`

Together with Prepare, these are the workloads proven in Login Enterprise 6.8.6 Script Editor/Standalone Engine and a real Desktop Connector Application Test: appliance delivery, serial execution, state continuity, `Notepad -> 0`, `Paint -> 1`, `Edge -> 0`, and final state `MonitorCount=2` / `LastUsedIndex=0`. They are evidence/regression harnesses, not the canonical lifecycle flow.

For Script Editor/Standalone Engine testing, place the DLL in that engine's local ScriptContent directory. For platform execution, upload `dist/LoginVSI.MultiMonitor.dll` to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`. `ForceRefreshMultiMonitorDll` defaults to `false`: missing stages, existing plus false retains, and existing plus true removes/copies/verifies. Consumers only load the staged local DLL.

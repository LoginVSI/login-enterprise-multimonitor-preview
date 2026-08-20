# Adapt your own workload

## 1. Classify the lifecycle

Identify whether the file prepares, starts/opens, runs an already-open application, closes it, or contains the full lifecycle. Preserve its `TARGET`, application actions, URLs/content, timers, EUX measurements, cadence, and scenario intent.

For Outlook, first identify the flavor. Classic Outlook and New Outlook use different executable, durable-window, control, navigation, and lifecycle models. Do not silently substitute one for the other: changing only `TARGET` or the executable is insufficient, and launch success does not prove interaction compatibility. Treat a requested Classic-to-New conversion as a substantive workload adaptation. Preserve timers, content, and measurements only around evidence-supported equivalent New Outlook interactions; otherwise surface the gap and require runtime validation.

## 2. Identify the durable/base window

Trace launch handoff, process reuse, title, top-level class, process name, and replacement-window behavior. Prefer documented `START` when the workload owns startup and can identify the durable main UI. Otherwise use documented `FindWindow` or `FindWindows` with compiler-proven named arguments `className` and `processName`.

Do not allocate splash, first-run/setup, open/save, compose/read/reminder, popup, child, or temporary launcher windows. Detect ambiguous pre-existing windows and fail instead of moving an unrelated user window.

## 3. Load the staged Preview DLL

Run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` first. A consumer verifies `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`, loads it with `Assembly.LoadFrom`, resolves `LoginVSI.MultiMonitor.MultiMonitorPlacer`, and uses `%TEMP%\LoginPI\MultiMonitor\state.txt`. Copy the small reflection pattern from a current workload; do not copy inline Win32 placement logic.

## 4. Select the placement call

```text
Start/open: durable window resolved -> PlaceNext exactly once
Run:        durable window reacquired -> PlaceLastUsed or PlaceOnMonitor; never PlaceNext
Close:      explicit bounded cleanup -> no placement/state call
Prepare:    staging only -> no allocation
```

Call placement after the durable window is ready and outside application-response/EUX timers wherever practical. Inspect `Success`, log the structured result, and abort clearly on failure. `PlaceNext` advances only after verification; maintenance calls never advance. Missing or malformed state may be repaired, but that repair is not an allocation.

Never persist HWND or native monitor handles across workload files.

## 5. Preserve scenario lifecycle

Use `Leave application running` intentionally when a Start/Open workload hands an application to Run or Close. Preserve `Run once` intent in Continuous and Load Tests. Do not depend on a process that happens to linger.

## 6. Review and validate

Record the source-to-adaptation mapping and every meaningful delta. Run `.\scripts\Test-Repository.ps1`. Then compile and execute disposable copies in Script Editor and test the actual scenario through the intended Connector/session. Static validation is not runtime proof; follow [testing](testing.md) and [troubleshooting](troubleshooting.md).

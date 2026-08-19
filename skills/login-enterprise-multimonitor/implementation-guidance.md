# Implementation guidance

Status: **DRAFT / PARTIALLY LOGIN ENTERPRISE-VALIDATED**. This describes the current Preview implementation, not a stable product contract. See `validation-guidance.md` for the proven 6.8.6 scope and remaining platform work.

## API priority and boundary

Use documented Login Enterprise operations for launch, `FindWindow`/`FindWindows`, waits, logs, timers, file checks, and `IWindow` operations. Use ordinary compatible C# for reflection and state logic. Use Win32 only for display discovery, HWND placement, and verification not exposed by the supplied scripting API.

The workload owns launch, durable/base `IWindow` discovery, application behavior, insertion point, and measurement boundaries. `LoginVSI.MultiMonitor.dll` owns monitor/state/placement mechanics and has no LoginPI.Engine reference.

## Durable/base-window contract and readiness

Only the durable/base UI consumes a round-robin destination. Never call allocating placement for a splash, first-run/setup dialog, open/save dialog, Outlook compose/read/reminder window, popup, child/secondary interaction window, or temporary launcher. Leave secondary placement to the application and Windows unless a separate evidence-backed requirement says otherwise.

Before adaptation, document process lifecycle, splash versus main UI, stable title/class/process criteria, whether the selected HWND survives for the workload/session, excluded dialogs/children, and the placement insertion point. Use this readiness hierarchy:

1. When the workload owns application startup and needs a durable main window, prefer documented `START` with sufficiently specific main title/class/process matching. Actual Notepad and Edge tests showed that the raw `ShellExecute` PID may exit while visible UI exists in another or reused process.
2. Otherwise resolve the intended durable `IWindow` through documented `FindWindow`/`FindWindows` behavior.
3. Supply `NativeWindowHandle` only after identification.
4. If empirical evidence shows that the correct window needs settling, expose `int PrePlacementReadinessDelayMilliseconds = 0;` at workload level and wait after identification only when positive.
5. Use blind fixed startup sleeps only as a fallback. Keep `ShellExecute` when the process/window lifecycle is known and explicitly handled.

Use `FindWindows(className: ..., processName: ...)`. Login Enterprise 6.8.6 compiler evidence rejects lowercase `classname` and `processname`; the earlier documentation ambiguity is resolved.

The optional application readiness delay is not the DLL's placement stabilization delay. Readiness happens after durable HWND identification but before invoking placement. Stabilization is the existing helper delay during restore/move/maximize/verification of that already-correct HWND. Do not add a mandatory global wait.

## Current state and selection contract

- Path: `%TEMP%\LoginPI\MultiMonitor\state.txt`.
- Schema: `MonitorCount=<integer>` and `LastUsedIndex=<integer>`.
- Initial/reset index: `-1`.
- Allocation: `(lastUsedIndex + 1) % monitorCount`.
- Recovery: reset for missing, malformed, out-of-range, or monitor-count-changed state.
- Commit: write atomically only after verified successful placement.
- Concurrency: serialize allocation with a sibling lock file.

Rediscover monitors for every call. Put the explicit primary first, then order remaining monitors by signed left/top coordinates and stable tie-breakers. Signed bounds preserve displays left of or above the primary. Never persist native handles.

## Current DLL contract

The dependency-free `netstandard2.0` assembly exposes reflection-friendly static methods on `LoginVSI.MultiMonitor.MultiMonitorPlacer`:

- `ResetState(string stateFilePath)` initializes state deliberately.
- `PlaceNext(IntPtr, string, string, bool, int)` allocates, places, verifies, then advances.
- `PlaceLastUsed(IntPtr, string, string, bool, int)` reapplies the persisted target without advancing.
- `PlaceOnMonitor(IntPtr, string, string, int, bool, int)` reapplies a specified target without advancing.

`PlacementResult` exposes success, application, monitor count, initial/target/verified indices, state advancement, elapsed milliseconds, Win32 error, and message. Callers must inspect `Success`; reflection invocation success alone does not prove placement.

## Placement behavior

Restore the HWND, allow stabilization, call `SetWindowPos` using full monitor bounds, maximize when requested, stabilize again, and verify with `MonitorFromWindow`. The operation has measurable overhead. A result advances state only after target verification.

## Unsupported Preview loading and staging

For Script Editor/Standalone Engine development, put `LoginVSI.MultiMonitor.dll` in that engine's local ScriptContent directory. Do not hard-code an installation-specific developer path. For a real Login Enterprise platform/Desktop Connector test, upload it to appliance `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`. The run-once `workloads/dll-backed/00-Prepare-MultiMonitor.cs` uses `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` with documented `CopyFile` to stage `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`.

Its contract is:

- missing local DLL: create the directory, copy from the configured ScriptContent source, verify destination, fail clearly if absent;
- existing local DLL and default `ForceRefreshMultiMonitorDll = false`: retain it and log without copying;
- existing local DLL and `true`: use documented `RemoveFile`, verify removal, copy the appliance file, verify destination, and log refresh.

Updating the appliance file alone does not update targets that retain a local copy. Return the toggle to `false` after deliberate refresh where appropriate. Consumer workloads use `FileExists`, abort usefully when missing, then use `Assembly.LoadFrom` plus reflection. They never force-refresh or routinely download. This is an unsupported Preview mechanism, not a formal product distribution/update API. Re-check supplied API evidence before altering any staging behavior; never invent an LE file API.

Initial staging and forced remove/copy refresh are runtime-proven through the local 6.8.6 engine ScriptContent surface. Appliance delivery remains unproven.

## Application patterns

- **Office:** identify the durable document/main `IWindow` after open measurement stops; allocate there. Exclude first-run, file, confirmation, message, compose, reminder, and slideshow windows. Reassert the base window after later minimize/maximize behavior when needed.
- **Simple Edge proof:** use `START(processName: "msedge", timeout: 30)` and `MainWindow`; this supplied a durable window in the tested DLL-backed harness.
- **Integrated Edge/browser:** preserve its application-specific new-window discovery and account for multiprocess/existing-instance ambiguity. Start allocates after original initialization; Run reuses and repeatedly reasserts the saved target after focus/maximize actions. Do not generalize the simplified harness into the Knowledge Worker flow without runtime evidence.
- **CMD:** do not use it as a generic proof target on configurations where Windows Terminal owns the visible terminal UI.
- **Persistent Start/Run:** state continuity and the long-lived window must be tested across independent workload files in an actual scenario.

## Failure handling

Return and log structured failure information for invalid HWNDs, monitor discovery, state locking, Win32 movement, and verification. Examples abort on placement failure to avoid silent false success. Product continuation policy remains undecided.

## Future alternative

A background session window router could respond to replacement windows, but it is neither implemented nor an approved requirement. Evaluate only with lifecycle, security, ownership, timing, and deployment evidence.

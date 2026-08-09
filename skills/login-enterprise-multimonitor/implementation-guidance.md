# Implementation guidance

Status: **DRAFT / NOT LOGIN ENTERPRISE-VALIDATED**. This describes the current Preview implementation, not a stable product contract.

## API priority and boundary

Use documented Login Enterprise operations for launch, `FindWindow`/`FindWindows`, waits, logs, timers, file checks, and `IWindow` operations. Use ordinary compatible C# for reflection and state logic. Use Win32 only for display discovery, HWND placement, and verification not exposed by the supplied scripting API.

The workload owns launch, current `IWindow` discovery, application behavior, insertion point, and measurement boundaries. `LoginVSI.MultiMonitor.dll` owns monitor/state/placement mechanics and has no LoginPI.Engine reference.

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

## Loading and staging

Current DLL-backed workloads expect `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` and use `Assembly.LoadFrom` plus reflection. The supplied documentation did not establish a supported automatic custom-DLL distribution API, so stage the DLL explicitly and validate that process. Do not invent delivery functionality.

## Application patterns

- **Office:** identify the durable document/main `IWindow` after open measurement stops; allocate there. Reassert after later minimize/maximize behavior when needed.
- **Edge/browser:** snapshot existing Edge windows before launch, prefer a newly observed window, and account for multiprocess/existing-instance ambiguity. Start allocates after original initialization; Run reuses and repeatedly reasserts the saved target after focus/maximize actions.
- **Persistent Start/Run:** state continuity and the long-lived window must be tested across independent workload files in an actual scenario.

## Failure handling

Return and log structured failure information for invalid HWNDs, monitor discovery, state locking, Win32 movement, and verification. Examples abort on placement failure to avoid silent false success. Product continuation policy remains undecided.

## Future alternative

A background session window router could respond to replacement windows, but it is neither implemented nor an approved requirement. Evaluate only with lifecycle, security, ownership, timing, and deployment evidence.

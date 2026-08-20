# Architecture

Status: generic library and canonical Prepare -> Open/Place -> Close behavior are runtime-proven in the recorded Login Enterprise 6.8.6 two-monitor environment. Office Preview Word/Excel/PowerPoint, corrected Edge, and New Outlook launch/find/place passed on one local 6.8.6 machine; Classic Outlook and New Outlook interaction automation remain runtime-pending. Knowledge Worker adaptations are generated/build-tested/static-validated; partner-lab validation is pending.

## Problem and universal intent

Compatible Login Enterprise workloads commonly interact primarily with the primary display. The Preview provides deterministic, reusable distribution across active displays without coupling the generic mechanism to Office, Edge, or a particular workload set.

## API priority and responsibility boundary

Workloads use documented Login Enterprise APIs first: launch through `START` or `ShellExecute`, find the correct `IWindow` through `FindWindow`/`FindWindows`, use `Restore`/`Maximize` where script-contained, log through `Log`, and bridge through `IWindow.NativeWindowHandle`. Ordinary .NET provides reflection, state, timing, collections, and files. Win32 is limited to display enumeration, HWND validation/movement, and monitor verification.

Application-specific code owns launch, correct durable/base-window identification, sequencing, business actions, timers, and the placement insertion point. `LoginVSI.MultiMonitor` owns discovery, ordering, state, selection, native restore/move/maximize for DLL calls, verification, locking, and structured results. It never changes Windows primary-monitor configuration.

## Durable-window allocation contract

One application consumes one round-robin destination only when its durable/base UI has been identified. A splash screen, first-run/setup dialog, open/save dialog, Outlook compose/read/reminder window, popup, child interaction window, or temporary launcher never allocates. Secondary windows normally remain under application and Windows placement control. Maintenance placement of the already allocated base window may reassert its existing target but does not advance state.

Window readiness is application-specific and precedes the helper call:

1. When the workload owns startup and needs a durable main application window, prefer documented `START` matching that identifies and waits for the real main UI by appropriate title, class, and process. A PID initially returned by raw `ShellExecute` does not prove ownership of the durable visible UI; modern applications may hand off or reuse another process.
2. Otherwise explicitly resolve the intended durable `IWindow` with documented `FindWindow`/`FindWindows` behavior.
3. Pass `NativeWindowHandle` only after that durable window is known.
4. If empirical application evidence requires settling after identification, use a workload-level setting such as `int PrePlacementReadinessDelayMilliseconds = 0;` and wait only when it is greater than zero.
5. Treat a blind fixed startup sleep as a fallback, not the primary identification mechanism. Retain `ShellExecute` where application-specific evidence establishes and explicitly handles its process/window lifecycle.

Application readiness delay and placement stabilization delay are separate. The former is an optional, default-zero workload wait after the correct HWND is known. The latter is the existing helper argument used around restore, move, maximize, and verification of that same HWND. There is no mandatory global readiness wait.

## Managed library

The single dependency-free assembly targets `netstandard2.0` with C# 7.3. This is a conservative portability choice for mature .NET Framework-era consumers and modern .NET, without a LoginPI.Engine reference or third-party package. Login Enterprise 6.8.6 Script Editor/Standalone Engine successfully loaded and invoked the staged assembly on August 18, 2026; broader release/platform compatibility is not inferred from that result or local SDK success.

The reflection-friendly static API is:

- `ResetState(string stateFilePath)`
- `PlaceNext(IntPtr windowHandle, string applicationName, string stateFilePath, bool maximize, int stabilizationDelayMilliseconds)`
- `PlaceOnMonitor(..., int targetMonitorIndex, ...)`
- `PlaceLastUsed(...)`

`PlaceNext` advances round-robin state after verified success. `PlaceOnMonitor` reasserts a known target without advancing. `PlaceLastUsed` supports persistent Start/Run pairs by reading the last verified target without consuming another destination.

`PlacementResult` reports success, application, monitor count, initial/target/verified indices, elapsed milliseconds, state advancement, Win32 error code, and a message.

## State

State remains compatible with the proven POC:

```text
%TEMP%\LoginPI\MultiMonitor\state.txt
MonitorCount=<integer>
LastUsedIndex=<integer>
```

Initialization uses `LastUsedIndex=-1`; next selection is `(lastUsedIndex + 1) % monitorCount`. Missing, malformed, out-of-range, or monitor-count-changed state resets to the current count and `-1`. Parse corruption is distinct from file I/O/access failures: operational read failures propagate to the structured placement failure path rather than being mislabeled and overwritten as corruption. The state update uses a same-directory temporary file and replacement. A short-lived exclusive `.lock` file serializes readers/writers around selection and placement. HWND and monitor handles are rediscovered and never persisted. Maintenance calls never advance allocation; repair can initialize malformed/missing state, after which `PlaceLastUsed` fails because no previous target exists.

## Monitor discovery and ordering

Each allocation or maintenance placement calls `EnumDisplayMonitors` and `GetMonitorInfo`. The primary flag is explicit. Ordering is:

1. Primary monitor.
2. Remaining monitors by signed `Left`, then signed `Top`, bounds, and handle as deterministic tie-breakers for that discovery.

Signed bounds preserve displays left of or above the primary. Synthetic tests cover primary-first order and negative coordinates.

## Placement flow

1. Validate the current HWND.
2. Acquire state access.
3. Rediscover and order monitors.
4. load/repair state and choose a target.
5. Restore the current window.
6. Wait for stabilization.
7. call `SetWindowPos` with the target's full bounds.
8. Wait, optionally maximize, and wait again.
9. Verify with `MonitorFromWindow`.
10. Advance state only for a verified `PlaceNext`.
11. Return timing and result information.

The library does not force foreground focus. Workloads retain focus ownership.

## Script-only and DLL-backed paths

Script-only workloads embed the same core state and placement behavior while using `IWindow.Restore`/`Maximize`. They isolate Script Editor behavior before assembly loading.

DLL-backed, Office Preview, and Knowledge Worker workloads use `FileExists`, ordinary `Assembly.LoadFrom`, and reflection. The unsupported Preview deployment uses the supplied ScriptContent file pattern, not a new distribution API. Script Editor/Standalone Engine testing resolves ScriptContent from the engine's local ScriptContent directory; the real platform path is a separate delivery surface:

1. Upload `LoginVSI.MultiMonitor.dll` to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the appliance.
2. Run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` once. It copies `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`.
3. Consumers verify that local path and load it; they never force-refresh or routinely copy it.

The prepare workload always stages a missing local DLL. With the default `ForceRefreshMultiMonitorDll = false`, it retains an existing local DLL. With the toggle set to `true`, it removes the existing file with documented `RemoveFile`, confirms removal, copies from ScriptContent, and verifies the new local file. Updating only the appliance file therefore does not update an existing target-local copy while the toggle remains false.

Initial staging and forced `RemoveFile` -> `CopyFile` refresh were runtime-proven with a locally staged ScriptContent DLL in Login Enterprise 6.8.6 Script Editor/Standalone Engine. Appliance delivery from `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`, including missing/default-retain/forced-refresh Prepare paths, is proven in the tested Desktop Connector Application Test.

## Canonical generic Preview flow

`00-Prepare-MultiMonitor.cs` retains the proven staging responsibility and does not initialize placement state. `01-Open-Place-Applications.cs` owns fresh-demonstration initialization through an explicit `ResetStateForFreshPreviewRun` toggle, keeping deployment separate from continuity policy. It preflights for zero matching Notepad, Paint, and Edge base windows, launches them, requires exactly one durable match per application, then calls `PlaceNext` once per window. `02-Close-Applications.cs` requests normal closure only for a sole matching base window; it skips ambiguity and has no placement DLL or state dependency.

Application Test leaves Open/Place running into Close while Prepare and Close remain off/not relevant. No HWND or monitor handle crosses files. This complete generic lifecycle is runtime-proven in the recorded Desktop Connector test. Continuous Test and Load Test adaptations must still choose `Run once` deliberately.

## Knowledge Worker sequencing and measurement

The small Office Preview preflights Word, Excel, PowerPoint, and Classic Outlook by stable process/class identity so it fails rather than moving a pre-existing base window. Classic Outlook avoids an English folder-title dependency. New Outlook is a separate `TARGET:olk` example using the runtime-proven `START()`/`MainWindow` path; it proves launch/find/place only, not Classic Outlook interaction compatibility. Edge requires zero existing base windows, uses the runtime-proven generic `START`/`MainWindow` launch path, independently requires one durable Edge window, and compares HWNDs before allocation. The corrected Edge implementation has one-machine runtime evidence.

Representative Office document workloads are placed after their existing open-document timers stop. The selected workbook, presentation, or document window is the base window; open/save and other dialogs do not allocate. The representative Classic Outlook adaptation allocates only its original Inbox `MainWindow`; open-message, compose, reminder, and first-run windows do not. Later base-window minimize/maximize actions reassert the same target without advancing state. Preparation and close workloads do not consume targets.

Edge Start snapshots existing top-level Edge HWNDs, identifies a newly observed `Chrome_WidgetWin_1` Edge window, ends `Browser_Start`, preserves its existing initialization wait, then allocates that browser base window. Edge Run resolves the expected persistent browser window, uses the last verified target from Start, and reasserts it after repeated maximize/focus operations. This adds cadence overhead but avoids treating a Start/Run pair as two applications. Same-HWND continuity and ambiguity with multiple matching Edge windows remain runtime validation gates.

The complete adaptations live under `workloads/knowledge-worker-multimonitor/`; the authoritative scenario order and settings remain in `reference/test-scenario/workload-sequence.txt`. The mapping manifest and static contracts enforce minimal deltas, but partner-lab runtime validation remains pending.

## Alternatives and open questions

Copying helper source into every workload remains useful for isolation but creates drift. The DLL centralizes behavior but adds staging and runtime compatibility requirements. A background session router remains a possible future alternative, not an implemented requirement or commitment.

Open evidence areas include Office/Knowledge Worker durable-window identity and replacement, DPI/scaling, concurrency under representative scenario load, display changes during placement, broader interactive/VDI behavior, and acceptable timing overhead. Appliance ScriptContent delivery, generic serial orchestration, state continuity, and canonical cleanup are proven for the tested 6.8.6 environment.

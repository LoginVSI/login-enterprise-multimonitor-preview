# Architecture

Status: implemented and locally build-tested; Login Enterprise and interactive placement validation remain pending.

## Problem and universal intent

Compatible Login Enterprise workloads commonly interact primarily with the primary display. The Preview provides deterministic, reusable distribution across active displays without coupling the generic mechanism to Office, Edge, or a particular workload set.

## API priority and responsibility boundary

Workloads use documented Login Enterprise APIs first: launch through `START` or `ShellExecute`, find the correct `IWindow` through `FindWindow`/`FindWindows`, use `Restore`/`Maximize` where script-contained, log through `Log`, and bridge through `IWindow.NativeWindowHandle`. Ordinary .NET provides reflection, state, timing, collections, and files. Win32 is limited to display enumeration, HWND validation/movement, and monitor verification.

Application-specific code owns launch, correct window identification, sequencing, business actions, timers, and the placement insertion point. `LoginVSI.MultiMonitor` owns discovery, ordering, state, selection, native restore/move/maximize for DLL calls, verification, locking, and structured results. It never changes Windows primary-monitor configuration.

## Managed library

The single dependency-free assembly targets `netstandard2.0` with C# 7.3. This is a conservative portability choice for mature .NET Framework-era consumers and modern .NET, without a LoginPI.Engine reference or third-party package. Actual Login Enterprise loader compatibility is not inferred from local SDK success.

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

Initialization uses `LastUsedIndex=-1`; next selection is `(lastUsedIndex + 1) % monitorCount`. Missing, malformed, out-of-range, or monitor-count-changed state resets to the current count and `-1`. The state update uses a same-directory temporary file and replacement. A short-lived exclusive `.lock` file serializes readers/writers around selection and placement. HWND and monitor handles are rediscovered and never persisted.

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

DLL-backed and integrated workloads use `FileExists`, ordinary `Assembly.LoadFrom`, and reflection. The documentation does not establish a dedicated DLL distribution API, so the DLL must be staged at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` by an environment-appropriate method.

## Integrated sequencing and measurement

Office document windows are placed after their existing open-document timers stop. Later minimize/maximize actions reassert the same target without advancing state. Preparation and close workloads do not consume targets.

Edge Start identifies a newly observed top-level Edge HWND, ends `Browser_Start`, preserves its initialization wait, then allocates. Edge Run uses the last verified target from Start and reasserts it after repeated maximize/focus operations. This adds cadence overhead but avoids treating a Start/Run pair as two applications.

The authoritative scenario order and settings remain in `reference/test-scenario/workload-sequence.txt`.

## Alternatives and open questions

Copying helper source into every workload remains useful for isolation but creates drift. The DLL centralizes behavior but adds staging and runtime compatibility requirements. A background session router remains a possible future alternative, not an implemented requirement or commitment.

Open evidence areas include Script Editor language/runtime compatibility, DLL loading, application window replacement, DPI/scaling, concurrency under real scenario load, display changes during placement, interactive/VDI behavior, and acceptable timing overhead.

# Testing and validation

## Evidence vocabulary

- **Planned:** intended but not executed.
- **Generated / not validated:** source exists without the required runtime evidence.
- **Locally build-tested:** compiled and exercised by the dependency-free local test harness.
- **Proven in Script Editor:** compiled and executed as an individual workload in Script Editor/standalone runner.
- **Proven in full Login Enterprise test:** exercised across independent files in an actual platform-managed test.
- **Proven in VDI:** exercised in a documented VDI environment.

## Current evidence

`build.ps1` currently builds the `netstandard2.0` assembly with zero warnings/errors and runs 17 tests covering:

- next-index sequences for one, two, three, and four displays;
- primary-first synthetic ordering;
- signed negative coordinates;
- valid, invalid, and missing state plus on-disk repair;
- monitor-count-change reset and repair;
- state serialization and round trip;
- same-path replacement writes and temporary-file cleanup;
- a safe structured failure for a zero HWND.

This is local pure-logic/failure-path evidence only. Separately, actual Login Enterprise 6.8.6 Script Editor/Standalone Engine execution on August 18, 2026 proved:

- `00-Prepare-MultiMonitor.cs` compiled and completed initial local staging plus forced `RemoveFile` -> `CopyFile` refresh from the engine's local ScriptContent directory;
- the staged DLL loaded and accepted `IWindow.NativeWindowHandle`;
- the compiler accepts `FindWindows(className: ..., processName: ...)` and rejects lowercase `classname`/`processname`;
- `START(processName: "notepad", timeout: 30)` supplied the durable `Untitled - Notepad - Notepad` main window after raw `ShellExecute` process tracking failed;
- on two physical monitors, reset created `LastUsedIndex=-1`, Notepad targeted/verified index 0, Paint targeted/verified index 1, and state ended at index 1;
- a later independent `START`/`MainWindow` Edge workload continued from index 1, targeted/verified index 0, and persisted index 0;
- deleting `state.txt` before the Edge placement caused automatic valid-state recreation and successful placement.

Notepad placement reported approximately 1.1 seconds elapsed in that run. Treat this as one observed result, not a performance guarantee. Corrupt-state and monitor-count-change recovery remain runtime validation items despite local unit coverage.

The real Login Enterprise 6.8.6 Desktop Connector Application Test then proved in a Console / NoRemote session:

- appliance delivery of `LoginVSI.MultiMonitor.dll` from ScriptContent;
- missing-local-DLL initial staging, existing/default-retain, and existing/forced-refresh Prepare paths;
- serial platform execution of `00-Prepare-MultiMonitor`, `01-Initialize-Notepad-Paint`, and `02-Continue-Edge` as three independent workloads;
- platform cross-workload state persistence;
- actual two-monitor placement `Notepad -> 0`, `Paint -> 1`, `Edge -> 0`;
- final state `MonitorCount=2` and `LastUsedIndex=0`;
- successful completion of all three AppExecutions.

## Script Editor versus actual scenario

Script Editor/Standalone Engine validates individual workload compilation, launch, correct durable `IWindow`, local ScriptContent staging, DLL loading, placement, logs, and failure handling. It runs one workload at a time. Separate standalone runs can demonstrate that a state file survives and is consumed later, but they do not prove that the Login Enterprise platform serially orchestrates independent workload files.

Repository files remain source of truth. Copy a workload to a disposable location before opening or running it in Script Editor. The editor may rewrite the working representation or line endings; deliberately apply validated changes back to repository source rather than testing directly against working-tree copies where avoidable.

The simple platform-orchestrated three-workload proof and cross-workload state are now proven. The final application lifecycle, persistent Start/Run behavior, complete representative ordering, and end-to-end Knowledge Worker behavior still require the future final flow and later scenario validation.

## Lifecycle and scenario settings

- Application Test provides per-workload `Leave application running`; its default is off.
- Continuous Test and Load Test provide per-workload `Leave application running` and `Run once`.
- Persistence between workloads must be intentional and scenario-controlled. A process that happens to linger is not the persistence contract.
- When applications opened and placed by a future Open/Place workload must survive into a later Close workload, enable `Leave application running` for Open/Place. The Close workload must explicitly close those applications.
- Preserve intended `Run once` semantics when adapting the final flow to Continuous Test or Load Test.

With `Leave application running` off in the proven Application Test, Login Enterprise stopped Notepad and Edge, both launched through `START`, at their workload boundaries. Paint, launched through `ShellExecute`, lingered. Treat Paint as a launch-lifecycle observation only, not as the model for cross-workload persistence.

## Non-blocking environment observations

The successful Desktop Connector run also emitted environment-specific messages that did not block compilation or execution:

- ICA, Blast, and PCoIP probe warnings appeared before the session resolved as NoRemote.
- Latency was not reported for this local Desktop Connector session.
- A `forceKillOnExit` warning explained that the schedule action controls cleanup.
- On ARM, a `Microsoft.DiaSymReader.Native.amd64.dll` load message appeared; compilation and execution succeeded afterward.

Retain these as diagnostic context. Do not treat them as placement failures or generalize them beyond the tested environment without further evidence.

## Manual test order

1. Run `build.ps1` and retain the console output.
2. Use only the compiler-proven `FindWindows` named-argument casing: `className` and `processName`.
3. For Script Editor/Standalone Engine development, place the DLL in that engine installation's local ScriptContent directory. Do not encode a particular developer installation path as a requirement.
4. Recheck the proven Prepare paths only when environment or implementation changes: missing local DLL, existing/default-retain, and deliberate `ForceRefreshMultiMonitorDll = true` remove/copy refresh. Return the repository/default toggle to `false`.
5. Confirm the scenario action settings before execution; do not rely on process linger for application persistence.
6. Compile each script-only, DLL-backed, and integrated workload from disposable copies.
7. Run the Notepad/Paint initializer independently. Confirm `START` supplies the durable Notepad `MainWindow`, the existing Paint flow finds its real window, and state/log results match the active topology. Paint may remain open while a `START`-owned Notepad is stopped by the Standalone Engine; record this as harness lifecycle behavior.
8. Use the three DLL-backed workloads as the proven simple platform regression harness; their serial execution and cross-workload state are now established in Desktop Connector.
9. For raw-launch workloads retained elsewhere, verify their explicit handoff/window-discovery logic. Do not assume the initially spawned PID owns the visible application UI.
10. Repeat DLL loading negative tests: remove or rename the local staged DLL and verify consumers fail with prepare-workload guidance rather than downloading it.
11. Validate one, three, then four displays where available; two-display physical placement is already proven for the simple DLL-backed harness.
12. Validate a topology with negative X/Y coordinates.
13. Validate corrupt state and a monitor-count change. Missing-state recovery is already proven in the DLL-backed Edge run.
14. Compile and run each integrated Office adaptation in Script Editor, checking timer boundaries and the durable-window checklist below.
15. Validate integrated Edge Start discovery success/failure timing against its configured launch timeout, then validate Edge Run including window identity and every later maximize/focus reassertion.
16. Proceed to the final three-workload mini-project below.
17. Repeat execution and inspect state, placement timing, logs, window identity, and application behavior.
18. Validate in the intended VDI environment and test DPI/scaling/topology variants.

## Next mini-project: final three-workload Preview flow

Build, in a separate implementation pass:

1. Prepare the multi-monitor helper.
2. Open applications, resolve their durable base windows, and round-robin place them.
3. Close the applications cleanly.

The Open/Place workload must use scenario-controlled `Leave application running` when the Close workload is expected to run later. The Close workload must own explicit cleanup. Preserve `Run once` intent when adapting the flow to Continuous Test or Load Test.

Do not implement this final flow as part of the current evidence-only pass. After it exists, validate application results/events, durable-window identity, timer boundaries, cleanup, and the complete representative sequence.

## Durable-window runtime checklist

For every allocating workload, record the selected window's title, class, process, and HWND immediately before placement. Confirm that it is the real durable/base application UI rather than a splash or temporary launcher; that the HWND remains the appropriate base window during subsequent workload actions where the application is expected to retain it; and that dialogs, popups, reminders, Outlook open/compose windows, and other secondary windows never call `PlaceNext` or advance `LastUsedIndex`. Confirm maintenance calls retain the target and report `StateAdvanced=false`.

If the correctly identified window still requires settling, record the application, observed failure/race, tested delay, and minimum justified workload-level readiness value. Default `PrePlacementReadinessDelayMilliseconds` to `0`. Do not confuse that post-identification wait with the helper's restore/move/maximize/verification stabilization delay.

## Timing evidence

Every placement result includes elapsed milliseconds. Capture both placement time and overall cadence effects. Verify existing application-response timers exclude placement where documented; do not silently redefine measurement intent.

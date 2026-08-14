# Testing and validation

## Evidence vocabulary

- **Planned:** intended but not executed.
- **Generated / not validated:** source exists without the required runtime evidence.
- **Locally build-tested:** compiled and exercised by the dependency-free local test harness.
- **Proven in Script Editor:** compiled and executed as an individual workload in Script Editor/standalone runner.
- **Proven in full Login Enterprise test:** exercised across independent files in the actual sequential scenario.
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

This is local pure-logic/failure-path evidence only. No test claims that a window moved on an interactive desktop.

## Script Editor versus actual scenario

Script Editor/standalone runner validates individual workload compilation, launch, correct durable `IWindow`, ScriptContent staging, DLL loading, placement, logs, and failure handling. Running both phases inside one script does not prove persistence across separate workload files.

An actual Login Enterprise scenario is required for the two-file proof, state across independent executions, Start/Run behavior, complete representative ordering, and end-to-end behavior. Preserve the scenario's enabled, `Run once`, and `Leave application running` settings.

## Manual test order

1. Run `build.ps1` and retain the console output.
2. Treat `FindWindows` named-argument casing as a blocking Script Editor compile check: the supplied syntax example uses `classname`/`processname`, the parameter table uses `className`/`processName`, and no preserved known-good call resolves the conflict. Do not change the generated lowercase call sites without runtime/signature evidence.
3. Build or obtain `dist/LoginVSI.MultiMonitor.dll`, upload it to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`, and compile `workloads/dll-backed/00-Prepare-MultiMonitor.cs` in Script Editor.
4. Run the prepare workload with `ForceRefreshMultiMonitorDll = false` against a missing local DLL. Verify directory creation, copy from `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"`, and the resulting `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`.
5. Run it again with the default toggle and verify the existing local DLL is retained without another copy. Then deliberately test `true`: verify remove, copy, destination existence, and clear refresh logs. Return the toggle to `false`.
6. Confirm that updating the appliance DLL alone does not replace an existing target-local DLL while the toggle is false.
7. Compile each script-only file in Script Editor.
8. Run script-only file 01 independently; verify Notepad/Paint logs and state.
9. Run script-only file 02 independently only for local behavior; do not label this cross-workload proof.
10. Repeat compilation/execution for DLL-backed files and confirm reflection loading. Remove or rename the local DLL for a negative test and verify each consumer fails with the prepare-workload guidance rather than downloading it.
11. Validate one, two, three, then four displays where available.
12. Validate a topology with negative X/Y coordinates.
13. Validate missing/corrupt state and a monitor-count change.
14. Run the two distinct files in a real scenario and verify `0,1,2,3` cycling as applicable.
15. Compile and run each integrated Office adaptation in Script Editor, checking timer boundaries and the durable-window checklist below.
16. Validate Edge Start discovery success/failure timing against its configured launch timeout, then validate Edge Run including window identity and every later maximize/focus reassertion.
17. Manually add the multi-monitor prepare workload before existing Office/M365 preparation in a test scenario; do not modify the preserved authoritative transcription. The conceptual order is multi-monitor prepare, existing Office/M365 preparation, application Start workloads, then Run workloads.
18. Run the complete authoritative enabled sequence in an actual scenario.
19. Repeat execution and inspect state, placement timing, logs, window identity, and application behavior.
20. Validate in the intended VDI environment and test DPI/scaling/topology variants.

## Durable-window runtime checklist

For every allocating workload, record the selected window's title, class, process, and HWND immediately before placement. Confirm that it is the real durable/base application UI rather than a splash or temporary launcher; that the HWND remains the appropriate base window during subsequent workload actions where the application is expected to retain it; and that dialogs, popups, reminders, Outlook open/compose windows, and other secondary windows never call `PlaceNext` or advance `LastUsedIndex`. Confirm maintenance calls retain the target and report `StateAdvanced=false`.

If the correctly identified window still requires settling, record the application, observed failure/race, tested delay, and minimum justified workload-level readiness value. Default `PrePlacementReadinessDelayMilliseconds` to `0`. Do not confuse that post-identification wait with the helper's restore/move/maximize/verification stabilization delay.

## Timing evidence

Every placement result includes elapsed milliseconds. Capture both placement time and overall cadence effects. Verify existing application-response timers exclude placement where documented; do not silently redefine measurement intent.

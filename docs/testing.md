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

Script Editor/standalone runner validates individual workload compilation, launch, correct `IWindow`, DLL loading, placement, logs, and failure handling. Running both phases inside one script does not prove persistence across separate workload files.

An actual Login Enterprise scenario is required for the two-file proof, state across independent executions, Start/Run behavior, complete representative ordering, and end-to-end behavior. Preserve the scenario's enabled, `Run once`, and `Leave application running` settings.

## Manual test order

1. Run `build.ps1` and retain the console output.
2. Stage `dist/LoginVSI.MultiMonitor.dll` at `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`.
3. Compile each script-only file in Script Editor.
4. Run script-only file 01 independently; verify Notepad/Paint logs and state.
5. Run script-only file 02 independently only for local behavior; do not label this cross-workload proof.
6. Repeat compilation/execution for DLL-backed files and confirm reflection loading.
7. Validate one, two, three, then four displays where available.
8. Validate a topology with negative X/Y coordinates.
9. Validate missing/corrupt state and a monitor-count change.
10. Run the two distinct files in a real scenario and verify `0,1,2,3` cycling as applicable.
11. Compile and run each integrated Office adaptation in Script Editor, checking timer boundaries.
12. Validate Edge Start then Run, including every later maximize/focus reassertion.
13. Run the complete authoritative enabled sequence in an actual scenario.
14. Repeat execution and inspect state, placement timing, logs, window identity, and application behavior.
15. Validate in the intended VDI environment and test DPI/scaling/topology variants.

## Timing evidence

Every placement result includes elapsed milliseconds. Capture both placement time and overall cadence effects. Verify existing application-response timers exclude placement where documented; do not silently redefine measurement intent.

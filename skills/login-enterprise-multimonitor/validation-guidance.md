# Validation guidance

Status: partially executed ladder. Record source commit/diff identity, Login Enterprise version, Windows/runtime, topology, steps, logs, screenshots where useful, and outcome. Generated code is not proven.

Runtime-proven on August 18, 2026 in Login Enterprise 6.8.6 Script Editor/Standalone Engine: prepare compile/run; local ScriptContent initial staging and forced refresh; DLL loading; `className`/`processName` compiler casing; durable `START`/`MainWindow` for simple Notepad and Edge; two-physical-monitor Notepad -> 0, Paint -> 1, later Edge -> 0 placement; state continuation across separate standalone executions; and missing-state recovery. This is not actual platform multi-workload orchestration.

Runtime-proven in a real Login Enterprise 6.8.6 Desktop Connector Application Test in a Console / NoRemote session: appliance ScriptContent delivery; missing/default-retain/forced-refresh Prepare paths; serial execution of `00-Prepare-MultiMonitor` and the two files now retained under `workloads/dll-backed/regression/`; platform cross-workload state; two-monitor `0,1,0`; final `MonitorCount=2` / `LastUsedIndex=0`; and three successful AppExecutions.

1. **Static/build validation** — run `build.ps1`, the unit harness, reference hashes, public-safety scan, and source review. This proves pure logic/build only.
2. **Script Editor compile** — compile each individual script-only, DLL-backed, and integrated workload.
3. **Preview DLL preparation** — local and appliance ScriptContent plus missing/default-retain/forced-refresh paths are proven in the tested environments. Recheck when the environment or implementation changes and return the toggle to false.
4. **Script Editor individual execution** — validate launch, durable/base `IWindow`, placement result, overhead log, and cleanup.
5. **One display** — the three-application harness expects `0,0,0`.
6. **Two displays** — the three-application harness expects `0,1,0`; this is runtime-proven in standalone and Desktop Connector platform runs.
7. **Three displays** — the three-application harness expects `0,1,2`.
8. **Four displays where available** — add an intentional fourth allocation to verify `0,1,2,3`; do not infer index 3 from a three-application run.
9. **Negative-coordinate topology** — include a display left of or above primary and verify signed bounds/order.
10. **State persistence** — schema and simple platform cross-workload continuity are proven; validate verified-success-only advancement and lifecycle-specific persistence in the final flow.
11. **Regression workload files** — use the two files under `workloads/dll-backed/regression/` when repeating the already-proven simple evidence sequence.
12. **Actual sequential Login Enterprise test** — simple regression execution and cross-file state are proven; repeat for the generated canonical flow.
13. **Monitor-count change** — change active display count and verify reset to primary-first allocation.
14. **Missing/corrupt state** — missing-state recovery is proven in the standalone DLL-backed Edge run; verify corrupt-state recovery and diagnostics at runtime.
15. **DLL loading** — validate target-local `Assembly.LoadFrom`, reflection contract, missing/corrupt DLL paths, consumer no-download behavior, and runtime compatibility.
16. **DLL-backed workloads** — repeat the sequential proof through the reusable contract.
17. **Office integration** — record title/class/process/HWND for the actual Outlook Inbox and Excel/PowerPoint/Word document base windows; exclude dialogs, reminders, open/compose windows, and confirm placement remains outside open-document timers.
18. **Browser integration** — test no existing Edge window and one or more existing windows; confirm the launched top-level window is not a splash/launcher and record later identity behavior.
19. **Persistent Start/Run behavior** — confirm Run reuses the Start destination and maintenance calls do not advance state.
20. **Repeated execution** — check cycling, locks, state integrity, application cleanup, and cadence.
21. **Canonical three-workload Preview flow** — validate `00-Prepare-MultiMonitor` off/not relevant -> `01-Open-Place-Applications` with `Leave application running` on -> `02-Close-Applications` off in a Desktop Connector Application Test. On two monitors expect `0,1,0`, all three applications at handoff, explicit bounded cleanup, and unchanged final `MonitorCount=2` / `LastUsedIndex=0`. This source is generated/not runtime-proven.
22. **Complete Knowledge Worker sequence** — preserve the authoritative ordering/settings and capture end-to-end evidence after the final flow exists.
23. **VDI/Horizon validation** — record platform/session/topology details; do not generalize one result to unsupported platforms.
24. **Timing/measurement validation** — quantify placement/reassertion overhead and confirm timer boundaries, workload intent, and scenario cadence remain acceptable.

For every allocating workload, prove that the selected HWND is the real durable application UI, not a splash; record whether it remains the same appropriate base window during later actions where expected; verify secondary windows never advance round-robin state; and confirm maintenance placement reports no advancement. If an application-specific readiness delay is necessary after correct HWND identification, record the observed race, configured value, and justification. Keep it distinct from placement stabilization.

Non-blocking messages in the proven local Desktop Connector session included ICA/Blast/PCoIP probes before NoRemote resolution, unavailable latency, schedule-controlled `forceKillOnExit`, and an ARM `Microsoft.DiaSymReader.Native.amd64.dll` load message followed by successful compile/run. Record these as environment observations, not placement failures.

Never report cross-workload, interactive placement, DLL runtime, complete-scenario, or VDI status from unit-test evidence.

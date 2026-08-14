# Validation guidance

Status: planned ladder. Record source commit/diff identity, Login Enterprise version, Windows/runtime, topology, steps, logs, screenshots where useful, and outcome. Generated code is not proven.

1. **Static/build validation** — run `build.ps1`, the unit harness, reference hashes, public-safety scan, and source review. This proves pure logic/build only.
2. **Script Editor compile** — compile each individual script-only, DLL-backed, and integrated workload.
3. **Preview DLL preparation** — upload to appliance ScriptContent; compile and run `00-Prepare-MultiMonitor.cs`; test missing, existing/default-retain, and existing/forced-refresh paths; verify the exact target-local path and return the toggle to false.
4. **Script Editor individual execution** — validate launch, durable/base `IWindow`, placement result, overhead log, and cleanup.
5. **One display** — expect sequential allocation `0,0,0,0`.
6. **Two displays** — expect `0,1,0,1`.
7. **Three displays** — expect `0,1,2,0`.
8. **Four displays where available** — expect the first four placements `0,1,2,3`.
9. **Negative-coordinate topology** — include a display left of or above primary and verify signed bounds/order.
10. **State persistence** — validate schema, verified-success-only advancement, and restart continuity.
11. **Separate workload files** — run `01-Initialize-Notepad-Paint.cs`, then `02-Continue-Cmd-Edge.cs` as independent executions.
12. **Actual sequential Login Enterprise scenario** — prove cross-file state; Script Editor alone cannot establish this.
13. **Monitor-count change** — change active display count and verify reset to primary-first allocation.
14. **Missing/corrupt state** — verify safe recovery and diagnostics.
15. **DLL loading** — validate target-local `Assembly.LoadFrom`, reflection contract, missing/corrupt DLL paths, consumer no-download behavior, and runtime compatibility.
16. **DLL-backed workloads** — repeat the sequential proof through the reusable contract.
17. **Office integration** — record title/class/process/HWND for the actual Outlook Inbox and Excel/PowerPoint/Word document base windows; exclude dialogs, reminders, open/compose windows, and confirm placement remains outside open-document timers.
18. **Browser integration** — test no existing Edge window and one or more existing windows; confirm the launched top-level window is not a splash/launcher and record later identity behavior.
19. **Persistent Start/Run behavior** — confirm Run reuses the Start destination and maintenance calls do not advance state.
20. **Repeated execution** — check cycling, locks, state integrity, application cleanup, and cadence.
21. **Complete Knowledge Worker sequence** — preserve the authoritative ordering/settings and capture end-to-end evidence.
22. **VDI/Horizon validation** — record platform/session/topology details; do not generalize one result to unsupported platforms.
23. **Timing/measurement validation** — quantify placement/reassertion overhead and confirm timer boundaries, workload intent, and scenario cadence remain acceptable.

For every allocating workload, prove that the selected HWND is the real durable application UI, not a splash; record whether it remains the same appropriate base window during later actions where expected; verify secondary windows never advance round-robin state; and confirm maintenance placement reports no advancement. If an application-specific readiness delay is necessary after correct HWND identification, record the observed race, configured value, and justification. Keep it distinct from placement stabilization.

Never report cross-workload, interactive placement, DLL runtime, complete-scenario, or VDI status from unit-test evidence.

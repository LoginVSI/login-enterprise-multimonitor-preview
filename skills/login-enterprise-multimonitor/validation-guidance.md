# Validation guidance

Status: planned ladder. Record source commit/diff identity, Login Enterprise version, Windows/runtime, topology, steps, logs, screenshots where useful, and outcome. Generated code is not proven.

1. **Static/build validation** — run `build.ps1`, the unit harness, reference hashes, public-safety scan, and source review. This proves pure logic/build only.
2. **Script Editor compile** — compile each individual script-only, DLL-backed, and integrated workload.
3. **Script Editor individual execution** — validate launch, safest `IWindow`, placement result, overhead log, and cleanup.
4. **One display** — expect sequential allocation `0,0,0,0`.
5. **Two displays** — expect `0,1,0,1`.
6. **Three displays** — expect `0,1,2,0`.
7. **Four displays where available** — expect the first four placements `0,1,2,3`.
8. **Negative-coordinate topology** — include a display left of or above primary and verify signed bounds/order.
9. **State persistence** — validate schema, verified-success-only advancement, and restart continuity.
10. **Separate workload files** — run `01-Initialize-Notepad-Paint.cs`, then `02-Continue-Cmd-Edge.cs` as independent executions.
11. **Actual sequential Login Enterprise scenario** — prove cross-file state; Script Editor alone cannot establish this.
12. **Monitor-count change** — change active display count and verify reset to primary-first allocation.
13. **Missing/corrupt state** — verify safe recovery and diagnostics.
14. **DLL loading** — stage the assembly, validate `Assembly.LoadFrom`, reflection contract, missing/corrupt DLL paths, and runtime compatibility.
15. **DLL-backed workloads** — repeat the sequential proof through the reusable contract.
16. **Office integration** — validate the actual Outlook/Excel/PowerPoint/Word window and that placement remains outside open-document timers.
17. **Browser integration** — test no existing Edge window and one or more existing windows; confirm the launched window and later behavior.
18. **Persistent Start/Run behavior** — confirm Run reuses the Start destination and maintenance calls do not advance state.
19. **Repeated execution** — check cycling, locks, state integrity, application cleanup, and cadence.
20. **Complete Knowledge Worker sequence** — preserve the authoritative ordering/settings and capture end-to-end evidence.
21. **VDI/Horizon validation** — record platform/session/topology details; do not generalize one result to unsupported platforms.
22. **Timing/measurement validation** — quantify placement/reassertion overhead and confirm timer boundaries, workload intent, and scenario cadence remain acceptable.

Never report cross-workload, interactive placement, DLL runtime, complete-scenario, or VDI status from unit-test evidence.

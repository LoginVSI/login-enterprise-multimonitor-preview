# Validation guidance

Status: Planned ladder. Record source/build identity, environment, topology, steps, results, and evidence at every executed level. Generated output is not proven. Script Editor evidence does not prove cross-workload behavior.

1. **Static/build validation** — syntax, pure logic, packaging, and available local checks.
2. **Script Editor compile** — compile the individual workload in the supported editor/runtime.
3. **Script Editor individual execution** — exercise launch, window identification, placement, results, and logs.
4. **One display** — verify safe deterministic behavior without a secondary display.
5. **Two displays** — verify discovery, primary-first order, placement, and cycling.
6. **Three displays** — verify ordering and repeated selection.
7. **Four displays where available** — extend topology and cycling evidence.
8. **Negative-coordinate topology** — place displays left of/above primary and verify signed bounds.
9. **State persistence** — verify initialization, updates, recovery, and deterministic continuation.
10. **Separate workload files** — exercise truly independent workload executions.
11. **Actual sequential Login Enterprise scenario** — prove cross-workload state and end-to-end ordering.
12. **Monitor-count change** — add/remove/disable displays between executions and verify recovery.
13. **Missing/corrupt state** — verify graceful initialization and failure information.
14. **DLL loading** — validate discovery, compatible runtime loading, versioning, and failure paths.
15. **DLL-backed workloads** — validate the reusable contract from individual workloads.
16. **Office integration** — validate correct application windows without coupling generic logic.
17. **Browser integration** — validate existing-instance and multiprocess/window behavior.
18. **Persistent Start/Run behavior** — validate long-lived applications across separate files.
19. **Repeated execution** — verify cycling, state integrity, cadence, and stable behavior.
20. **Complete Knowledge Worker sequence** — exercise the preserved representative configuration in an actual scenario.
21. **VDI/Horizon validation** — document platform, session, topology, and evidence; treat Horizon only as a technically useful generic example.
22. **Timing/measurement validation** — quantify overhead and confirm existing measurement boundaries and intent remain intact.

# Testing and validation

## Evidence vocabulary

- **Generated/build-tested/static-validated:** source and automated contracts pass; no Login Enterprise runtime claim.
- **Proven in Script Editor:** one workload compiled and executed in Script Editor/Standalone Engine.
- **Proven in full Login Enterprise test:** independent workloads executed through the actual platform.
- **Proven in VDI:** exercised in a documented VDI protocol/environment.

## Automated test taxonomy

Run all automated repository gates with:

```powershell
.\scripts\Test-Repository.ps1
```

| Layer | Current coverage | What it does not prove |
| --- | --- | --- |
| Smoke | Restore/build and executable test-harness launch | Login Enterprise compilation or UI automation |
| Unit | Next-index sequences, serialization, parsing, state repair | Real files shared across platform workloads |
| Functional/pure logic | Primary-first ordering, negative coordinates, atomic replacement, invalid-HWND result | Interactive window movement |
| Source-contract/static | Canonical Close neutrality, workload API casing, staged paths, Office allocation count, Knowledge Worker manifest/deltas/timers/Start-Run contracts, compiled DLL framework/reference contract | Application window durability at runtime |
| Repository integrity | Diff checks, reference hashes, preserved-evidence hashes, public-safety scan, artifact hygiene | Reachable-history security audit or legal approval |
| Runtime/manual | Script Editor and Desktop Connector evidence below | Untested releases/topologies/protocols |

`-Fast` runs repository integrity and static contracts only. CI runs the full command on Windows for every push and pull request to `main`.

## Runtime-proven generic baseline

Login Enterprise 6.8.6 Script Editor/Standalone Engine and Desktop Connector testing established:

- local-engine and appliance ScriptContent delivery of `LoginVSI.MultiMonitor.dll`;
- missing/default-retain/forced-refresh Prepare paths;
- compiler-required `FindWindows(className: ..., processName: ...)` casing;
- durable `START`/`MainWindow` behavior for the tested Notepad and Edge paths;
- two physical monitors and verified `Notepad -> 0`, `Paint -> 1`, `Edge -> 0` round robin;
- cross-workload state continuation and missing-state recovery;
- actual serial workload execution in a Console / NoRemote Desktop Connector Application Test;
- canonical generic Prepare -> Open/Place -> Close execution, scenario-controlled handoff, and cleanup behavior.

This evidence is specific to the recorded environment. Corrupt-state and monitor-count-change recovery are unit-tested but remain separate runtime items. Mixed DPI, other releases, broader topologies, and VDI protocols remain unproven.

## New workload status

The Office Preview and `knowledge-worker-multimonitor` sets are **generated/build-tested/static-validated; partner-lab runtime validation pending**. Passing CI must not change that label.

The Knowledge Worker contracts verify:

- every preserved original has exactly one mapped adaptation;
- originals remain hash-protected;
- `TARGET`, primary class, and timer names remain present;
- line deltas remain within the reviewed manifest budget;
- Outlook, Edge Start, Excel, PowerPoint, and Word each contain one allocation;
- Edge Run uses `PlaceLastUsed`/`PlaceOnMonitor` without `PlaceNext`;
- preparation and Close workloads do not allocate or access placement state.

## Partner-lab validation

Follow [test-lab-quickstart.md](test-lab-quickstart.md). For every allocating workload, record the durable window title/class/process/HWND immediately before placement and verify that secondary/transient windows never allocate.

Validate at least:

1. Office Preview Word, Excel, PowerPoint, Outlook, and Edge in documented order.
2. Complete preserved scenario order using the adapted files and original `Run once`/`Leave application running` intent.
3. Outlook Inbox allocation while reminders, read, and compose windows remain non-allocating.
4. Edge Start allocates the newly identified base window once; Edge Run reuses that destination and reports `StateAdvanced=false` for maintenance.
5. Excel, PowerPoint, and Word allocate only after their open-document timers stop, then reassert rather than reallocate after minimize/maximize.
6. Close workloads clean up without changing the final state.
7. Application events/results, structured placement results, timing/cadence impact, and failure behavior.
8. Existing-instance ambiguity, first-run UI, localization, application versions, media staging, and the active monitor topology.

Repository files remain source of truth. Test disposable copies in Script Editor because the editor may rewrite its working representation or line endings. Do not commit raw Engine logs.

## Non-blocking recorded environment observations

The proven local Desktop Connector session emitted ICA/Blast/PCoIP probe warnings before resolving NoRemote, did not report latency, explained schedule-controlled `forceKillOnExit`, and logged an ARM `Microsoft.DiaSymReader.Native.amd64.dll` load message before successful compile/run. These were environment observations, not placement failures.

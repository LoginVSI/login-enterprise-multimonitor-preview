# Testing and validation

Current runtime claims and their boundaries live in [evidence status](evidence-status.md). This page defines how repository and runtime evidence is produced. Do not copy the full status matrix into another document.

Run the authoritative repository gate:

```powershell
.\scripts\Test-Repository.ps1
```

`-Fast` runs integrity and static/source contracts without restore, build, or unit execution. CI runs the full command on Windows for pushes and pull requests to `main`.

## Test taxonomy

| Category | Automated coverage | It does not prove |
| --- | --- | --- |
| Smoke | Restore, clean Release build, executable harness, distributable copy | Login Enterprise compilation or UI behavior |
| Unit | Round robin, bounds, parsing, duplicate keys, serialization, repair | Interactive placement |
| Functional/pure logic | Primary-first order, negative coordinates, atomic replacement, lock serialization/timeout, invalid-HWND failure | Real multi-display behavior |
| Source-contract/static | DLL path/API casing, lifecycle allocation, Office ownership, Edge/Outlook rules, KW mappings/deltas/timers/substitutions | Durable application identity at runtime |
| Repository integrity | Whitespace, immutable hashes, public safety, artifact hygiene, DLL target/dependencies/checksum, Markdown links and critical paths | Legal approval or Login Enterprise behavior |
| Script Editor | Individual workload compile and execution | Platform sequencing or cross-file state |
| Actual Login Enterprise scenario | Named workload order, lifecycle, application behavior, placement, state, and cleanup in one environment | Untested versions, topologies, applications, or protocols |

CI deliberately does not fake Login Enterprise end-to-end testing.

## Runtime validation ladder

1. Record the source commit/diff, Login Enterprise and Engine versions, Windows/application versions, Connector/session, display topology/scaling, scenario type, and scenario settings.
2. Verify the distributable checksum and stage it through the intended ScriptContent/Prepare path. Exercise missing, retain-existing, and forced-refresh branches when deployment behavior is in scope.
3. Compile disposable copies of every workload in Script Editor. Do not let Script Editor rewrite protected repository evidence.
4. Execute each workload individually. Record the durable/base title, class, process, HWND strategy, placement result, application result, overhead, and cleanup.
5. Run the actual Application Test, Continuous Test, or Load Test in authoritative order. Record `Run once`, `Leave application running`, Start/Run/Close handoff, and final state.
6. On one monitor, expect every allocation at `0`. On two monitors, three fresh allocations expect `0,1,0`. On three, they expect `0,1,2`.
7. Confirm `TargetMonitorIndex == VerifiedMonitorIndex`. `PlaceNext` advances only after verified success. Maintenance reports `StateAdvanced=False`.
8. Prove that splash/setup/dialog/popup/compose/read/reminder/child windows do not consume an allocation.
9. Validate application-specific process handoff, existing-instance ambiguity, base-window replacement, self-repositioning, first-run/profile readiness, localization, and content/media prerequisites.
10. Repeat enough complete loops to support the claimed resilience level. One successful loop is not Continuous/Load resilience evidence.
11. Exercise missing/corrupt state, monitor-count change, negative-coordinate layouts, mixed DPI, topology changes, concurrency, and additional Connectors where those claims are intended.
12. Confirm placement remains outside existing EUX/application-response timers wherever practical and quantify added overhead/cadence effects.

## Application-specific review points

- **Classic Outlook:** verify configured profile/import lifecycle, durable Inbox Explorer, and non-allocating compose/read/reminder windows. Keep it distinct from New Outlook.
- **New Outlook:** local evidence covers `TARGET:olk` launch/find/place only. Interaction automation needs its own controls and runtime evidence.
- **Edge/Chromium:** do not assume the spawned PID owns the durable UI. Test zero, one, and multiple existing windows, Start/Run identity, and later self-repositioning.
- **Word/Excel/PowerPoint:** allocate the durable document window after the source open timer and use non-allocating maintenance after later state changes.
- **Prepare/Close:** verify they leave allocation state untouched and cleanup remains bounded.

## Evidence record

For every runtime claim capture:

- commit and uncommitted diff identity;
- environment and display topology;
- workload files, order, and lifecycle settings;
- durable/base window evidence;
- structured placement result and state before/after;
- application events/results, timer placement, and cleanup;
- iteration/loop count and observed failures;
- reviewed screenshots or minimized log excerpts when useful.

Keep raw Engine logs private because they can contain authentication, session, infrastructure, or personal information. Record environment observations separately from placement failures. In particular, do not attribute an EUX or metrics interruption to the Preview without causal evidence.

Update [evidence status](evidence-status.md) only after review. Clearly label build/static validation, local runtime evidence, partner-lab runtime evidence, pending multi-loop work, and broader untested compatibility.

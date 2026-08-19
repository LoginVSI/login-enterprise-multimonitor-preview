# Testing and validation

Run the authoritative gate:

```powershell
.\scripts\Test-Repository.ps1
```

`-Fast` runs repository integrity and static/source contracts without restore/build/unit execution. CI runs the full command on Windows for pushes and pull requests to `main`.

## Test taxonomy

| Category | Automated coverage | It does not prove |
| --- | --- | --- |
| Smoke | Restore, clean Release build, executable test harness, distributable copy | Login Enterprise compilation/UI behavior |
| Unit | Round robin, bounds, parsing, duplicate keys, serialization, repair | Interactive placement |
| Functional/pure logic | Primary-first ordering, negative coordinates, atomic replacement, lock serialization/timeout, invalid-HWND failure | Real multi-display behavior |
| Source-contract/static | DLL paths/API casing, allocation lifecycle, Office ownership/Edge/Outlook rules, KW mapping/deltas/timer sequence/substitutions, action SHA pins | Durable application identity at runtime |
| Repository integrity | Git whitespace, immutable hashes, public safety, artifact hygiene, DLL target/dependency/checksum, expected public paths | Legal approval or Login Enterprise behavior |
| Runtime/manual | Script Editor and actual platform evidence | Untested versions/topologies/protocols |

CI deliberately does not fake Login Enterprise end-to-end tests.

## Validation matrix

| Dimension | Evidence |
| --- | --- |
| Login Enterprise 6.8.6 generic framework | Runtime-proven |
| Script Editor/Standalone Engine loading/staging | Runtime-proven |
| Appliance ScriptContent and all Prepare branches | Runtime-proven |
| Desktop Connector Console / NoRemote | Runtime-proven |
| Physical two-monitor generic flow | Runtime-proven (`Notepad 0`, `Paint 1`, `Edge 0`) |
| Cross-workload state and generic Prepare/Open/Close | Runtime-proven |
| Missing-state recovery | Runtime-proven |
| Corrupt state and topology-change recovery | Automated only; runtime pending |
| 3+ monitors | Algorithmically covered; physical runtime pending |
| Negative coordinates / mixed DPI / high resolution | Logic covered where stated; runtime pending |
| Office Preview Word/Excel/PowerPoint/classic Outlook/Edge | Generated/build-tested/static-validated; partner-lab pending |
| Representative ten-file KW adaptation | Generated/build-tested/static-validated; partner-lab pending |
| New Outlook | Unsupported/unvalidated |
| Other Login Enterprise/Office/Windows versions and VDI protocols | Pending |

## Partner-lab runtime validation

Follow [test-lab-quickstart.md](test-lab-quickstart.md). For each workload record the selected durable title/class/process/HWND, structured result, state before/after, application events, scenario settings, and timing. Verify:

1. Word, Excel, PowerPoint, classic Outlook, and Edge Office examples in order, including existing-instance failure behavior.
2. All ten adapted files in the preserved scenario order and lifecycle settings.
3. Outlook secondary compose/read/reminder windows never allocate.
4. Edge Start owns one new base window; Edge Run reports `StateAdvanced=false` and never allocates.
5. Word/Excel/PowerPoint allocation occurs after original open timers, with maintenance rather than reallocation later.
6. Preparation and Close remain state-neutral; cleanup and final state are correct.
7. Content/media prerequisites, original interactions, first-run/profile/localization, and failure diagnostics.
8. Relevant Windows/Office/LE/VDI/topology/scaling dimensions.

Repository files remain source of truth. Test disposable copies in Script Editor because it may rewrite working representation/line endings. Never publish raw Engine logs.

Recorded non-blocking Desktop Connector observations include ICA/Blast/PCoIP probes before NoRemote resolution, unavailable latency, schedule-controlled `forceKillOnExit`, and an ARM DIA symbol-reader load message followed by successful compile/run. They were not placement failures.

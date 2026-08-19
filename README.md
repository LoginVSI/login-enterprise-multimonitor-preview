# Login Enterprise Multi-Monitor Preview

An unsupported Preview for distributing compatible Login Enterprise C# workload windows across active Windows displays. The reusable library is application-neutral: workloads identify the correct durable `IWindow`; the helper receives `NativeWindowHandle` and owns monitor discovery, primary-first ordering, persistent round robin, native placement, verification, and structured results.

> **Want to test this in a lab? Start with the [test-lab quickstart](docs/test-lab-quickstart.md).**

This is a public working example, not a generally available feature, support commitment, compatibility promise, or delivery commitment.

## What is here

- `dist/LoginVSI.MultiMonitor.dll` — dependency-free `netstandard2.0` Preview helper.
- `workloads/dll-backed/` — runtime-proven generic Prepare -> Open/Place -> Close flow and retained regression harness.
- `workloads/office-preview/` — small Word, Excel, PowerPoint, Outlook, and Edge examples for first lab use.
- `workloads/knowledge-worker-multimonitor/` — complete minimal-delta adaptations of the preserved representative workload set, with a machine-checked mapping manifest.
- `reference/` — immutable originals, proven POCs, supplied Login Enterprise scripting documentation, and scenario transcription.
- `scripts/Test-Repository.ps1` — authoritative build, test, integrity, public-safety, DLL, and workload-contract gate.
- `skills/login-enterprise-multimonitor/` — repository AI guidance grounded in the implementation and recorded evidence.

## Validation status

| Area | Status |
| --- | --- |
| Generic DLL loading, ScriptContent delivery, Prepare branches, physical two-monitor placement, state continuity | Runtime-proven in Login Enterprise 6.8.6 |
| Generic canonical Prepare -> Open/Place -> Close lifecycle and cleanup | Runtime-proven in the recorded Desktop Connector Console / NoRemote test |
| Office Preview examples | Generated/build-tested/static-validated; partner-lab runtime validation pending |
| Knowledge Worker/KW25 adaptations | Generated/build-tested/static-validated; partner-lab runtime validation pending |
| Other Login Enterprise releases, mixed DPI, broader topologies, VDI protocols | Not validated |

Automated checks do not simulate Login Enterprise or claim interactive placement. See [testing and validation](docs/testing.md).

## Build and validate

From Windows PowerShell or PowerShell 7:

```powershell
.\scripts\Test-Repository.ps1
```

This runs whitespace checks, both preserved-reference hash verifiers, public-safety and workload/DLL contracts, restore/build, and all unit/pure-logic/source-contract tests. During editing, `-Fast` skips restore/build/unit execution.

`build.ps1` alone restores, builds, tests, and copies the helper to `dist/LoginVSI.MultiMonitor.dll`.

## Preview deployment and state

Upload the DLL to:

```text
/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll
```

Run `workloads/dll-backed/00-Prepare-MultiMonitor.cs` before consumers. It stages:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

`ForceRefreshMultiMonitorDll` defaults to `false`: a missing copy stages, an existing copy is retained, and `true` performs the proven remove/copy/verify refresh. Script Editor/Standalone Engine uses its own local engine ScriptContent directory; no developer-specific installation path is a product requirement.

State is separate:

```text
%TEMP%\LoginPI\MultiMonitor\state.txt
MonitorCount=<integer>
LastUsedIndex=<integer>
```

State starts at `-1`, advances only after verified `PlaceNext`, and safely repairs missing, malformed, out-of-range, or monitor-count-changed state. HWND and monitor handles are never persisted.

## Authoring contract

Only a durable/base application window consumes a destination. Splash, setup, file dialogs, Outlook compose/read/reminder windows, popups, child windows, and temporary launchers do not. Prefer documented `START` when the workload owns startup and needs the durable main UI; otherwise use documented `FindWindow`/`FindWindows`. Login Enterprise 6.8.6 compiler evidence requires named arguments `className` and `processName`.

Start workloads allocate once. Run workloads use `PlaceLastUsed`/`PlaceOnMonitor` maintenance and do not allocate again. Close and preparation workloads do not touch placement state. Keep placement outside EUX/application-response timers wherever practical and preserve original scenario lifecycle settings.

## Evidence, safety, and license

Never modify `reference/original-workloads/`, `reference/proven-pocs/`, or the authoritative scenario transcription. Raw Engine logs belong only in ignored local `artifacts/`; review and minimize diagnostic excerpts before sharing.

License selection remains pending owner/organization approval. Public readability does **not** grant an open-source reuse license. See [LICENSE.md](LICENSE.md).

Architecture, limitations, history, product handoff, and the [publication-readiness review](docs/publication-readiness.md) remain in [`docs/`](docs/).

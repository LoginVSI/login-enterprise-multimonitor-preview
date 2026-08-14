# Login Enterprise Multi-Monitor Preview

This repository contains an early, unsupported Preview and active R&D implementation for distributing compatible Login Enterprise C# workload windows across active Windows displays. It is a public working example, not a generally available feature, support commitment, compatibility promise, or delivery commitment.

The implementation is deliberately application-neutral. Login Enterprise workloads continue to launch applications, identify the correct `IWindow`, preserve application behavior and measurement boundaries, and choose when placement occurs. The reusable helper receives `IWindow.NativeWindowHandle` and owns monitor discovery, primary-first ordering, persistent round-robin selection, native movement, maximize behavior, verification, timing, and structured results.

## Current implementation

- A dependency-free `netstandard2.0` library under `src/LoginVSI.MultiMonitor/`.
- A two-file script-only Notepad/Paint then Command Prompt/Edge sequence.
- Equivalent reflection-loaded DLL-backed workloads plus a dedicated Preview DLL preparation workload.
- Derived Preview adaptations of the enabled representative Office and Edge workloads.
- Pure-logic and safe failure-path tests that run without an interactive desktop.
- A Windows PowerShell 5.1-friendly build and distribution script.
- A draft AI skill and technical Product/Development handoff material.

The library and its 17 local tests build successfully in this repository. Login Enterprise Script Editor compilation, Login Enterprise runtime loading, actual multi-display movement, two-file state continuity, the complete scenario, and VDI behavior remain unvalidated.

## Build

From Windows PowerShell:

```powershell
.\build.ps1
```

The script cleans known build outputs, restores and builds without third-party packages, runs the console test harness, and copies the intentional distributable to `dist/LoginVSI.MultiMonitor.dll`.

## Unsupported Preview DLL deployment

The Preview uses the supplied Knowledge Worker ScriptContent pattern; it is not a formal product distribution or update mechanism. Build or obtain `dist/LoginVSI.MultiMonitor.dll`, then upload that file with SFTP/WinSCP or equivalent to the Login Enterprise appliance at:

```text
/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll
```

Add `workloads/dll-backed/00-Prepare-MultiMonitor.cs` as a run-once preparation step before DLL consumers. It retrieves `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` and stages the assembly at:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

`ForceRefreshMultiMonitorDll` defaults to `false`. A missing local DLL is always staged. An existing local DLL is retained by default, so merely replacing the appliance copy does not update targets that already have one. Set the toggle to `true` for an intentional deployment refresh; the prepare workload removes the existing local file, copies the appliance version, and verifies the destination. Return it to `false` afterward where appropriate. Consumer workloads only verify and load the target-local DLL; they do not repeatedly download or force-refresh it.

State is stored separately in the same directory as `state.txt`:

```text
MonitorCount=<integer>
LastUsedIndex=<integer>
```

The initial index is `-1`. State advances only after the target monitor verifies successfully. Monitor-count changes, missing state, and invalid state reset the index safely. HWND and monitor handles are never persisted.

## Repository map

- `reference/original-workloads/`: immutable known-good baselines protected by SHA-256 and byte-preserving Git attributes.
- `reference/proven-pocs/`: byte-preserved successful POC evidence.
- `reference/login-enterprise-docs/`: supplied scripting/metalanguage reference and examples; the API source of truth.
- `reference/test-scenario/`: authoritative scenario transcription and supporting screenshot.
- `workloads/script-only/`: self-contained placement proofs without DLL loading.
- `workloads/dll-backed/`: the same sequence using the staged managed helper.
- `workloads/integrated/`: derived representative workload adaptations; originals remain unchanged.
- `src/LoginVSI.MultiMonitor/`: reusable state, ordering, Win32, placement, and result implementation.
- `tests/LoginVSI.MultiMonitor.Tests/`: dependency-free console tests.
- `dist/`: intentional Preview distributable output.
- `docs/`: architecture, testing, limitations, requirements context, history, and handoff.
- `skills/login-enterprise-multimonitor/`: draft AI workflow grounded in this implementation.
- `artifacts/`: ignored local logs, screenshots, state, and diagnostics.

## Validation model

Script Editor and the standalone runner can compile, run, and debug one workload at a time. They can validate individual launch, window identification, DLL loading, and placement behavior. They cannot prove state continuity across two independent workload files merely by combining phases into one script.

True cross-workload state, Start/Run relationships, complete Knowledge Worker ordering, and end-to-end behavior require an actual Login Enterprise test scenario. Actual movement also requires an interactive Windows session with the target display topology. See `docs/testing.md`.

## Timing and behavior

Placement is not zero-cost. Restore, stabilization, move, maximize, verification, locking, and state I/O add runtime and cadence overhead. Integrated placement is kept outside existing application-response timers wherever practical, and the structured result reports elapsed milliseconds.

Only a correctly identified durable/base application window consumes a destination. Splash screens, setup and file dialogs, Outlook compose/read/reminder windows, popups, child interaction windows, and temporary launchers do not. Workloads should prefer a sufficiently specific `START`, otherwise resolve the durable `IWindow` with `FindWindow`/`FindWindows`, and only then pass its `NativeWindowHandle` to the helper. An optional workload-level readiness delay may follow identification and defaults to zero; it is distinct from the helper's placement stabilization delay.

The helper does not change the Windows primary monitor. It only orders the reported primary display first for round-robin selection. Edge remains the highest-risk integration because later browser actions repeatedly focus and maximize its persistent window; the adapted Run workload therefore reasserts the previously selected target without advancing state.

## Reference protection and public safety

Never edit files under `reference/original-workloads/` or `reference/proven-pocs/`. Run both `scripts/Verify-ReferenceHashes.ps1 -Verify` and `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after implementation work. Create all adaptations under `workloads/`, label evidence accurately, and run `scripts/Test-PublicSafety.ps1` before publication.

## License and support

License selection remains pending. This unsupported Preview makes no GA, support, compatibility, or release claim.

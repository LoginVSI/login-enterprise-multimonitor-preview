# Login Enterprise Multi-Monitor Preview

This repository contains an early, unsupported Preview and active R&D implementation for distributing compatible Login Enterprise C# workload windows across active Windows displays. It is a public working example, not a generally available feature, support commitment, compatibility promise, or delivery commitment.

The implementation is deliberately application-neutral. Login Enterprise workloads continue to launch applications, identify the correct `IWindow`, preserve application behavior and measurement boundaries, and choose when placement occurs. The reusable helper receives `IWindow.NativeWindowHandle` and owns monitor discovery, primary-first ordering, persistent round-robin selection, native movement, maximize behavior, verification, timing, and structured results.

## Current implementation

- A dependency-free `netstandard2.0` library under `src/LoginVSI.MultiMonitor/`.
- A two-file script-only Notepad/Paint then Edge regression sequence.
- A canonical reflection-loaded DLL-backed Prepare -> Open/Place -> Close flow, plus the retained proven regression harness.
- Derived Preview adaptations of the enabled representative Office and Edge workloads.
- Pure-logic and safe failure-path tests that run without an interactive desktop.
- A Windows PowerShell 5.1-friendly build and distribution script.
- A draft AI skill and technical Product/Development handoff material.

The library and its 20 local tests build successfully in this repository. Login Enterprise 6.8.6 Script Editor/Standalone Engine testing on August 18, 2026 proved individual compilation, loading, durable-window, placement, and state-recovery behavior. A subsequent real Desktop Connector Application Test in a Console / NoRemote session proved appliance ScriptContent delivery, all three Prepare branches, serial execution of the three independent regression workloads, and platform cross-workload state persistence. The new canonical flow is implemented/generated but not runtime-proven. The complete representative scenario and VDI behavior remain unvalidated.

## Build

From Windows PowerShell:

```powershell
.\build.ps1
```

The script cleans known build outputs, restores and builds without third-party packages, runs the console test harness, and copies the intentional distributable to `dist/LoginVSI.MultiMonitor.dll`.

## Unsupported Preview DLL deployment

The Preview uses the supplied Knowledge Worker ScriptContent pattern; it is not a formal product distribution or update mechanism. The Desktop Connector Application Test proved delivery after uploading `dist/LoginVSI.MultiMonitor.dll` to the Login Enterprise appliance at:

```text
/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll
```

Add `workloads/dll-backed/00-Prepare-MultiMonitor.cs` as a run-once preparation step before DLL consumers. It retrieves `UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll"` and stages the assembly at:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

`ForceRefreshMultiMonitorDll` defaults to `false`. A missing local DLL is always staged. An existing local DLL is retained by default, so merely replacing the ScriptContent copy does not update targets that already have one. Set the toggle to `true` for an intentional deployment refresh; the prepare workload removes the existing local file, copies the ScriptContent version, and verifies the destination. Return it to `false` afterward where appropriate. Consumer workloads only verify and load the target-local DLL; they do not repeatedly download or force-refresh it.

In Script Editor/Standalone Engine development, ScriptContent is resolved from that engine's local ScriptContent directory. Installation paths vary by environment and version and are not a product requirement. Appliance delivery from `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` is now proven for the tested Login Enterprise 6.8.6 Desktop Connector Application Test.

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
- `workloads/dll-backed/`: the canonical staged-DLL flow; `regression/` retains the runtime-proven simple harness.
- `workloads/integrated/`: derived representative workload adaptations; originals remain unchanged.
- `src/LoginVSI.MultiMonitor/`: reusable state, ordering, Win32, placement, and result implementation.
- `tests/LoginVSI.MultiMonitor.Tests/`: dependency-free console tests.
- `dist/`: intentional Preview distributable output.
- `docs/`: architecture, testing, limitations, requirements context, history, and handoff.
- `skills/login-enterprise-multimonitor/`: draft AI workflow grounded in this implementation.
- `artifacts/`: ignored local logs, screenshots, state, and diagnostics.

## Validation model

Script Editor and the Standalone Engine compile, run, and debug one workload at a time. They validate individual launch, window identification, DLL loading, placement, and file behavior. The Desktop Connector Application Test separately proved real serial platform execution and state continuity across the workloads now retained under `workloads/dll-backed/regression/` together with `00-Prepare-MultiMonitor.cs`.

Keep repository files as source of truth. Copy a workload to a disposable location before opening it in Script Editor because the editor may rewrite the working representation or line endings; apply validated changes deliberately back to repository source.

The canonical current Preview flow is `00-Prepare-MultiMonitor.cs`, `01-Open-Place-Applications.cs`, then `02-Close-Applications.cs`. It is implemented/generated and awaits a real Desktop Connector Application Test; a successful build is not runtime proof. The old Notepad/Paint initializer and Edge continuation remain as proven regression evidence under `workloads/dll-backed/regression/`. Complete representative application integration remains a later track. See `docs/testing.md`.

## Timing and behavior

Placement is not zero-cost. Restore, stabilization, move, maximize, verification, locking, and state I/O add runtime and cadence overhead. Integrated placement is kept outside existing application-response timers wherever practical, and the structured result reports elapsed milliseconds.

Only a correctly identified durable/base application window consumes a destination. Splash screens, setup and file dialogs, Outlook compose/read/reminder windows, popups, child interaction windows, and temporary launchers do not. When a workload owns startup and needs the durable main application window, prefer a sufficiently specific Login Enterprise `START`; the initially spawned PID from raw `ShellExecute` is not proof that it owns the visible UI. Otherwise resolve the durable `IWindow` with `FindWindow`/`FindWindows`, and only then pass its `NativeWindowHandle` to the helper. `ShellExecute` remains appropriate where the process/window lifecycle is understood and explicitly handled. An optional workload-level readiness delay may follow identification and defaults to zero; it is distinct from the helper's placement stabilization delay.

The helper does not change the Windows primary monitor. It only orders the reported primary display first for round-robin selection. Edge remains the highest-risk integration because later browser actions repeatedly focus and maximize its persistent window; the adapted Run workload therefore reasserts the previously selected target without advancing state.

Application persistence is scenario-controlled, not inferred from a process lingering. For the canonical Application Test, configure Prepare with `Leave application running` off/not relevant, Open/Place with it on, and Close with it off. Continuous Test and Load Test expose both per-workload `Leave application running` and `Run once`; preserve the intended one-time setup/cleanup semantics when adapting the flow. Close explicitly requests bounded window cleanup and never consumes or resets monitor state.

## Reference protection and public safety

Never edit files under `reference/original-workloads/` or `reference/proven-pocs/`. Run both `scripts/Verify-ReferenceHashes.ps1 -Verify` and `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after implementation work. Create all adaptations under `workloads/`, label evidence accurately, and run `scripts/Test-PublicSafety.ps1` before publication.

## License and support

License selection remains pending. This unsupported Preview makes no GA, support, compatibility, or release claim.

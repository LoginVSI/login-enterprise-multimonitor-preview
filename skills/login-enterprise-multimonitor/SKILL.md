---
name: login-enterprise-multimonitor
description: Adapt and validate compatible Login Enterprise C# workloads with the reusable Multi-Monitor Preview while preserving application behavior, scenario sequencing, and measurement integrity. Use for script-only, DLL-backed, Office, browser, persistent Start/Run, or full-scenario multi-monitor workload work in this repository.
---

# Login Enterprise Multi-Monitor Preview

Status: **PREVIEW / PARTIALLY RUNTIME-VALIDATED**. The generic library, staging, physical placement, state continuity, and canonical Prepare -> Open/Place -> Close lifecycle are runtime-proven in the recorded Login Enterprise 6.8.6 environment. Office Preview and Knowledge Worker adaptations are generated/build-tested/static-validated; partner-lab runtime validation is pending. Treat repository evidence and recorded status as authoritative.

## Workflow

1. Read `AGENTS.md`, `README.md`, the relevant documents, the complete source workload, the supplied Login Enterprise scripting reference, applicable examples, and proven POCs. Read `product-context.md` for product boundaries and `implementation-guidance.md` for the current implementation contract.
2. Run `scripts/Verify-ReferenceHashes.ps1 -Verify` and `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after a major pass. Never edit, rename, reformat, or delete immutable originals, proven POCs, or the authoritative scenario transcription. Create adaptations under `workloads/`.
3. Classify every supplied file as preparation, Start/open, Run, Close, or single-file lifecycle. Trace process handoff; splash versus main UI; durable title/class/process; whether the selected HWND survives; existing-instance ambiguity; dialogs, popups, and child windows that must not allocate; interactions; timer boundaries; placement insertion point; cadence; cleanup; and Start/Run relationships. Do not assume the initially spawned PID owns the durable visible UI.
4. Prefer documented Login Enterprise APIs, then compatible .NET/C#, then Win32 only when necessary. Never invent a Login Enterprise API. Use `IWindow.NativeWindowHandle` only after the workload has identified the correct current window.
5. Choose the pattern:
   - use `workloads/script-only/` to isolate placement/state behavior without assembly loading;
   - use `workloads/dll-backed/` for the canonical reflection-loaded reusable-assembly flow, and `workloads/dll-backed/regression/` only for the retained proven harness;
   - use `workloads/office-preview/` for small first-lab examples;
   - use `workloads/knowledge-worker-multimonitor/` for complete preserved-workload adaptations, and read its manifest before editing.
6. Insert allocating placement only after the durable/base application window exists and outside EUX/application-response/performance timers wherever practical. When the workload owns startup and needs the main window, prefer a specific documented `START`; otherwise resolve the intended window with documented `FindWindow`/`FindWindows`, then pass its `NativeWindowHandle`. Use compiler-proven `FindWindows` named arguments `className` and `processName`. Retain `ShellExecute` only where the lifecycle is understood and explicitly handled. Never allocate for splash, first-run/setup, open/save, Outlook compose/read/reminder, popup, child/secondary, or temporary launcher windows. Never move measurement boundaries silently. Log the structured result and overhead.
7. For Start/Run pairs, call `PlaceNext` exactly once in Start/open and use `PlaceLastUsed` or `PlaceOnMonitor` maintenance in Run. Reassert the same target after application actions that later restore, maximize, focus, replace, or reposition the durable window; do not consume another round-robin destination for maintenance or secondary windows.
8. Preserve `TARGET`, primary script class, original launch intent, application actions/interactions, URLs/content, timers and timer boundaries, EUX/application measurements, cadence, cleanup, `Run once`, and `Leave application running` intent. Change content only for an explicit public-safety reason and record the fidelity impact. For the canonical Application Test, set Prepare off/not relevant, Open/Place `Leave application running` on, and Close off. Continuous Test and Load Test also provide `Run once`; preserve deliberate one-time semantics. Close must use bounded explicit cleanup and must not allocate, reset, or alter placement state.
9. For DLL-backed work, use the unsupported Preview ScriptContent workflow in `implementation-guidance.md`: use the engine's local ScriptContent directory for Script Editor/Standalone Engine development and appliance ScriptContent for platform execution. Run the dedicated preparation workload, retain an existing local copy by default, and force refresh only in that preparation step. Consumers verify and load the target-local DLL; they do not repeatedly download it. Appliance delivery and all three Prepare paths are proven in the tested Desktop Connector Application Test.
10. Create or update a source-to-adaptation mapping/delta record. For every file state its lifecycle, allocation behavior, durable-window method, launch/window changes, content substitutions, timer impact, and runtime status. Keep protected originals immutable.
11. Run `scripts/Test-Repository.ps1`, then validate in the order in `validation-guidance.md`. Record durable HWND identity and secondary-window non-consumption. Script Editor proves an individual workload; an actual Login Enterprise test proves platform serial execution and cross-file persistence. Interactive movement requires a real multi-display desktop.
12. Report exactly what was preserved and changed, the automated results, and the human runtime test: files/order, scenario settings, expected monitor sequence/state, durable-window evidence, secondary-window checks, events/results, timing, and cleanup. Label generated/static-validated work **not runtime-proven** until that environment passes.

## Guardrails

- Keep launch, correct-window discovery, application sequencing, and timing in the workload; keep monitor discovery, ordering, state, placement, verification, and results in reusable code.
- Never persist HWND or monitor handles, change the configured Windows primary monitor, add third-party runtime dependencies, or require administrator rights.
- Never describe a meaningful public-safety substitution as exact content preservation; keep the preserved original immutable and disclose reduced fidelity.
- Treat Edge and other self-repositioning applications as lifecycle integrations, not one-time moves.
- Never use a lingering `ShellExecute` process as the application-persistence contract; scenario settings own persistence.
- A workload has one associated `TARGET`. In a combined generic harness, document which application owns it, use only evidence-backed alternatives for other launches, preflight away existing matching windows, and mark cross-workload ownership/cleanup unvalidated until a real scenario proves it.
- Do not use CMD as a deterministic generic harness where Windows Terminal hosts its visible UI; do not add CMD-specific product logic.
- Keep application readiness separate from placement stabilization. A workload-level `PrePlacementReadinessDelayMilliseconds` defaults to `0` and may be used only after the durable HWND is identified when empirical evidence justifies settling; never add a mandatory global wait.
- Do not invent Login Enterprise distribution behavior. Verify `UrnBaseForFiles`, `CopyFile`, `FileExists`, `RemoveFile`, and directory handling against supplied documentation/examples before changing staging.
- Do not claim zero-cost placement, runtime compatibility, cross-workload continuity, or VDI behavior without evidence.
- Use public-safe generic Preview language and run `scripts/Test-PublicSafety.ps1` before completion.

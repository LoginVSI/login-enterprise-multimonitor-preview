---
name: login-enterprise-multimonitor
description: Adapt and validate compatible Login Enterprise C# workloads with the reusable Multi-Monitor Preview while preserving application behavior, scenario sequencing, and measurement integrity. Use for script-only, DLL-backed, Office, browser, persistent Start/Run, or full-scenario multi-monitor workload work in this repository.
---

# Login Enterprise Multi-Monitor Preview

Status: **DRAFT / PARTIALLY VALIDATED**. Selected Login Enterprise 6.8.6 Script Editor/Standalone Engine paths were runtime-proven on August 18, 2026; platform orchestration and integrated workloads remain unvalidated. Treat repository evidence and recorded validation status as authoritative.

## Workflow

1. Read `AGENTS.md`, `README.md`, the relevant documents, the complete source workload, the supplied Login Enterprise scripting reference, applicable examples, and proven POCs. Read `product-context.md` for product boundaries and `implementation-guidance.md` for the current implementation contract.
2. Run `scripts/Verify-ReferenceHashes.ps1 -Verify` and `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after a major pass. Never edit, rename, reformat, or delete immutable originals, proven POCs, or the authoritative scenario transcription. Create adaptations under `workloads/`.
3. Trace the workload before editing: process lifecycle; splash versus main UI; durable title/class/process; whether the selected HWND survives for the workload/session; dialogs, popups, and child windows that must not allocate; interactions; timer boundaries; placement insertion point; cadence; cleanup; and Start/Run relationships. Do not assume the initially spawned PID owns the durable visible UI.
4. Prefer documented Login Enterprise APIs, then compatible .NET/C#, then Win32 only when necessary. Never invent a Login Enterprise API. Use `IWindow.NativeWindowHandle` only after the workload has identified the correct current window.
5. Choose the pattern:
   - use `workloads/script-only/` to isolate placement/state behavior without assembly loading;
   - use `workloads/dll-backed/` for the reflection-loaded reusable assembly;
   - use `workloads/integrated/` for derived complete workload adaptations.
6. Insert allocating placement only after the durable/base application window exists and outside EUX/application-response/performance timers wherever practical. When the workload owns startup and needs the main window, prefer a specific documented `START`; otherwise resolve the intended window with documented `FindWindow`/`FindWindows`, then pass its `NativeWindowHandle`. Use compiler-proven `FindWindows` named arguments `className` and `processName`. Retain `ShellExecute` only where the lifecycle is understood and explicitly handled. Never allocate for splash, first-run/setup, open/save, Outlook compose/read/reminder, popup, child/secondary, or temporary launcher windows. Never move measurement boundaries silently. Log the structured result and overhead.
7. For Start/Run pairs, allocate once in Start and use maintenance placement in Run. Reassert the same target after application actions that later restore, maximize, focus, replace, or reposition the durable window; do not consume another round-robin destination for maintenance or secondary windows.
8. Preserve the original behavior and scenario intent. Use `reference/test-scenario/workload-sequence.txt` for the complete representative set without changing ordering, enabled state, `Run once`, or `Leave application running` semantics.
9. For DLL-backed work, use the unsupported Preview ScriptContent workflow in `implementation-guidance.md`: use the engine's local ScriptContent directory for Script Editor/Standalone Engine development, use appliance ScriptContent only for a real platform test, run the dedicated preparation workload, retain an existing local copy by default, and force refresh only in that preparation step. Consumers verify and load the target-local DLL; they do not repeatedly download it.
10. Validate in the order in `validation-guidance.md`. Record durable HWND identity and secondary-window non-consumption. Script Editor proves only an individual workload; cross-file persistence requires an actual Login Enterprise scenario. Interactive movement requires a real multi-display desktop.
11. Record evidence precisely, update public documentation and limitations, rerun hash and public-safety checks, and label generated work as unvalidated until the relevant environment actually passes.

## Guardrails

- Keep launch, correct-window discovery, application sequencing, and timing in the workload; keep monitor discovery, ordering, state, placement, verification, and results in reusable code.
- Never persist HWND or monitor handles, change the configured Windows primary monitor, add third-party runtime dependencies, or require administrator rights.
- Treat Edge and other self-repositioning applications as lifecycle integrations, not one-time moves.
- Do not use CMD as a deterministic generic harness where Windows Terminal hosts its visible UI; do not add CMD-specific product logic.
- Keep application readiness separate from placement stabilization. A workload-level `PrePlacementReadinessDelayMilliseconds` defaults to `0` and may be used only after the durable HWND is identified when empirical evidence justifies settling; never add a mandatory global wait.
- Do not invent Login Enterprise distribution behavior. Verify `UrnBaseForFiles`, `CopyFile`, `FileExists`, `RemoveFile`, and directory handling against supplied documentation/examples before changing staging.
- Do not claim zero-cost placement, runtime compatibility, cross-workload continuity, or VDI behavior without evidence.
- Use public-safe generic Preview language and run `scripts/Test-PublicSafety.ps1` before completion.

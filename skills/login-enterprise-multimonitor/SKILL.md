---
name: login-enterprise-multimonitor
description: Adapt and validate compatible Login Enterprise C# workloads with the reusable Multi-Monitor Preview while preserving application behavior, scenario sequencing, and measurement integrity. Use for script-only, DLL-backed, Office, browser, persistent Start/Run, or full-scenario multi-monitor workload work in this repository.
---

# Login Enterprise Multi-Monitor Preview

Status: **DRAFT / UNVALIDATED**. The Preview API and implementation are not stable, and this skill has not been validated in Login Enterprise. Treat repository evidence and recorded validation status as authoritative.

## Workflow

1. Read `AGENTS.md`, `README.md`, the relevant documents, the complete source workload, the supplied Login Enterprise scripting reference, applicable examples, and proven POCs. Read `product-context.md` for product boundaries and `implementation-guidance.md` for the current implementation contract.
2. Run `scripts/Verify-ReferenceHashes.ps1 -Verify` and `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after a major pass. Never edit, rename, reformat, or delete immutable originals, proven POCs, or the authoritative scenario transcription. Create adaptations under `workloads/`.
3. Trace the workload before editing: launch path, process and window lifecycle, correct `IWindow`, interactions, timer boundaries, cadence, cleanup, and Start/Run relationships. Do not assume the first process window is durable.
4. Prefer documented Login Enterprise APIs, then compatible .NET/C#, then Win32 only when necessary. Never invent a Login Enterprise API. Use `IWindow.NativeWindowHandle` only after the workload has identified the correct current window.
5. Choose the pattern:
   - use `workloads/script-only/` to isolate placement/state behavior without assembly loading;
   - use `workloads/dll-backed/` for the reflection-loaded reusable assembly;
   - use `workloads/integrated/` for derived complete workload adaptations.
6. Insert allocating placement after the durable application window exists and outside EUX/application-response/performance timers wherever practical. Never move measurement boundaries silently. Log the structured result and overhead.
7. For Start/Run pairs, allocate once in Start and use maintenance placement in Run. Reassert the same target after application actions that later restore, maximize, focus, replace, or reposition the window; do not consume another round-robin destination for maintenance.
8. Preserve the original behavior and scenario intent. Use `reference/test-scenario/workload-sequence.txt` for the complete representative set without changing ordering, enabled state, `Run once`, or `Leave application running` semantics.
9. Validate in the order in `validation-guidance.md`. Script Editor proves only an individual workload; cross-file persistence requires an actual Login Enterprise scenario. Interactive movement requires a real multi-display desktop.
10. Record evidence precisely, update public documentation and limitations, rerun hash and public-safety checks, and label generated work as unvalidated until the relevant environment actually passes.

## Guardrails

- Keep launch, correct-window discovery, application sequencing, and timing in the workload; keep monitor discovery, ordering, state, placement, verification, and results in reusable code.
- Never persist HWND or monitor handles, change the configured Windows primary monitor, add third-party runtime dependencies, or require administrator rights.
- Treat Edge and other self-repositioning applications as lifecycle integrations, not one-time moves.
- Do not claim zero-cost placement, runtime compatibility, cross-workload continuity, or VDI behavior without evidence.
- Use public-safe generic Preview language and run `scripts/Test-PublicSafety.ps1` before completion.

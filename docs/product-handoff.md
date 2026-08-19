# Product and Development handoff

This technical handoff accompanies, but does not replace, the separately managed formal PRD. The implementation is an unsupported Preview.

## Implementation summary

The repository contains a generic `netstandard2.0` placement DLL, testable state/ordering logic, structured results, script-only and DLL-backed sequential proofs, and 10 derived representative scenario workloads. Generic code is independent of LoginPI.Engine and application type.

## Product problem and architecture

See `product-requirements-context.md` and `architecture.md`. The core authoring contract is: the workload finds the correct `IWindow` and chooses the safe insertion point; the helper receives its current native handle and performs one allocation or maintenance placement.

## Repository and distribution

The root README maps source and evidence. `build.ps1` produces `dist/LoginVSI.MultiMonitor.dll`. The unsupported Preview workflow uploads it to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`; `workloads/dll-backed/00-Prepare-MultiMonitor.cs` then uses `UrnBaseForFiles.UrnBase` plus documented file operations to stage it beneath `%TEMP%\LoginPI\MultiMonitor`. Consumers load only that target-local file.

## Validated behavior

- Preserved POCs: successful implementation evidence as described in their source.
- Current library build: passed locally with zero warnings/errors.
- Current unit harness: 20 tests passed, including source-contract checks for the canonical flow.
- Login Enterprise 6.8.6 Script Editor/Standalone Engine on August 18, 2026: prepare compile/run, local ScriptContent initial staging and forced refresh, runtime DLL loading, `className`/`processName` compiler casing, durable `START`/`MainWindow` for the simple Notepad and Edge proofs, two-physical-monitor Notepad/Paint/Edge placement, file-state continuation across separate standalone executions, and missing-state recovery passed.
- Login Enterprise 6.8.6 Desktop Connector Application Test: appliance ScriptContent delivery, all three Prepare paths, serial execution of the three independent DLL-backed workloads, platform state persistence, `Notepad -> 0`, `Paint -> 1`, `Edge -> 0`, final `MonitorCount=2` / `LastUsedIndex=0`, and all three AppExecutions passed.
- Canonical Prepare -> Open/Place -> Close source: implemented/generated, not runtime-proven. Representative durable-window behavior, complete scenario, and VDI: not yet validated.

## Scenario behavior

Use `reference/test-scenario/workload-sequence.txt` without changing order or settings. Manually configure the new run-once multi-monitor prepare workload before existing Office/M365 preparation for Preview testing; this future scenario edit is not preserved evidence. Preparation and close derivatives do not consume destinations. Edge Run reuses the Start target. Outlook, Excel, PowerPoint, and Word each allocate one target for their durable main/document window. Splash screens, first-run/file dialogs, Outlook message/reminder windows, popups, and child windows never allocate.

For the separate canonical generic Application Test, configure Prepare off/not relevant, Open/Place with `Leave application running` on, and Close off. Open/Place resets only because it represents a fresh deterministic demonstration. Close acts only on sole matching base windows and must leave the final placement state unchanged. Continuous Test/Load Test adaptations must preserve deliberate `Run once` semantics.

## Decisions

- Preserve the v0.3 state path/schema.
- Order primary first, then signed X/Y.
- Rediscover on every placement and never persist handles.
- Advance only after verification.
- Use `netstandard2.0`/C# 7.3 and no third-party dependencies.
- Use the supplied ScriptContent pattern only for this unsupported Preview staging workflow; do not represent it as a product updater.
- Retain existing target-local DLLs by default and require explicit remove-and-copy refresh.
- Allocate only after the workload has identified its durable/base HWND; application readiness and helper stabilization are separate delays.
- Keep placement outside known open-document timers.
- Reassert rather than reallocate after later maximize/focus behavior.

## Unresolved issues and technical debt

See `known-limitations.md`. Reflection boilerplate is intentionally duplicated in standalone workload files because Login Enterprise compiles them independently. A productized authoring surface should reduce this duplication. State v1 tracks count/index only and does not detect same-count monitor identity changes.

## PRD relationship and AI skill

The product-requirements context supplies implementation findings, not approved requirements. The repository AI skill documents the actual Preview workflow and runtime evidence but remains draft pending canonical-flow runtime validation, representative integration, and API stabilization.

## Recommended engineering follow-up

The next mini-project is a real Desktop Connector Application Test of the canonical source: Prepare off/not relevant, Open/Place `Leave application running` on, and Close off. Verify `0,1,0`, all three windows at handoff, explicit bounded cleanup, and unchanged final state after Close. Preserve deliberate `Run once` behavior in later Continuous Test/Load Test adaptations. Do not confuse the proven simple regression harness with this generated flow.

## Support and release considerations

Define supported Login Enterprise/Windows/runtime versions, deployment/signing, state lifecycle, diagnostics, failure policy, configuration, security review, upgrade/rollback, ownership, and release posture before productization. No support or delivery commitment is implied.

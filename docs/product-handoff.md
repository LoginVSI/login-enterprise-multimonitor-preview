# Product and Development handoff

This technical handoff accompanies, but does not replace, the separately managed formal PRD. The implementation is an unsupported Preview.

## Implementation summary

The repository contains a generic `netstandard2.0` placement DLL, testable state/ordering logic, structured results, script-only and DLL-backed sequential proofs, and 10 derived representative scenario workloads. Generic code is independent of LoginPI.Engine and application type.

## Product problem and architecture

See `product-requirements-context.md` and `architecture.md`. The core authoring contract is: the workload finds the correct `IWindow` and chooses the safe insertion point; the helper receives its current native handle and performs one allocation or maintenance placement.

## Repository and distribution

The root README maps source and evidence. `build.ps1` produces `dist/LoginVSI.MultiMonitor.dll`. Runtime examples expect the DLL beneath `%TEMP%\LoginPI\MultiMonitor` because no supported custom DLL distribution API was established.

## Validated behavior

- Preserved POCs: successful implementation evidence as described in their source.
- Current library build: passed locally with zero warnings/errors.
- Current unit harness: 17 tests passed.
- Script Editor, runtime DLL loading, actual window movement, separate-file continuity, complete scenario, and VDI: not yet validated.

## Scenario behavior

Use `reference/test-scenario/workload-sequence.txt` without changing order or settings. Preparation and close derivatives do not consume destinations. Edge Run reuses the Start target. Outlook, Excel, PowerPoint, and Word each allocate one target for their durable main/document window.

## Decisions

- Preserve the v0.3 state path/schema.
- Order primary first, then signed X/Y.
- Rediscover on every placement and never persist handles.
- Advance only after verification.
- Use `netstandard2.0`/C# 7.3 and no third-party dependencies.
- Keep automatic DLL delivery out until supported documentation establishes a contract.
- Keep placement outside known open-document timers.
- Reassert rather than reallocate after later maximize/focus behavior.

## Unresolved issues and technical debt

See `known-limitations.md`. Reflection boilerplate is intentionally duplicated in standalone workload files because Login Enterprise compiles them independently. A productized authoring surface should reduce this duplication. State v1 tracks count/index only and does not detect same-count monitor identity changes.

## PRD relationship and AI skill

The product-requirements context supplies implementation findings, not approved requirements. The repository AI skill documents the actual Preview workflow but remains draft until Login Enterprise validation and API stabilization.

## Recommended engineering follow-up

Follow `testing.md` in order: Script Editor compile/load, individual placement, topology/state cases, two independent files in a real scenario, integrated Office, Edge Start/Run, complete sequence, repeated runs, timing, and VDI.

## Support and release considerations

Define supported Login Enterprise/Windows/runtime versions, deployment/signing, state lifecycle, diagnostics, failure policy, configuration, security review, upgrade/rollback, ownership, and release posture before productization. No support or delivery commitment is implied.

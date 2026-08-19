# Product requirements context

This is a technical companion and working input, **not the formal PRD**. The formal PRD lives in the product-management system. Preview code and findings are not product commitments.

## Problem

Compatible Login Enterprise workloads commonly drive one primary application window on the primary display. Multi-monitor validation may need representative application activity distributed repeatably across the active display topology.

## User story and value

Provisional story: a workload author can opt a correctly identified application window into a reusable next-monitor operation without rewriting launch, application interaction, or measurement logic.

Potential value includes repeatable multi-display activity, more representative desktop behavior, reusable authoring, and a common evidence base. Scope and prioritization require Product Owner review.

## Current and desired behavior

The Preview rediscovers active monitors, orders the primary first, selects from file-backed state, restores/moves/maximizes, verifies, reports overhead, and advances state after success. The desired product direction is broad compatible-workload reuse rather than per-application placement code.

The Preview does not change the configured Windows primary monitor and does not persist window or monitor handles.

## Universal compatible-workload goal

The generic helper accepts only an HWND, application label, state path, maximize choice, and stabilization delay. Application-specific code continues to own the actual `IWindow` and insertion point. Compatibility boundaries remain evidence-driven and unapproved as formal requirements.

## Representative Knowledge Worker validation

Office and Edge derivatives exercise different patterns: replaced document windows, persistent Start/Run windows, later minimize/maximize/focus actions, timers, and cleanup. They validate the generic mechanism; they do not define application-specific core architecture.

## Candidate base requirements

- Primary-first deterministic ordering with signed coordinates.
- Persistent round-robin state across independent workload executions.
- Safe missing/invalid/count-changed state recovery.
- Verified-success-only state advancement.
- Structured results and placement time.
- Standard-user operation and no third-party runtime dependency.
- Clear workload/helper responsibility boundary.

These are implemented Preview behaviors, not yet accepted formal product requirements.

## Nice-to-haves

Potential future areas include richer monitor identity in state, configurable ordering/policies, diagnostics tooling, signed distribution, centralized routing, and a supported workload-author integration surface. None is committed.

## Workload-author experience

The current unsupported Preview pattern requires uploading one assembly to appliance ScriptContent, running a preparation workload to stage it locally, loading it, passing the already identified durable `IWindow.NativeWindowHandle`, logging `PlacementResult`, and choosing either allocation or maintenance placement. Existing local copies are retained by default and explicitly refreshed only in preparation. A productized experience should reduce reflection boilerplate and clearly describe supported window-lifecycle and update patterns.

## Runtime and persistent-state behavior

The MVP state path and schema remain POC-compatible. Calls serialize through a short file lock. Monitor-count changes reset the index; same-count identity/topology changes do not. `PlaceLastUsed` supports Start/Run continuity without consuming another target.

## Scenario/sequencing behavior

Platform continuity across independent files is proven for the simple regression Desktop Connector Application Test. The canonical Application Test must configure Prepare off/not relevant, Open/Place `Leave application running` on, and Close off. Close performs explicit bounded cleanup without changing placement state. Continuous Test and Load Test also expose `Run once`; preserve its intended semantics in those adaptations.

## Compatibility

The library targets `netstandard2.0`/C# 7.3. Loading and invocation are proven in Login Enterprise 6.8.6 Script Editor/Standalone Engine and Desktop Connector Application Test on the tested Windows machine. Compatibility remains unvalidated across other Login Enterprise releases, the generated canonical lifecycle, other Windows and application versions, DPI modes, display topologies beyond the tested physical pair, VDI platforms, and deployment policies.

## Measurement and timing expectations

Placement should remain outside existing application-response measurements wherever practical. It is explicitly nonzero-cost and reports elapsed milliseconds. Product thresholds and acceptable cadence impact are open.

## Error/failure behavior

The helper returns structured failures for invalid HWNDs, no monitors, state-lock timeout, invalid target, Win32 failure, and failed verification. Workload examples abort on unsuccessful placement to avoid silently claiming success. A product continuation/degradation policy remains open.

## Security and deployment

The helper runs as the standard user, writes beneath `%TEMP%`, loads a staged local assembly, and has no third-party dependency. The ScriptContent/remove-and-copy workflow is Preview-only. Signing, trusted staging, binary integrity, allowed paths, version detection, update/rollback, logging policy, and distribution ownership remain open product decisions.

## Observability

Results expose application name, monitor count, initial/target/verified indices, state advancement, Win32 error, message, and elapsed time. Future evidence should determine whether additional topology/state identifiers are necessary.

## Risks and open questions

See `known-limitations.md`. Key questions include runtime compatibility, correct-window durability, Edge replacement behavior, concurrency, same-count topology changes, mixed DPI, acceptable overhead, automatic deployment, and support ownership.

## Preview findings and validation evidence

POCs are preserved successful evidence. The August 18, 2026 standalone results prove individual preparation/loading, named-argument casing, simple placement, continued state, and missing-state recovery. The Desktop Connector Application Test additionally proves appliance delivery, all Prepare paths, real serial execution, and platform cross-workload state for the simple two-monitor regression harness. The canonical Prepare -> Open/Place -> Close source and integrated workloads remain generated/not validated. Use `testing.md` for current local checks and evidence statuses.

## Architecture alternatives

Current choices are embedded script-only logic for isolation and a single managed DLL for reuse. A future background router is only an alternative for evaluation.

## Productization considerations

Review API stability, framework/runtime support, signed packaging, state contract/versioning, concurrency, configuration, diagnostics, failure policy, authoring ergonomics, security, support, and migration before formalization.

## Product Owner and Development handoff

Product Owner review should establish the accepted problem, value, requirements, priorities, and release posture. Development handoff should use the source, API, build, scenario, evidence statuses, limitations, and manual validation ladder in this repository.

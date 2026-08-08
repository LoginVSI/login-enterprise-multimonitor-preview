# Architecture

Status: Planned. This document scaffolds decisions; it does not describe a validated implementation.

## Problem

Compatible Login Enterprise workloads commonly interact primarily with the primary display. Multi-monitor validation may require representative application activity distributed predictably across available displays.

## Goals and universal design intent

Create a reusable, application-neutral Preview mechanism applicable to broadly compatible Login Enterprise C# workloads. Representative Knowledge Worker flows validate the mechanism but do not define its architecture. Preserve workload behavior and measurement integrity.

## Metalanguage-first principle

Use documented Login Enterprise scripting/metalanguage first, compatible .NET/C# second, and native Windows/P/Invoke third. Confirm exact APIs in supplied documentation; do not invent them.

## Responsibility boundary

Application-specific workloads own launch, application interaction, correct main-window identification, sequencing, existing timer boundaries, and when to request placement. Reusable code owns monitor discovery, primary detection and primary-first ordering, persistent round-robin state, next-monitor selection, native placement, suitable restore/maximize behavior, verification, and result/error information.

## State

TBD after reference review: persistence location and format, initialization, atomicity, concurrency, corruption recovery, monitor-count changes, and standard-user access.

## Monitor enumeration and primary-first ordering

TBD after POC and documentation review: enumerate active displays, capture bounds/working areas and primary status, then define deterministic primary-first ordering for remaining displays. Cover negative coordinates and topology changes.

## Placement

TBD: native HWND placement, restore/maximize policy, working-area versus monitor bounds, verification, retry policy, focus effects, and structured results.

## Reuse alternatives

### Helper-copy architecture

Script-contained compatible helper code can isolate behavior before deployment and runtime-loading complexity. Duplication and drift are open concerns.

### DLL architecture

A managed helper may centralize the application-neutral mechanism. Its API, target runtime, deployment, Script Editor compatibility, and versioning remain TBD.

### Dynamic loading

Loading a helper dynamically may reduce compile-time coupling, but discovery, compatibility, error handling, and security require validation.

### Possible background session router

A future background window router is an architecture alternative, not a requirement or commitment. Ownership, lifecycle, matching, security, timing, and deployment implications remain open.

## Scenario sequencing

State and application behavior span independent workload files. The preserved scenario reference informs integration, but experiments must not alter it silently. Full behavior requires an actual sequential Login Enterprise scenario.

## Measurement boundary

Placement adds overhead. Keep it outside EUX, application-response, and performance timers wherever practical, without silently moving boundaries or altering cadence.

## Deployment and compatibility

Target standard-user execution without third-party runtime dependencies. Compatibility across Login Enterprise runtime versions, Windows versions, DPI modes, display topologies, VDI platforms, and helper deployment remains to be established.

## Open questions

- Which supplied Login Enterprise APIs can perform each step?
- What state contract is safest across independent executions?
- Which reuse architecture best fits Script Editor and deployed workloads?
- How should placement, verification, retries, focus, restore, and maximize interact?
- What compatibility and distribution envelope can evidence support?

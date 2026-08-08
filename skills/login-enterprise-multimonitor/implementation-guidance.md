# Implementation guidance

Status: **TBD / UNVALIDATED**. Do not infer a stable API or final design from these placeholders. Complete them only after reading all supplied documentation, original workloads, representative examples, and proven POCs.

## Metalanguage-first principle

TBD: map required operations to documented Login Enterprise functions first, compatible .NET/C# second, and native Windows/P/Invoke third.

## Architecture

TBD: preserve the workload-specific/reusable responsibility boundary and application neutrality.

## State

TBD: persistence scope, location, schema, atomic writes, concurrency, recovery, reset, and monitor-count change.

## Monitor enumeration

TBD: supported discovery method, metadata, active-display filtering, bounds, work areas, DPI, and failures.

## Primary-first ordering

TBD: deterministic primary-first algorithm and ordering of remaining displays.

## Coordinates

TBD: coordinate systems, working areas, scaling, and placement calculations.

## Negative coordinates

TBD: signed bounds for displays left of or above the primary.

## Native placement

TBD: HWND bridge, restore/move/maximize policy, verification, retries, focus, and results.

## DLL/helper API

TBD: public contract, target runtime, types, versioning, dependencies, and deployment.

## Dynamic loading

TBD: assembly discovery, path trust, reflection contract, compatibility, and graceful failure.

## Error handling

TBD: structured results, logging, recovery, continuation policy, and public-safe diagnostics.

## Office

TBD: application-specific window identification and sequencing only; do not move generic behavior into Office-specific code.

## Browser/Edge

TBD: existing instances, multiprocess/window identity, launch behavior, and replacement windows.

## Persistent Start/Run workloads

TBD: ownership and state across independent workload files and long-lived application windows.

## Scenario sequencing

TBD: integrate without silently changing the known-good reference ordering or settings.

## Possible future background window router

TBD architecture alternative only; not an approved requirement, implementation, or delivery commitment.

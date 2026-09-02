# Product and Development handoff

This public technical handoff is implementation evidence for product discussion. It is not a formal PRD, support statement, roadmap, or delivery commitment. The current repository remains an unsupported Multi-Monitor Preview.

## Customer problem and validated use case

Login Enterprise workloads in multi-display sessions often exercise durable application windows on the primary display. A workload author needs a repeatable way to distribute independent applications across active monitors without rewriting application interactions or measurement logic.

The validated representative use case is a sequential Knowledge Worker flow in which Classic Outlook, Edge, Excel, PowerPoint, and Word consume primary-first round-robin destinations. A two-monitor external partner-lab Application Test passed 7/7 actions with zero failures, and three-monitor Office placement demonstrated indices `0,1,2`. These results are bounded to the recorded flows. [Evidence status](evidence-status.md) is the source of truth.

## Functional behavior implemented by the Preview

- Discover active monitors for each placement and identify the Windows primary explicitly.
- Order the primary first, then other displays deterministically by signed coordinates and stable tie-breakers.
- Select the next index from persistent state and advance only after destination verification succeeds.
- Restore, move, optionally maximize, verify, and return structured result/error information.
- Reapply the last or specified monitor without advancing allocation state.
- Recover safely from missing, malformed, out-of-range, or monitor-count-changed state.
- Serialize state access with a short local lock and never persist HWND or monitor handles.

The reflection-friendly static API is:

- `ResetState(string stateFilePath)`
- `PlaceNext(IntPtr windowHandle, string applicationName, string stateFilePath, bool maximize, int stabilizationDelayMilliseconds)`
- `PlaceLastUsed(...)`
- `PlaceOnMonitor(..., int targetMonitorIndex, ...)`

`PlacementResult` reports success, application, monitor count, initial/target/verified indices, elapsed time, state advancement, Win32 error, and message.

## Allocation and lifecycle semantics

One independently exercised application consumes one destination after its durable/base top-level window is known. Splash screens, first-run/setup UI, open/save dialogs, popups, child windows, temporary launchers, and Classic Outlook compose/read/reminder windows do not allocate.

| Lifecycle | Responsibility |
| --- | --- |
| Prepare | Stage prerequisites only; no allocation or reset |
| Start/Open | Resolve the durable/base window and call `PlaceNext` exactly once |
| Run | Reacquire that base window and use `PlaceLastUsed` or `PlaceOnMonitor` only when maintenance is justified |
| Close | Perform bounded cleanup; do not allocate, reset, or alter placement state |
| Single-file | Allocate the durable/base window once, outside measured boundaries where practical, then retain original behavior and cleanup |

Scenario settings are part of this contract. Start/Open must remain alive when a later Run/Close needs the application. `Run once` intent must be explicit for Continuous and Load Tests.

## Responsibility boundary

Application-specific workload code owns:

- launch and process handoff;
- profile, activation, first-run, content, and environment readiness;
- correct durable/base window identification and ambiguity handling;
- business interactions, sequencing, timers/EUX boundaries, cadence, and cleanup;
- when an application needs non-allocating maintenance after self-repositioning or replacement.

The generic placement component owns monitor discovery/order, persistent state, target selection, native movement/state handling, verification, locking, and structured diagnostics.

This boundary explains the Classic Outlook evidence. The stripped-down Office example assumes a configured environment. The real Knowledge Worker adaptation retains PRF/PST staging, `/importprf`, relevant first-run handling, Inbox/message/compose activity, one durable Inbox allocation, and later same-monitor maintenance. Its partner-lab pass is stronger evidence for that adapted scenario, not proof that generic placement should own Outlook profile setup.

## Preview deployment and state

The Preview uses existing Login Enterprise ScriptContent as a convenient delivery surface. The DLL is uploaded to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`; Prepare copies it to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`. Existing target-local copies are retained by default, and an explicit Prepare toggle performs a forced refresh.

State is stored at `%TEMP%\LoginPI\MultiMonitor\state.txt` as `MonitorCount` and `LastUsedIndex`. This staging, reflection loading, `%TEMP%` state, and customer-authored boilerplate are Preview scaffolding, not a recommendation for the final product distribution or authoring experience.

## Evidence summary

- Build/static: repository build, unit/pure-logic tests, contracts, links, protected hashes, public safety, DLL framework/dependency/checksum.
- Local runtime: Login Enterprise 6.8.6 DLL loading/staging, two-monitor state and placement, canonical Prepare/Open/Close, Word/Excel/PowerPoint, corrected Edge, and New Outlook launch/find/place.
- External partner runtime: two-monitor representative KW Application Test passed 7/7; Classic Outlook and Edge Start/Run passed in that real flow; Office placement demonstrated monitor indices `0,1,2` in a three-monitor environment.
- Pending: monitored multi-loop Load/Continuous resilience, broader compatibility, and an external latency/session-metrics comparison. An observed EUX reporting interruption has no established connection to placement.

Automated validation never substitutes for Script Editor execution or an actual Login Enterprise scenario.

## Productization boundary

Preview-only or replaceable scaffolding includes manual ScriptContent upload, a customer-run Prepare workload, reflection boilerplate in every consumer, an unversioned `%TEMP%` state file, workload-managed logging text, and repository-specific examples/contracts.

A formal implementation would likely need:

- a supported placement abstraction available to workload authors without reflection boilerplate;
- owned distribution, update, rollback, version compatibility, and signing;
- integrated lifecycle hooks or documented Start/Run/Close semantics;
- stable diagnostics, telemetry, correlation, and support-safe log handling;
- defined state/concurrency behavior for sessions, users, loops, and parallel workloads;
- a compatibility and support policy across Login Enterprise, Windows, applications, Connectors/VDI protocols, display topology, and DPI modes;
- authoring guidance and migration from Preview workloads.

## Placement idempotency consideration

Current Preview calls intentionally perform the normal restore/move/maximize/verify flow. A future product implementation could detect that the durable window is already on the selected target and already in the requested state, then avoid redundant window-state operations. It would still verify the destination, treat the application as having consumed that allocation, advance round-robin state for a successful allocating call, and return useful diagnostics. This is an optimization question, not a current Preview correctness defect.

## Unresolved engineering and product questions

- What supported workload-facing API or placement abstraction should exist?
- Which component owns distribution, signing, versioning, refresh, rollback, and compatibility negotiation?
- How should lifecycle integration expose allocation versus maintenance and enforce Close neutrality?
- What diagnostics and telemetry are support-safe, and how are placement results correlated with workload/EUX events?
- What are the state scope, lock, recovery, concurrency, and multi-user semantics?
- How should same-count topology changes, hot-plug events, mixed DPI, negative coordinates, and maximize behavior be handled?
- Which Login Enterprise releases, Windows/application versions, VDI/Connector technologies, and monitor counts are supported?
- Should idempotent placement skip redundant window operations while still consuming allocation state?
- What are the expected overhead, failure, retry, degradation, cleanup, and support policies?
- Which scenarios and evidence gates are required before supported release consideration?

License approval also remains open. Public readability currently grants no open-source license or reuse rights.

## Repository consumption

Product and Development reviewers should start with [architecture](architecture.md), [evidence status](evidence-status.md), [testing](testing.md), [known limitations](known-limitations.md), and the [Knowledge Worker mapping](../workloads/knowledge-worker-multimonitor/adaptation-manifest.json). The [manual](adapt-your-own-workload.md) and [AI-assisted](agentic-workload-adaptation.md) guides show the current workload-author experience and the amount of productization work that could be removed from customers.

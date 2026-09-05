# Evidence status

This page is the source of truth for Multi-Monitor Preview validation claims. Other pages should summarize and link here instead of maintaining separate detailed status tables.

Last reconciled: September 4, 2026.

The repository is an unsupported engineering Preview. A result applies only to the named environment and flow. Static validation does not establish Login Enterprise runtime behavior, and one successful application or topology does not establish broad compatibility.

## Evidence matrix

| Area | Status | Recorded evidence and boundary |
| --- | --- | --- |
| Repository build, unit tests, contracts, protected hashes, public safety, DLL contract | Build/static validated | The repository validation command covers these checks. It does not simulate Login Enterprise or a desktop session. |
| Generic DLL loading and target-local staging | Local runtime proven | Login Enterprise 6.8.6 Script Editor/Standalone Engine and the recorded Desktop Connector Application Test loaded the generic DLL. Missing, retain-existing, and forced-refresh Prepare paths passed. |
| Monitor discovery, two-monitor placement, and round-robin state | Local runtime proven | Physical placement and state advancement passed, including the generic `Notepad 0`, `Paint 1`, `Edge 0` sequence. |
| Workload foreground focus after placement | Runtime validated in Script Editor, tester-reported | Calling Login Enterprise `IWindow.Focus()` on the resolved application window after successful placement brought it to the foreground. This validates the focus pattern in the reported Script Editor environment, not the complete updated visual demo or cross-workload behavior. Focus remains optional workload behavior, separate from DLL placement success. |
| Canonical Prepare -> Open/Place -> Close | Local runtime proven | The recorded Login Enterprise 6.8.6 Desktop Connector Console / NoRemote Application Test passed serial execution, cross-workload state, scenario-controlled application handoff, and bounded cleanup. |
| Office Preview Word, Excel, PowerPoint | Local runtime proven | Each launch/find/place flow passed on one Login Enterprise 6.8.6 machine. Broader Office, Windows, locale, and session coverage is not implied. |
| Office Preview Edge | Local runtime proven | The corrected `START(processName: "msedge")` -> `MainWindow` flow passed locally. The earlier transient-PID failure occurred before placement and is application-lifecycle evidence, not a placement-library failure. |
| Office Preview New Outlook | Local runtime proven for launch/find/place | `TARGET:olk` -> `START()` -> `MainWindow` -> `PlaceNext` passed on one two-monitor 6.8.6 machine. New Outlook mail/calendar interaction automation was not proven. |
| Office Preview Classic Outlook | Not proven locally | The local environment did not initially have the same configured Classic Outlook environment as the partner lab. This result is not a generic placement failure. |
| Representative adapted Knowledge Worker flow, two monitors | External partner-lab Application Test passed | One-user Application Test completed 7/7 actions with zero failures: Prepare MultiMonitor, adapted Classic Outlook, Edge Start, Edge Run, Excel, PowerPoint, and Word. Observed allocating placement was Outlook `0`, Edge `1`, Excel `0`, PowerPoint `1`, Word `0`. Edge Run maintained the Edge allocation without consuming another monitor. |
| Representative adapted Classic Outlook | External partner-lab runtime proven for this flow | The real adapted workload completed its profile/data preparation, `/importprf` launch, relevant first-run handling, Inbox/message/compose activity, one durable Inbox allocation, and later non-allocating reassertion. This does not prove every Classic Outlook configuration. |
| Representative adapted Edge Start/Run and demo media | External partner-lab runtime proven for this flow | Edge passed after target profile and Edge first-run preparation. The locally staged Big Buck Bunny demo played. The media file is optional test content, not part of the placement mechanism. |
| Three-monitor Office placement | External partner-lab runtime demonstrated | The updated DLL was tested in two-monitor and three-monitor partner environments. Word placed on index `0`, Excel on `1`, and PowerPoint on `2`. This is application-placement evidence, not a complete three-monitor KW scenario or broad topology qualification. |
| Multi-loop Continuous/Load resilience | In progress, not proven | A two-monitor, one-user Load Test with Session Metrics started and initially followed the expected placement pattern. A monitored rerun is intended to confirm stable behavior across at least two complete loops. Do not call repeated-loop resilience proven yet. |
| nVector latency/session-metrics comparison | External partner validation in progress | Comparison with one-monitor testing is outside the core Preview requirement. No conclusion is recorded here. |
| EUX metrics-reporting interruption | Observed, attribution unknown | The workload remained active during a reporting interruption. There is no current evidence connecting that interruption to multi-monitor placement. Do not attribute it to the Preview without evidence. |
| Other Login Enterprise, Office, and Windows versions; VDI protocols; mixed DPI; topology changes; broader concurrency | Untested or partially logic-tested | Only the specific evidence above is claimed. See [known limitations](known-limitations.md). |

## Why the two Classic Outlook examples have different evidence

The [simple Office Preview Classic Outlook workload](../workloads/office-preview/40-Place-Microsoft-Outlook.cs) is intentionally small. It assumes Classic Outlook is already installed, configured, and ready; rejects ambiguous existing windows; starts Outlook; resolves its durable Explorer/MainWindow; and places that window. It does not reproduce profile, PRF, PST, activation, or first-run preparation from a full business workload.

The [representative Knowledge Worker Classic Outlook adaptation](../workloads/knowledge-worker-multimonitor/%28KW%29%20Microsoft%20Outlook.cs) keeps the source workload's application lifecycle. It stages its PRF/PST through the existing Login Enterprise behavior, rewrites the PRF TEMP path, invokes Outlook through the original `/importprf` flow, handles relevant first-run and activation dialogs, performs Inbox/message/compose interactions, allocates the durable Inbox base window once, and reasserts that same monitor after later window-state changes without allocating again.

Application and profile readiness belong to the application-specific workload lifecycle. Monitor discovery, state, target selection, movement, verification, and result reporting belong to the reusable helper. The partner-lab Knowledge Worker pass is therefore stronger evidence for that adapted Classic Outlook scenario than the stripped-down Office smoke example. It does not turn the simple example into a profile bootstrap workload or establish universal Outlook compatibility.

## Demo media boundary

Big Buck Bunny is optional demo content used by the representative Edge workload. It is not required by, distributed with, or supported as a feature of the Multi-Monitor Preview.

The public workload expects `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`. A tester may either change that workload configuration to an already staged local file, such as a partner image's `C:\temp\Big Buck Bunny Demo.mp4`, or copy the file into the public Preview path during environment preparation. The partner-specific path is deliberately not a generic default.

## Recording new evidence

Record the Login Enterprise version, Connector/session, Windows and application versions, display topology, scenario type and lifecycle settings, workload order, structured placement fields, application result, and loop count. Keep raw Engine logs private. Update this page only after the result has been reviewed, then shorten or link any affected summaries elsewhere.

# Representative Knowledge Worker Multi-Monitor adaptations

This directory contains one public adaptation for each of the ten immutable supplied files in [`reference/original-workloads/`](../../reference/original-workloads/). Those ten files—not every current or future vendor Knowledge Worker template—are the source of truth for **this** adaptation. The supplied docs/reference also contains newer three-phase KW25 examples for research; this directory is not automatically synchronized to them.

Status: **build/static validated and partner-lab runtime passed for the recorded two-monitor single-user Application Test**. The test completed 7/7 actions with zero failures: Prepare, Classic Outlook, Edge Start, Edge Run, Excel, PowerPoint, and Word. Observed allocating placement was Outlook `0`, Edge `1`, Excel `0`, PowerPoint `1`, and Word `0`. Multi-loop Load/Continuous resilience remains in progress. See [`docs/evidence-status.md`](../../docs/evidence-status.md).

## Import and order

Upload/stage the Preview DLL with [`00-Prepare-MultiMonitor.cs`](../dll-backed/00-Prepare-MultiMonitor.cs), then import these adapted files using the order and enabled/`Run once`/`Leave application running` intent in the immutable [`workload-sequence.txt`](../../reference/test-scenario/workload-sequence.txt). Do not edit that transcription.

## Allocation lifecycle

| File/lifecycle | Placement behavior |
| --- | --- |
| Office preparation | Neutral; no state or placement |
| Microsoft Outlook (Classic) | Allocates its original durable Inbox Explorer once; compose/read/reminder windows do not allocate |
| Edge Start | Distinguishes a newly opened durable Edge window and allocates once |
| Edge Run | `PlaceLastUsed`/`PlaceOnMonitor` maintenance only; never allocates |
| Excel, PowerPoint, Word | Allocate the durable document window once after the original open timer; later maximize/reposition points reassert without allocation |
| Excel, PowerPoint, Word Close | Neutral cleanup; no state or placement |

All consumers load the staged real DLL. No HWND or monitor handle crosses workload files. Placement remains outside original response timers wherever practical. The manifest and contracts protect `TARGET`, primary class, ordered timer calls, allocation count, neutral files, and reviewed line-delta budgets.

The preserved Outlook source and this adaptation target **Classic Outlook** (`outlook.exe` / `OUTLOOK` / `rctrl_renwnd32`) and its Classic-specific controls. They are not a New Outlook (`olk`) workload. Converting those interactions to New Outlook would be a separate substantive adaptation requiring New Outlook-specific control/navigation evidence and runtime validation; changing only the target is insufficient.

The simple Office Preview Classic Outlook workload assumes an installed, clean, configured environment and only preflights, launches, resolves, and places the durable Explorer/MainWindow. This full adaptation keeps the existing PRF/PST staging, PRF TEMP rewrite, `/importprf` launch, relevant first-run/activation handling, Inbox/message/compose interactions, one durable Inbox allocation, and later same-monitor reassertion. Its partner-lab pass is evidence for this adapted lifecycle. Application/profile readiness remains workload-owned; monitor discovery/state/placement remains helper-owned.

## Deliberate minimal deltas and public-safety substitutions

[`adaptation-manifest.json`](adaptation-manifest.json) is the machine-checked mapping and delta record. The application interactions, timers, URLs/content, and lifecycle remain as close as practical to the originals, except for disclosed Preview integration and public-safety changes:

- historical corporate example recipients in the preserved Outlook evidence are replaced by reserved `example.invalid` recipients; the preserved addresses are not credentials and must not be used as active test recipients;
- one customer-oriented Edge target is intentionally replaced with `about:blank`, reducing exact content fidelity for that tab; exact original/internal content behavior requires appropriately controlled validation;
- a machine-specific Edge media path becomes the configurable generic path `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`; stage the media there or change the workload to an already staged local file. A partner template's `C:\temp` copy is a valid lab choice, not the public default.

Big Buck Bunny is optional demo/test content for this Edge workload. It is not required by the multi-monitor mechanism and is not a supported Preview feature.

The preserved originals remain verbatim and hash-protected. No meaningful substitution should be described as an exact-content match.

Before broadening runtime status or claiming repeated-loop resilience, validate every durable window, timer boundary, secondary-window non-allocation, Start/Run handoff, media prerequisite, cleanup action, state result, original application interaction, and complete loop count in the named environment.

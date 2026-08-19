# Representative Knowledge Worker Multi-Monitor adaptations

This directory contains one public adaptation for each of the ten immutable supplied files in [`reference/original-workloads/`](../../reference/original-workloads/). Those ten files—not every current or future vendor Knowledge Worker template—are the source of truth for **this** adaptation. The supplied docs/reference also contains newer three-phase KW25 examples for research; this directory is not automatically synchronized to them.

Status: **generated/build-tested/static-validated; partner-lab runtime validation pending**.

## Import and order

Upload/stage the Preview DLL with [`00-Prepare-MultiMonitor.cs`](../dll-backed/00-Prepare-MultiMonitor.cs), then import these adapted files using the order and enabled/`Run once`/`Leave application running` intent in the immutable [`workload-sequence.txt`](../../reference/test-scenario/workload-sequence.txt). Do not edit that transcription.

## Allocation lifecycle

| File/lifecycle | Placement behavior |
| --- | --- |
| Office preparation | Neutral; no state or placement |
| Outlook | Allocates its original durable Inbox Explorer once; compose/read/reminder windows do not allocate |
| Edge Start | Distinguishes a newly opened durable Edge window and allocates once |
| Edge Run | `PlaceLastUsed`/`PlaceOnMonitor` maintenance only; never allocates |
| Excel, PowerPoint, Word | Allocate the durable document window once after the original open timer; later maximize/reposition points reassert without allocation |
| Excel, PowerPoint, Word Close | Neutral cleanup; no state or placement |

All consumers load the staged real DLL. No HWND or monitor handle crosses workload files. Placement remains outside original response timers wherever practical. The manifest and contracts protect `TARGET`, primary class, ordered timer calls, allocation count, neutral files, and reviewed line-delta budgets.

## Deliberate minimal deltas and public-safety substitutions

[`adaptation-manifest.json`](adaptation-manifest.json) is the machine-checked mapping and delta record. The application interactions, timers, URLs/content, and lifecycle remain as close as practical to the originals, except for disclosed Preview integration and public-safety changes:

- historical corporate example recipients in the preserved Outlook evidence are replaced by reserved `example.invalid` recipients; the preserved addresses are not credentials and must not be used as active test recipients;
- one customer-oriented Edge target is intentionally replaced with `about:blank`, reducing exact content fidelity for that tab; exact original/internal content behavior requires appropriately controlled validation;
- a machine-specific Edge media path becomes a generic path under `%TEMP%\LoginPI\MultiMonitor`; stage the media file before the test.

The preserved originals remain verbatim and hash-protected. No meaningful substitution should be described as an exact-content match.

Before changing runtime status, validate every durable window, timer boundary, secondary-window non-allocation, Start/Run handoff, media prerequisite, cleanup action, state result, and original application interaction in the partner lab.

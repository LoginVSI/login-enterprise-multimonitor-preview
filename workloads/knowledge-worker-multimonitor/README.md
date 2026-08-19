# Knowledge Worker multi-monitor workloads

These are the complete public Preview adaptations of the preserved representative scenario workload set. Immutable originals remain under `reference/original-workloads/`. Status: **generated/build-tested/static-validated; partner-lab runtime validation pending**.

`adaptation-manifest.json` maps every adapted file to its preserved source, type, allocation behavior, durable-window method, intentional change, and line-delta budget. `scripts/Test-WorkloadContracts.ps1` verifies complete coverage, identical `TARGET` and primary script class, preserved timer names, bounded line deltas, one allocating call where required, and non-allocating Run/Close behavior.

## Durable-window placement boundaries

| Workload | Destination-consuming window | Windows that do not consume |
| --- | --- | --- |
| Prepare for Microsoft Office 365 | None | Temporary Word preparation UI and first-run dialogs |
| Microsoft Outlook | `MainWindow` from `START` matching `Inbox*`, `rctrl_renwnd32`, and `OUTLOOK` | Sign-in/first-run dialogs, reminders, opened-message windows, compose/new-message windows |
| Edge Start-4KVideoHeavy | Newly observed top-level `Chrome_WidgetWin_1` window for `msedge`, after the preserved initialization wait | Pre-existing Edge windows, tabs, popups, and transient launch UI |
| Edge Run-4KVideoHeavy | Expected persistent Edge base window matching class/title/process; `PlaceLastUsed` reuses Start's destination | Tabs, popups, and interaction UI; Run does not allocate a new destination |
| Microsoft Excel | `_activeDocument`, the `loginvsi*` `XLMAIN` workbook found after `Open_Excel_Document` stops | Initial/old workbook, open/save and confirmation dialogs, first-run/recovery UI |
| Microsoft PowerPoint | `newPowerpoint`, the `loginvsi*` `PPTFrameClass` presentation found after `Open_Powerpoint_Document` stops | Initial/old presentation, open/save dialogs, first-run UI, slideshow/interaction windows |
| Microsoft Word | `newWord`, the `loginvsi*` `OpusApp` document found after `Open_Word_Document` stops | Initial/old document, open/save dialogs, first-run and confirmation UI |
| Close Excel/Word/PowerPoint | None | The matched base window is closed only; no placement call is made |

The adaptation keeps the preserved application selectors and interactions. Office minimize/maximize actions reassert the same base-window target without advancing. Edge Start allocates once; Edge Run uses maintenance placement. No general readiness wait was added; Edge Start retains its original configurable initialization wait after window discovery. If runtime evidence shows that an already-correct durable window needs settling, add an application-specific default-zero delay and record the justification separately from the helper's 350 ms placement stabilization.

The Edge Start adaptation replaces its fixed local video path with `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`; stage that generic media file if the 4K path is required. It retains the original cadence and tab count.

Actual Login Enterprise 6.8.6 compiler evidence requires `FindWindows(className: ..., processName: ...)`; lowercase `classname` and `processname` are rejected. Edge Start snapshots existing top-level handles and uses short searches to identify a new base window. Its failure path is approximately bounded by `waitTimeoutInSecondsMsedgeLaunch`, plus the snapshot/final-search margin, and still requires partner-lab timing validation.

Run files according to `reference/test-scenario/workload-sequence.txt`. Do not reorder or reinterpret enabled, `Run once`, or `Leave application running` settings. Before runtime placement, record title/class/process/HWND; confirm it is the durable non-splash base UI; confirm secondary windows never advance state; and record any application readiness delay. Edge Run matching and same-HWND continuity need explicit runtime evidence.

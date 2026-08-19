# Integrated representative workloads

These are derived Preview adaptations of the enabled scenario workloads. Immutable originals remain under `reference/original-workloads/`.

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

This review found no repository evidence that proves a safer supported selector than the current application-derived choices, so no executable integrated window-selection behavior changed. Office minimize/maximize actions reassert the same base-window target without advancing. Edge Start allocates once; Edge Run uses maintenance placement. No new general readiness wait was added; Edge Start retains its existing configurable initialization wait after window discovery. If runtime evidence shows that an already-correct durable window needs additional settling, add an application-specific `int PrePlacementReadinessDelayMilliseconds = 0;`, wait only when positive, and record the justification. This is distinct from the existing 350 ms helper placement stabilization delay.

The Edge Start adaptation replaces its fixed local video path with `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`; stage that generic media file if the 4K path is required. The workload retains its original cadence and tab count.

## `FindWindows` argument casing

Actual Login Enterprise 6.8.6 Script Editor compiler evidence resolves the supplied reference inconsistency. `FindWindows` accepts `className` and `processName`; it rejects lowercase `classname` and `processname`. All non-preserved repository call sites use the compiler-proven casing. Integrated Edge behavior itself remains unvalidated.

The integrated Edge Start discovery loop measures elapsed time and uses short individual searches so its failure path remains approximately bounded by `waitTimeoutInSecondsMsedgeLaunch`. The initial pre-launch snapshot plus the final individual search or wait may add a small amount beyond that configured bound; this requires Script Editor timing validation.

Run files according to `reference/test-scenario/workload-sequence.txt`. Do not reorder or reinterpret enabled, `Run once`, or `Leave application running` settings. Before runtime placement, record title/class/process/HWND; confirm it is the durable non-splash base UI and remains appropriate during later actions where expected; confirm secondary windows never advance state; and record any readiness delay. Edge Run's matching and same-HWND continuity need explicit runtime evidence. Status: generated/not validated in Script Editor or a full Login Enterprise scenario.

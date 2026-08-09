# Integrated representative workloads

These are derived Preview adaptations of the enabled scenario workloads. Immutable originals remain under `reference/original-workloads/`.

## Placement boundaries

- **Prepare for Microsoft Office 365:** no placement; its run-once Word window is transient and must not consume a destination.
- **Microsoft Outlook:** calls `START`, maximizes the main window, then allocates before subsequent inbox and message interaction; it reasserts the same target after the later minimize/maximize sequence.
- **Edge Start-4KVideoHeavy:** distinguishes a newly created Edge HWND, stops `Browser_Start`, waits for original initialization, then allocates a destination outside the measurement.
- **Edge Run-4KVideoHeavy:** uses the last successful target from Start without advancing state and reasserts it after later focus/maximize operations.
- **Microsoft Excel:** allocates the actual opened document immediately after `Open_Excel_Document` stops; reasserts after later minimize/maximize.
- **Microsoft PowerPoint:** allocates the actual opened presentation immediately after `Open_Powerpoint_Document` stops; reasserts after later minimize/maximize.
- **Microsoft Word:** allocates the actual opened document immediately after `Open_Word_Document` stops; reasserts after later minimize/maximize.
- **Close Excel/Word/PowerPoint:** cleanup only; no placement and no state consumption.

The Edge Start adaptation replaces its fixed local video path with `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`; stage that generic media file if the 4K path is required. The workload retains its original cadence and tab count.

## Monday Script Editor validation: `FindWindows` argument casing

The supplied scripting reference is internally inconsistent: its `FindWindows` syntax example uses lowercase `classname` and `processname`, while the adjacent parameter table uses `className` and `processName`. No preserved known-good workload or proven POC calls `FindWindows`, and C# named arguments are case-sensitive. The generated Edge call sites remain unchanged and follow the syntax example; that casing is not proven. Compile the script-only phase 2, DLL-backed phase 2, and integrated Edge Start workloads in Script Editor before treating these call sites as valid, then record the actual signature evidence before changing them.

The integrated Edge Start discovery loop measures elapsed time and uses short individual searches so its failure path remains approximately bounded by `waitTimeoutInSecondsMsedgeLaunch`. The initial pre-launch snapshot plus the final individual search or wait may add a small amount beyond that configured bound; this requires Script Editor timing validation.

Run files according to `reference/test-scenario/workload-sequence.txt`. Do not reorder or reinterpret enabled, `Run once`, or `Leave application running` settings. Status: generated/not validated in Script Editor or a full Login Enterprise scenario.

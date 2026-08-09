# Integrated representative workloads

These are derived Preview adaptations of the enabled scenario workloads. Immutable originals remain under `reference/original-workloads/`.

## Placement boundaries

- **Prepare for Microsoft Office 365:** no placement; its run-once Word window is transient and must not consume a destination.
- **Microsoft Outlook:** allocates after `START` and before application interaction; reasserts the same target after the workload's minimize/maximize sequence.
- **Edge Start-4KVideoHeavy:** distinguishes a newly created Edge HWND, stops `Browser_Start`, waits for original initialization, then allocates a destination outside the measurement.
- **Edge Run-4KVideoHeavy:** uses the last successful target from Start without advancing state and reasserts it after later focus/maximize operations.
- **Microsoft Excel:** allocates the actual opened document immediately after `Open_Excel_Document` stops; reasserts after later minimize/maximize.
- **Microsoft PowerPoint:** allocates the actual opened presentation immediately after `Open_Powerpoint_Document` stops; reasserts after later minimize/maximize.
- **Microsoft Word:** allocates the actual opened document immediately after `Open_Word_Document` stops; reasserts after later minimize/maximize.
- **Close Excel/Word/PowerPoint:** cleanup only; no placement and no state consumption.

The Edge Start adaptation replaces its fixed local video path with `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`; stage that generic media file if the 4K path is required. The workload retains its original cadence and tab count.

Run files according to `reference/test-scenario/workload-sequence.txt`. Do not reorder or reinterpret enabled, `Run once`, or `Leave application running` settings. Status: generated/not validated in Script Editor or a full Login Enterprise scenario.

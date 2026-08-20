# Office Preview examples

These small workloads demonstrate the DLL pattern; they are not full Office application workloads and contain no EUX measurements. Word, Excel, PowerPoint, corrected Edge, and New Outlook launch/find/place have one-machine Login Enterprise 6.8.6 runtime evidence. Classic Outlook remains runtime-pending in that environment.

## Run order

1. `workloads/dll-backed/00-Prepare-MultiMonitor.cs`
2. `01-Reset-Placement-State.cs`
3. `10-Place-Microsoft-Word.cs`
4. `20-Place-Microsoft-Excel.cs`
5. `30-Place-Microsoft-PowerPoint.cs`
6. Choose one Outlook flavor if desired:
   - `40-Place-Microsoft-Outlook.cs` — Microsoft Outlook (Classic)
   - `41-Place-Microsoft-Outlook-New.cs` — Microsoft Outlook (New)
7. `50-Place-Microsoft-Edge.cs`

Do not run both Outlook variants in the standard sequence. With either one Outlook flavor included, fresh-state application indices are `0,1,0,1,0` on two displays and `0,1,2,0,1` on three. Without Outlook they are `0,1,0,1` and `0,1,2,0`. Reset is an explicit demonstration step; omit it when a longer Continuous/Load scenario should retain allocation continuity.

## Window ownership

Word, Excel, and PowerPoint preflight their durable class/process identities and abort if an existing base window could be moved accidentally. They then use documented `START` without absolute install paths or localized title matching.

- **Microsoft Outlook (Classic):** `TARGET:outlook.exe`, `OUTLOOK`, and the `rctrl_renwnd32` Explorer class. Its Classic Outlook UI/control assumptions do not apply to New Outlook.
- **Microsoft Outlook (New):** `TARGET:olk`, documented `START()`, and the resulting `MainWindow`. This runtime-proven example covers launch/find/place only; it does not establish New Outlook interaction automation or compatibility with the Classic Outlook/KW workload.

Classic Outlook and New Outlook are separate workload targets. Changing only `TARGET` or the executable is not a valid interaction-workload conversion.

Edge requires zero existing `Chrome_WidgetWin_1`/`msedge` base windows, uses the runtime-proven `START(processName: "msedge")`/`MainWindow` path, independently requires exactly one durable Edge window, and verifies that both HWNDs match before allocation. Testers must close existing Edge windows first. Each workload calls `PlaceNext` exactly once only after resolving its base window.

These heuristics still require broader lab validation across Office versions, localization, first-run/activation/sign-in, Protected View, application reuse, replacement windows, and session technology. Close existing Word/Excel/PowerPoint/classic Outlook/Edge durable windows before testing.

## Recorded local runtime evidence

On one Login Enterprise 6.8.6 machine, Word, Excel, PowerPoint, and corrected Edge launched and placed successfully. A standalone New Outlook proof established `TARGET:olk -> START() -> MainWindow -> NativeWindowHandle -> PlaceNext`, producing a verified two-monitor placement and clean `STOP()` in the proof. The repository New Outlook example intentionally omits `STOP()` to follow the Office Preview's scenario-controlled lifecycle. Classic Outlook was not tested because that machine provides New Outlook. New Outlook interactions beyond launch/find/place remain unproven.

## Lifecycle and cleanup

For an Application Test, use `Leave application running: ON` when these apps must survive into a later cleanup workload. These placement examples do not call `STOP()` and do not provide a broad process-killing Close workload because that could terminate unrelated user instances. Close the specifically opened windows through a reviewed scenario/workload, use Login Enterprise scenario cleanup, or use a clean disposable session. Preserve intended `Run once` behavior in Continuous/Load Tests; incidental process linger is not persistence.

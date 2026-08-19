# Office Preview examples

These five small workloads demonstrate the DLL pattern; they are not full Office application workloads and contain no EUX measurements. They are **generated/build-tested/static-validated; partner-lab runtime validation pending**.

## Run order

1. `workloads/dll-backed/00-Prepare-MultiMonitor.cs`
2. `01-Reset-Placement-State.cs`
3. `10-Place-Microsoft-Word.cs`
4. `20-Place-Microsoft-Excel.cs`
5. `30-Place-Microsoft-PowerPoint.cs`
6. `40-Place-Microsoft-Outlook.cs`
7. `50-Place-Microsoft-Edge.cs`

From a fresh state, the application indices are `0,1,0,1,0` on two displays and `0,1,2,0,1` on three. Reset is an explicit demonstration step; omit it when a longer Continuous/Load scenario should retain allocation continuity.

## Window ownership

Word, Excel, and PowerPoint preflight their durable class/process identities and abort if an existing base window could be moved accidentally. They then use documented `START` without absolute install paths or localized title matching. Outlook targets **classic Win32 Outlook** by `OUTLOOK` process and `rctrl_renwnd32` Explorer class, not by an English folder title. New Outlook is not supported or validated. Outlook profile/sign-in/first-run/setup UI must already be resolved.

Edge snapshots qualifying `Chrome_WidgetWin_1`/`msedge` top-level handles before launch, opens a new window, polls for a uniquely new durable window, and aborts rather than binding an arbitrary existing window. Each workload calls `PlaceNext` exactly once only after resolving its base window.

These heuristics still require lab validation across Office versions, localization, first-run/activation/sign-in, Protected View, application reuse, replacement windows, and session technology. Close existing Word/Excel/PowerPoint/classic Outlook instances before testing. Existing Edge windows may remain, but ownership must resolve uniquely.

## Lifecycle and cleanup

For an Application Test, use `Leave application running: ON` when these apps must survive into a later cleanup workload. These examples do not provide a broad process-killing Close workload because that could terminate unrelated user instances. Close the specifically opened windows through a reviewed scenario/workload, or use a clean disposable session. Preserve intended `Run once` behavior in Continuous/Load Tests; incidental process linger is not persistence.

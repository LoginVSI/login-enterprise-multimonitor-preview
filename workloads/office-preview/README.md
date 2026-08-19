# Office Preview example workloads

Status: **generated/build-tested/static-validated; partner-lab runtime validation pending**.

This small example set is easier to inspect than the complete Knowledge Worker adaptations. Each application is a separate Login Enterprise workload with one associated target, uses documented `START`/`MainWindow` matching drawn from the supplied examples, loads the real staged Preview DLL, and calls `PlaceNext` once for the durable base window.

Run in this order after `workloads/dll-backed/00-Prepare-MultiMonitor.cs`:

1. `01-Reset-Placement-State.cs` for a deterministic demonstration.
2. `10-Place-Microsoft-Word.cs`
3. `20-Place-Microsoft-Excel.cs`
4. `30-Place-Microsoft-PowerPoint.cs`
5. `40-Place-Microsoft-Outlook.cs`
6. `50-Place-Microsoft-Edge.cs`

With two monitors, expected indices are `0,1,0,1,0`. With three monitors they are `0,1,2,0,1`. The reset workload does not allocate.

Prerequisites:

- Desktop Office applications and Edge are installed for the test user.
- Outlook already has a usable profile and exposes an `Inbox*` main window. This example intentionally does not provision Outlook data.
- Run Prepare first so `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` exists.
- Use `Leave application running: No` for a simple independent Application Test, or configure persistence and explicit cleanup deliberately if the lab needs all windows to remain open.

These examples contain no EUX or application-response measurements. They demonstrate launch, durable base-window resolution, DLL loading, allocation, structured logging, and failure handling only. First-run dialogs, localization, existing instances, and exact application-version behavior require partner-lab validation. For the complete preserved-workload adaptation, use `workloads/knowledge-worker-multimonitor/`.

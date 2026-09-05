# Script Editor visual demo

`01-Office-MultiMonitor-Visual-Demo.cs` is a short visual workload for a manual Script Editor Play demonstration. It resets Preview placement state, opens Word, Edge, Excel, and PowerPoint in that order, and allocates each durable base window with `PlaceNext`.

Stage `LoginVSI.MultiMonitor.dll` first with `workloads/dll-backed/00-Prepare-MultiMonitor.cs`, then close existing durable windows for all four applications before pressing Play. The workload pauses for 3 seconds after Word, Edge, and Excel, holds the final layout for 8 seconds after PowerPoint, and then requests closure only for the exact windows it opened.

After each successful placement, this demo calls `IWindow.Focus()` on the same resolved durable window so the application is visible in the foreground during recording. This is an intentional workload choice, not a placement-success condition or a requirement for every workload. See [manual adaptation guidance](../../docs/adapt-your-own-workload.md).

This is a generated visual/demo asset with no measurement timers. It is not the canonical production workload lifecycle example and still requires Script Editor runtime validation in the recording environment.

# Development history

## Basic placement evidence

The preserved v0.1 POC provides successful implementation evidence for monitor enumeration, primary-first ordering, signed coordinates, restore/move/maximize, and monitor verification in a Login Enterprise workload.

## Persistent state evidence

The preserved v0.3 POC provides successful implementation evidence for the two-line state schema, reset behavior, round-robin selection, verified-success advancement, timing, and in-script phase continuity. Its two internal phases are not evidence for separate workload-file continuity.

## Reusable library

The Preview now contains a dependency-free `netstandard2.0` implementation with reflection-friendly APIs and structured results. Login Enterprise 6.8.6 Script Editor/Standalone Engine loading and invocation were proven on August 18, 2026. Current build and test results are recorded in `testing.md`.

## Sequential workloads

Script-only and DLL-backed Notepad/Paint then Edge pairs are generated. The DLL-backed proof used `START` for durable Notepad and Edge main windows and retained the existing successful Paint launch/`FindWindow` flow. CMD was removed because Windows Terminal hosted the visible command UI on the tested configuration and Login Enterprise could not find the requested standalone `cmd` window.

A dedicated preparation workload stages the helper from ScriptContent to the target-local Preview path. Initial staging and explicit remove-and-copy refresh are runtime-proven using the Script Editor engine's local ScriptContent directory. The later Desktop Connector Application Test proved appliance ScriptContent delivery and the retain-by-default path.

## August 18, 2026 Script Editor evidence

Login Enterprise 6.8.6 Script Editor/Standalone Engine discovered two physical monitors. Reset produced `LastUsedIndex=-1`; DLL-backed Notepad placed and verified on index 0, Paint on index 1, and a later independent Edge workload started from index 1, placed and verified on index 0, and persisted index 0. Deleting `state.txt` before the Edge workload recreated valid state and completed successfully. These separate standalone executions prove physical placement and file-state continuation in that harness, not serial multi-workload orchestration by the Login Enterprise platform.

Notepad started through `START` was stopped by the Standalone Engine. Paint, launched through the existing ShellExecute/FindWindow flow, remained open after its workload. This is a harness lifecycle observation, not a product defect.

## Integrated workloads

Ten enabled-scenario derivatives are present. Their durable/base Outlook, Edge, Excel, PowerPoint, and Word windows receive placement; splash, setup, dialogs, Outlook message/reminder windows, transient preparation, and cleanup workloads do not consume state by contract. No integrated workload or durable-HWND continuity is yet proven in Script Editor or a full scenario.

## Complete scenario, VDI, and product handoff

The authoritative sequence is preserved. A real Login Enterprise 6.8.6 Desktop Connector Application Test subsequently ran the three independent DLL-backed workloads serially in a Console / NoRemote session. Appliance ScriptContent delivery, missing/default-retain/forced-refresh Prepare paths, platform state continuity, the two-monitor `0,1,0` sequence, final `MonitorCount=2` / `LastUsedIndex=0` state, and successful AppExecutions are proven.

The clean canonical three-workload Preview source now consists of Prepare; Open/resolve unique durable base windows and round-robin place them; then bounded Close. Reset remains in Open/Place as an explicit fresh-demonstration policy rather than being mixed into DLL staging. The earlier successful pair moved unchanged to `workloads/dll-backed/regression/`. The canonical lifecycle is generated/not runtime-proven; its Desktop Connector test is next. Representative application and VDI validation remain later tracks.

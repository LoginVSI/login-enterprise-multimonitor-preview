# Script-only sequential proof

These two self-contained workloads embed the POC-derived generic monitor/state/placement logic directly:

1. `01-Initialize-Notepad-Paint.cs` resets `%TEMP%\LoginPI\MultiMonitor\state.txt` once, then launches and places Notepad and Paint.
2. `02-Continue-Edge.cs` preserves valid state, then launches and places Edge through documented `START`/`MainWindow`.

For the three successful placements, the expected logical indices are:

- one monitor: `0,0,0`;
- two monitors: `0,1,0`;
- three or more monitors: `0,1,2`.

Only intended durable/base windows may consume these destinations. Actual DLL-backed testing showed raw Notepad and Edge `ShellExecute` PIDs could exit while visible UI existed elsewhere or in a reused process, so these script-only test workloads also use `START`/`MainWindow`. Paint retains its existing launch/`FindWindow` flow because that flow placed successfully. CMD is not used because the tested modern Windows configuration hosted the visible command UI in Windows Terminal rather than an independently discoverable `cmd` top-level window.

State access uses an exclusive `FileStream`; disposal releases mutual exclusion and then attempts to delete the zero-byte `.lock` marker. Abrupt termination or a cleanup failure can leave the marker behind, but the marker alone does not hold the lock and later calls still acquire mutual exclusion through `FileShare.None`.

Script Editor can validate each file independently. The equivalent DLL-backed sequence is runtime-proven, but these script-only adaptations remain generated/not validated. Only a real Login Enterprise scenario running distinct files in order can validate platform-orchestrated cross-workload state continuity.

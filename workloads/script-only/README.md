# Script-only sequential proof

These two self-contained workloads embed the POC-derived generic monitor/state/placement logic directly:

1. `01-Initialize-Notepad-Paint.cs` resets `%TEMP%\LoginPI\MultiMonitor\state.txt` once, then launches and places Notepad and Paint.
2. `02-Continue-Cmd-Edge.cs` preserves valid state, then launches and places a uniquely titled Command Prompt and a newly distinguished Edge `about:blank` window.

For the first four successful placements, the expected logical indices are:

- one monitor: `0,0,0,0`;
- two monitors: `0,1,0,1`;
- three monitors: `0,1,2,0`;
- four or more monitors: `0,1,2,3`.

The second workload snapshots existing top-level Edge HWNDs before launch and accepts only a newly observed Edge window. It aborts instead of deliberately falling back to an unrelated existing window.

Only intended durable/base windows may consume these four destinations. The process-only Notepad/Paint selectors, titled Command Prompt selector, and newly observed top-level Edge selector remain generated/not runtime-validated for splash avoidance and HWND durability. No executable selector is changed without application evidence establishing a safer supported title/class/process match. During validation, record each selected title/class/process/HWND and verify that transient or secondary windows do not advance state.

State access uses an exclusive `FileStream`; disposal releases mutual exclusion and then attempts to delete the zero-byte `.lock` marker. Abrupt termination or a cleanup failure can leave the marker behind, but the marker alone does not hold the lock and later calls still acquire mutual exclusion through `FileShare.None`.

Script Editor can validate each file independently. Only a real Login Enterprise scenario running these distinct files in order can validate cross-workload state continuity. Status: generated/not validated in Script Editor or a full scenario.

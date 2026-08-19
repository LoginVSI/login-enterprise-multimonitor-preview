# Workloads

This directory contains new Preview workloads derived from repository evidence. Immutable files under `reference/original-workloads/` and `reference/proven-pocs/` must never be changed.

- `script-only/` isolates placement and state behavior in self-contained scripts.
- `dll-backed/` contains a dedicated Preview DLL preparation workload and demonstrates the same sequence through the reusable staged DLL.
- `integrated/` preserves representative workload actions while adding DLL-backed placement at deliberate application-specific insertion points.

The conceptual scenario order is multi-monitor prepare/DLL staging, existing Office/M365 preparation, application Start workloads, then Run workloads. Adding that new prepare step is a future manual scenario configuration action; do not edit `reference/test-scenario/workload-sequence.txt`.

Only a durable/base application window may consume a round-robin destination. Splash, setup, file-dialog, Outlook compose/read/reminder, popup, child, and temporary launcher windows do not. When the workload owns startup and needs the durable main window, prefer documented `START`; do not equate a raw launch PID with ownership of modern visible UI. Individual workload evidence is stated in the subdirectory READMEs; integrated workloads remain generated/not validated unless explicitly noted.

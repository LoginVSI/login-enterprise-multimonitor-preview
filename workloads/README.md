# Workloads

This directory contains new Preview workloads derived from repository evidence. Immutable files under `reference/original-workloads/` and `reference/proven-pocs/` must never be changed.

- `script-only/` isolates placement and state behavior in self-contained scripts.
- `dll-backed/` contains a dedicated Preview DLL preparation workload and demonstrates the same sequence through the reusable staged DLL.
- `integrated/` preserves representative workload actions while adding DLL-backed placement at deliberate application-specific insertion points.

The proven simple harness order is multi-monitor prepare/DLL staging, Notepad/Paint initialization, then Edge continuation. The future final Preview order is Prepare, Open/resolve/place durable application windows, then Close applications. Do not edit `reference/test-scenario/workload-sequence.txt` in place.

Only a durable/base application window may consume a round-robin destination. Splash, setup, file-dialog, Outlook compose/read/reminder, popup, child, and temporary launcher windows do not. When the workload owns startup and needs the durable main window, prefer documented `START`; do not equate a raw launch PID with ownership of modern visible UI. Individual workload evidence is stated in the subdirectory READMEs; integrated workloads remain generated/not validated unless explicitly noted.

Application persistence is scenario-controlled. Application Test provides per-workload `Leave application running`, defaulting to off; Continuous Test and Load Test provide `Leave application running` plus `Run once`. Enable persistence deliberately when Open/Place must hand applications to Close, and preserve `Run once` intent in Continuous/Load adaptations.

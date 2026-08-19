# Workloads

This directory contains new Preview workloads derived from repository evidence. Immutable files under `reference/original-workloads/` and `reference/proven-pocs/` must never be changed.

- `script-only/` isolates placement and state behavior in self-contained scripts.
- `dll-backed/` contains the canonical Prepare -> Open/Place -> Close flow through the reusable staged DLL; its `regression/` directory retains the runtime-proven simple sequence.
- `integrated/` preserves representative workload actions while adding DLL-backed placement at deliberate application-specific insertion points.

The canonical current Preview order is Prepare, Open/resolve/place durable application windows, then Close applications. It is generated/not runtime-proven. The proven simple harness remains separately labeled under `dll-backed/regression/`. Future representative application adaptations remain under `integrated/` and are not part of this generic flow. Do not edit `reference/test-scenario/workload-sequence.txt` in place.

Only a durable/base application window may consume a round-robin destination. Splash, setup, file-dialog, Outlook compose/read/reminder, popup, child, and temporary launcher windows do not. When the workload owns startup and needs the durable main window, prefer documented `START`; do not equate a raw launch PID with ownership of modern visible UI. Individual workload evidence is stated in the subdirectory READMEs; integrated workloads remain generated/not validated unless explicitly noted.

Application persistence is scenario-controlled. For Application Test, set Prepare off/not relevant, Open/Place on, and Close off. Continuous Test and Load Test also expose `Run once`; preserve the intended one-time behavior when adapting the flow. Close performs explicit bounded cleanup without allocating or changing round-robin state.

# Workloads

This directory contains new Preview workloads derived from repository evidence. Immutable files under `reference/original-workloads/` and `reference/proven-pocs/` must never be changed.

- `script-only/` isolates placement and state behavior in self-contained scripts.
- `dll-backed/` contains the canonical Prepare -> Open/Place -> Close flow through the reusable staged DLL; its `regression/` directory retains the runtime-proven simple sequence.
- `office-preview/` provides small Word, Excel, PowerPoint, Outlook, and Edge first-lab examples.
- `knowledge-worker-multimonitor/` preserves the complete representative workload actions while adding DLL-backed placement at deliberate insertion points; its manifest makes source/delta review explicit.

The canonical generic Prepare -> Open/Place -> Close flow is runtime-proven in the recorded Desktop Connector environment. The simple harness remains under `dll-backed/regression/`. Office and complete Knowledge Worker adaptations are generated/build-tested/static-validated with partner-lab runtime validation pending. Do not edit `reference/test-scenario/workload-sequence.txt` in place.

Only a durable/base application window may consume a round-robin destination. Splash, setup, file-dialog, Outlook compose/read/reminder, popup, child, and temporary launcher windows do not. Classic Outlook and New Outlook are separate targets with different executable/window/control models; do not substitute one for the other. When the workload owns startup and needs the durable main window, prefer documented `START`; do not equate a raw launch PID with ownership of modern visible UI. Individual workload evidence is stated in the subdirectory READMEs; Office/Knowledge Worker workloads remain partner-lab pending unless explicitly updated from recorded evidence.

Application persistence is scenario-controlled. For Application Test, set Prepare off/not relevant, Open/Place on, and Close off. Continuous Test and Load Test also expose `Run once`; preserve the intended one-time behavior when adapting the flow. Close performs explicit bounded cleanup without allocating or changing round-robin state.

# Workloads

This directory contains new Preview workloads derived from repository evidence. Immutable files under `reference/original-workloads/` and `reference/proven-pocs/` must never be changed.

- `script-only/` isolates placement and state behavior in self-contained scripts.
- `dll-backed/` demonstrates the same sequence through the reusable staged DLL.
- `integrated/` preserves representative workload actions while adding DLL-backed placement at deliberate application-specific insertion points.

All workload source is generated/not validated in Login Enterprise until its status is explicitly updated with evidence.

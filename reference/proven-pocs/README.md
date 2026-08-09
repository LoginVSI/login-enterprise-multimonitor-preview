# Proven proofs of concept

This directory preserves two proof-of-concept workloads that executed successfully and informed the Preview implementation:

- `MultiMonitor-Basic-Placement-Poc-v0.1.cs` provides implementation evidence for monitor discovery, primary-first ordering, signed coordinates, restore/move/maximize behavior, and placement verification in one workload.
- `MultiMonitor-PersistentState-TwoPhase-Poc-v0.3.cs` adds the two-line file-backed state schema, reset and round-robin behavior, verified-success state advancement, placement timing, and continuity between two phases inside one workload execution.

These files are preserved evidence, not style-cleanup targets. The v0.3 internal phases do not prove state continuity across separate workload files or a complete Login Enterprise scenario. Neither POC establishes compatibility across every display topology, Login Enterprise release, VDI platform, or application.

Run `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify` before and after changes that consume this evidence.

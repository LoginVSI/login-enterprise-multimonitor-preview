# Test-scenario reference

This directory preserves known-good Login Enterprise scenario ordering and configuration evidence. Individual C# files do not fully describe Knowledge Worker execution: order, enabled state, `Run once`, and `Leave application running` can materially affect applications and persistent state.

`workload-sequence.txt` is a reference, not an executable scenario export. Do not silently change its ordering or settings. Put experiments and integrated variants elsewhere and document any deliberate divergence.

Its SHA-256 is tracked by `reference/PRESERVED-EVIDENCE-SHA256SUMS.txt` and verified with `scripts/Verify-PreservedEvidenceHashes.ps1 -Verify`. The screenshot is supporting evidence and is intentionally not hashed by that manifest.

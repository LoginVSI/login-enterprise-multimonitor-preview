# Repository helper scripts

## Verify-ReferenceHashes.ps1

Use `-Generate` only after the repository owner has added and reviewed the complete immutable baseline workload set. It replaces `reference/original-workloads/SHA256SUMS.txt`. Use `-Verify` before and after major implementation passes to detect modified, missing, and unexpected files.

## Test-PublicSafety.ps1

Scans Git-tracked and untracked non-ignored source-like files for a deliberately small set of high-confidence identity, local-path, credential-assignment, and obvious token patterns. Rules are defined near the top of the script for extension. Generated/build/artifact directories are excluded.

This helper supplements human public-safety review and repository security/secret-scanning tooling; it does not replace them.

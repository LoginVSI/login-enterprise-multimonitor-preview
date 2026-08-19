# Repository helper scripts

## Test-Repository.ps1

The authoritative repository validation entry point is:

```powershell
.\scripts\Test-Repository.ps1
```

It runs whitespace/integrity checks, both preserved-reference verifiers, the public-safety scan, DLL and workload source contracts, restore/build, and all unit/pure-logic/source-contract tests. `-Fast` runs repository integrity and static checks without restore/build/unit execution; it is useful during editing but does not replace the full publication gate.

`Test-WorkloadContracts.ps1` validates the generic, Office Preview, and Knowledge Worker workload invariants and mapping manifest. `Test-DllContract.ps1` enforces the reusable DLL target/dependency/checksum boundary. `Test-Documentation.ps1` verifies local Markdown links and the core agentic-adaptation skill contract. These are static checks, not Login Enterprise runtime tests.

## New-TestLabBundle.ps1

Creates an ignored partner/test-lab directory under `artifacts/` containing only the DLL/checksum, Prepare, Office Preview, Knowledge Worker adaptations, and quickstart. Add `-Zip` for an archive. The script verifies the DLL contract/checksum first; the repository remains authoritative and no logs/reference evidence are bundled.

## Verify-ReferenceHashes.ps1

Use `-Generate` only after the repository owner has added and reviewed the complete immutable baseline workload set. It replaces `reference/original-workloads/SHA256SUMS.txt`. Use `-Verify` before and after major implementation passes to detect modified, missing, and unexpected files.

## Verify-PreservedEvidenceHashes.ps1

Verifies the separate `reference/PRESERVED-EVIDENCE-SHA256SUMS.txt` manifest for preserved POC source and the authoritative `reference/test-scenario/workload-sequence.txt`. It detects modified, missing, and unexpected POC files. The supporting scenario PNG and reference README files are intentionally excluded. Use `-Generate` only after explicit review of the preserved evidence baseline.

## Test-PublicSafety.ps1

Scans Git-tracked and untracked non-ignored source-like files for a deliberately small set of high-confidence identity, local-path, credential-assignment, obvious token, and non-placeholder email patterns. Explicit immutable evidence paths are excluded from the email rule; normal output reports only rule/path/line and never echoes candidate values. Generated/build/artifact directories are excluded.

This helper supplements human public-safety review and repository security/secret-scanning tooling; it does not replace them.

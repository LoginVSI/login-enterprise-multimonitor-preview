# Validation status

Generated package, not runtime proven. Local repository validation builds the reusable library, not Login Enterprise workload scripts. Script Editor compilation, individual runtime execution, interactive monitor verification, and actual scenario loop testing remain required.

The full repository command initially failed at restore because sandbox access to the user's NuGet configuration was denied. A rerun with the required access passed restore, Release build and all 30 repository tests. The build rewrote dist as designed; original DLL and checksum bytes were restored afterward. No distributable change is part of this adaptation.

Commands used from the repository root:

```powershell
.\scripts\Test-Repository.ps1
.\scripts\Test-Repository.ps1 -Fast
.\scripts\Test-KW25Contracts.ps1 -SourceDirectory 'C:\Repos\CommunityWorkloads\Knowledge Worker 2025'
.\scripts\Verify-ReferenceHashes.ps1 -Verify
.\scripts\Verify-PreservedEvidenceHashes.ps1 -Verify
.\scripts\Test-PublicSafety.ps1
.\scripts\Test-DllContract.ps1
git diff --check
```

KW25 contracts compare all output hashes and source behavior after removing explicitly marked additions, allow only the disclosed recipient substitution, preserve Prepare bytes, forbid Run allocation/global-last-used, and check documented allocation expectations for 1 through 4 monitors. Supplying SourceDirectory additionally checks all twelve external source hashes. These checks do not prove HWND ownership or desktop behavior at runtime.

Final engineering audit covered original targets/classes, original actions, content, timer boundaries, first-run UI, native-handle lifetime and scenario persistence. Public-safety automation supplements source review. Customer compilation and runtime execution are the customer's responsibility, not a claim of this audit. No customer environment data or logs are packaged.

Audit fixes: all ten allocating/maintaining scripts now recheck unique process/class/title ownership immediately before placement and guard null results. Assignment records contain monitor count plus index. Run validates assignment range and shared-state presence/schema/count before placement, preventing silent recovery from missing/corrupt state during ordinary maintenance. Run contains no assignment writes/deletes. A physical topology change may still trigger the DLL's documented failure/reset path; same-count topology changes and concurrent external state changes remain unsupported. Restart the scenario after failures or topology changes.

The final audit runs the commands above again, including all 30 existing library tests and expanded KW25 source contracts. Source contracts additionally verify ownership guards, count/range/shared-state checks, read-only Run assignments, unique manifest coverage and the original BMP hash. No Login Enterprise script compilation or runtime execution occurred. The mixed Office/KW22 Edge configuration was not supplied or tested; conditional Office-only expectations in the README are not mixed-scenario proof.

Final audit results: full repository validation passed restore, Release build and 30/30 tests after the sandbox NuGet access failure was resolved by an authorized rerun. Fast validation, external-source KW25 contracts, both protected-hash checks, public safety, restored-distributable contract and git diff --check all passed. The final working tree contains only intended KW25 package, contract and documentation changes. The original DLL/checksum bytes were restored after the build and remain unchanged. This validation record precedes the owner-directed release commit.

The repository owner subsequently directed publication and accepted responsibility for the decision, as recorded in [PUBLICATION.md](PUBLICATION.md). Publication does not change the static/build-only validation status. Customer runtime testing remains pending.

Publication packaging checks removed trailing whitespace from adapted scripts only. Directory-local Git attributes retain LF for manifest-verified adapted files and disable text conversion/diff for the two byte-identical Prepare copies. This preserves source bytes and manifest hashes across clean checkouts without changing application behavior.

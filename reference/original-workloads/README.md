# Original workloads

Place complete, known-good baseline Login Enterprise workloads here. These files are immutable evidence: never edit, reformat, rename, delete, modernize, auto-fix, or add multi-monitor logic to them. Create every adaptation as a new file under `workloads/`.

After the owner adds and reviews the complete baseline set, explicitly establish its manifest with:

```powershell
.\scripts\Verify-ReferenceHashes.ps1 -Generate
```

Then use `-Verify` before and after major implementation work. Generation replaces the manifest and therefore must be an intentional owner action.

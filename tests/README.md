# Tests

`LoginVSI.MultiMonitor.Tests/` is a dependency-free `net8.0` console harness. Its 30 tests cover round-robin sequences for one through four displays, synthetic primary-first ordering, negative coordinates, valid/invalid/missing state, repair persistence, monitor-count changes, serialization, round trips, replacement writes, the safe zero-HWND failure result, canonical workload source contracts, and the compiled reusable-DLL framework/dependency contract.

Run through `build.ps1` or, from the repository root, run:

```powershell
dotnet run --project tests/LoginVSI.MultiMonitor.Tests/LoginVSI.MultiMonitor.Tests.csproj
```

Repository-wide workload and preserved-source checks run through `scripts/Test-Repository.ps1`. These automated checks do not claim actual window movement, Login Enterprise compatibility, cross-workload continuity, an interactive display topology, or VDI behavior.

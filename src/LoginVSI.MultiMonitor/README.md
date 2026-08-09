# LoginVSI.MultiMonitor

Dependency-free application-neutral Multi-Monitor Preview library targeting `netstandard2.0` with conservative C# 7.3 syntax. It has no LoginPI.Engine or third-party package dependency.

## Public API

- `MultiMonitorPlacer.ResetState(stateFilePath)`
- `MultiMonitorPlacer.PlaceNext(windowHandle, applicationName, stateFilePath, maximize, stabilizationDelayMilliseconds)`
- `MultiMonitorPlacer.PlaceOnMonitor(...)` for maintenance placement without state advancement.
- `MultiMonitorPlacer.PlaceLastUsed(...)` for persistent Start/Run pairs.
- `RoundRobinLogic`, `StateFileStore`, `MonitorDescriptor`, `PlacementState`, and `PlacementResult` expose testable logic and reflection-friendly data.

`PlaceNext` rediscovers displays, orders the primary first then secondaries by signed X/Y with deterministic tie-breakers, repairs state, selects the next index, restores and moves the current HWND, optionally maximizes it, verifies with `MonitorFromWindow`, reports elapsed time, and advances state only after verification.

`netstandard2.0` was chosen as a conservative single-assembly portability target that can be consumed by mature .NET Framework-era hosts as well as modern .NET without requiring a Login Enterprise reference. Local build success does not establish Script Editor/runtime compatibility; that requires manual validation.

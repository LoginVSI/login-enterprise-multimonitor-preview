# Known limitations and validation risks

This unsupported Preview has no committed delivery date, support claim, or validated compatibility envelope.

- **Login Enterprise runtime scope:** Login Enterprise 6.8.6 Script Editor/Standalone Engine compiled the prepare workload and loaded/invoked the staged `netstandard2.0` DLL. This is not evidence for other releases, the appliance delivery path, Desktop Connector orchestration, or the complete scenario.
- **`FindWindows` named arguments:** actual 6.8.6 compiler evidence resolves the supplied documentation inconsistency: use `className` and `processName`. Lowercase `classname` and `processname` are rejected. The earlier compile gate is closed.
- **Interactive desktop:** local unit tests do not prove actual movement, maximize behavior, focus, or verification on a desktop.
- **Durable-window identification:** `START(processName: "notepad")` and `START(processName: "msedge")` supplied durable windows in the tested 6.8.6 standalone runs. Integrated workloads have not been runtime-proven to exclude every splash, modal, temporary launcher, or replacement HWND. Title/class/process and same-HWND durability must still be recorded for each integration.
- **Raw launch process identity:** raw `ShellExecute` produced short-lived Notepad and Edge PIDs while their visible UIs lived elsewhere or reused another process. Do not treat the initial PID as durable-window ownership. `ShellExecute` remains usable only where the lifecycle is understood and handled.
- **Terminal-host behavior:** the tested `cmd.exe /k title ...` UI was hosted visibly by Windows Terminal and was not discoverable as the requested standalone `cmd` top-level window. CMD is therefore not a deterministic generic Preview harness on that configuration; no CMD-specific product logic is planned.
- **Edge:** existing instances, multiprocess behavior, delayed/new top-level windows, and later maximize/focus actions make it higher risk. Start aborts rather than selecting an indistinguishable existing window; Run assumes the authoritative Start/Run order.
- **Persistent applications:** `PlaceLastUsed` requires valid prior state. A monitor-count reset between Start and Run leaves no prior target and fails safely.
- **Focus:** the library does not force foreground focus; workloads do. Native placement and application actions may still affect focus.
- **Fullscreen and later actions:** applications can relocate or replace windows after verification. Integrated code reasserts after known minimize/maximize points only.
- **Scenario dependencies:** enabled state, order, `Run once`, and `Leave application running` semantics may materially affect behavior.
- **DPI/scaling:** mixed DPI and scaling behavior is unvalidated. Full monitor bounds are supplied before maximize.
- **Negative coordinates:** represented and unit-tested, but not proven on a physical topology.
- **State concurrency:** calls use a five-second exclusive file lock. Script-only and library implementations attempt best-effort marker deletion after releasing the stream; abrupt termination or deletion failure may leave a harmless zero-byte marker. Real concurrent workloads and non-local filesystems need validation.
- **Atomic state replacement:** implemented with same-directory `File.Replace`/`File.Move`; target filesystem behavior must be validated in deployment.
- **Monitor changes:** count changes reset state. Same-count topology or identity changes are not represented in the MVP schema.
- **Timing:** restore, three stabilization intervals, movement, maximize, verification, locking, and I/O add overhead. Edge maintenance placement adds repeated cadence overhead.
- **Preview DLL staging:** initial staging and forced refresh through the engine's local ScriptContent directory are proven in 6.8.6 Script Editor/Standalone Engine. Appliance delivery and platform execution remain unvalidated. This is an unsupported Preview mechanism, not formal distribution/update behavior.
- **DLL refresh/versioning:** the default retains an existing target-local DLL. Updating the appliance copy alone does not update those targets; intentional refresh requires the workload toggle. There is no version comparison, integrity check, rollback, fleet orchestration, or automatic toggle reset.
- **Media staging:** the integrated 4K Edge Start workload expects a generic local media file to be staged separately.
- **Deployment/security:** binary provenance, signing, version selection, update, rollback, and trust policy remain deployment responsibilities.

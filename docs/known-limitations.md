# Known limitations and validation risks

This unsupported Preview has no committed delivery date, support claim, or validated compatibility envelope.

- **Login Enterprise runtime scope:** Login Enterprise 6.8.6 Script Editor/Standalone Engine compiled and invoked the DLL, and a Desktop Connector Application Test proved appliance delivery plus simple serial three-workload orchestration in a Console / NoRemote session. This is not evidence for other releases, the final Knowledge Worker flow, or VDI protocols.
- **`FindWindows` named arguments:** actual 6.8.6 compiler evidence resolves the supplied documentation inconsistency: use `className` and `processName`. Lowercase `classname` and `processname` are rejected. The earlier compile gate is closed.
- **Interactive desktop:** actual two-monitor movement and verification are proven for the simple Notepad/Paint/Edge Desktop Connector harness. Mixed DPI, broader topologies, integrated applications, focus interactions, and VDI remain unvalidated.
- **Durable-window identification:** `START(processName: "notepad")` and `START(processName: "msedge")` supplied durable windows in the tested 6.8.6 standalone runs. Integrated workloads have not been runtime-proven to exclude every splash, modal, temporary launcher, or replacement HWND. Title/class/process and same-HWND durability must still be recorded for each integration.
- **Raw launch process identity:** raw `ShellExecute` produced short-lived Notepad and Edge PIDs while their visible UIs lived elsewhere or reused another process. Do not treat the initial PID as durable-window ownership. `ShellExecute` remains usable only where the lifecycle is understood and handled.
- **Terminal-host behavior:** the tested `cmd.exe /k title ...` UI was hosted visibly by Windows Terminal and was not discoverable as the requested standalone `cmd` top-level window. CMD is therefore not a deterministic generic Preview harness on that configuration; no CMD-specific product logic is planned.
- **Edge:** existing instances, multiprocess behavior, delayed/new top-level windows, and later maximize/focus actions make it higher risk. Start aborts rather than selecting an indistinguishable existing window; Run assumes the authoritative Start/Run order.
- **Persistent applications:** `PlaceLastUsed` requires valid prior state. A monitor-count reset between Start and Run leaves no prior target and fails safely.
- **Focus:** the library does not force foreground focus; workloads do. Native placement and application actions may still affect focus.
- **Fullscreen and later actions:** applications can relocate or replace windows after verification. Integrated code reasserts after known minimize/maximize points only.
- **Scenario dependencies:** persistence must be deliberate. Application Test provides `Leave application running` and defaults it off; Continuous Test and Load Test provide both `Leave application running` and `Run once`. The final Open/Place action must leave applications running when a later Close action owns cleanup, and Continuous/Load adaptations must preserve intended `Run once` semantics.
- **DPI/scaling:** mixed DPI and scaling behavior is unvalidated. Full monitor bounds are supplied before maximize.
- **Negative coordinates:** represented and unit-tested, but not proven on a physical topology.
- **State concurrency:** calls use a five-second exclusive file lock. Script-only and library implementations attempt best-effort marker deletion after releasing the stream; abrupt termination or deletion failure may leave a harmless zero-byte marker. Real concurrent workloads and non-local filesystems need validation.
- **Atomic state replacement:** implemented with same-directory `File.Replace`/`File.Move`; target filesystem behavior must be validated in deployment.
- **Monitor changes:** count changes reset state. Same-count topology or identity changes are not represented in the MVP schema.
- **Timing:** restore, three stabilization intervals, movement, maximize, verification, locking, and I/O add overhead. Edge maintenance placement adds repeated cadence overhead.
- **Preview DLL staging:** local-engine initial staging/refresh plus appliance delivery and missing/default-retain/forced-refresh platform paths are proven in the tested 6.8.6 environments. This remains an unsupported Preview mechanism, not formal distribution/update behavior.
- **DLL refresh/versioning:** the default retains an existing target-local DLL. Updating the appliance copy alone does not update those targets; intentional refresh requires the workload toggle. There is no version comparison, integrity check, rollback, fleet orchestration, or automatic toggle reset.
- **Media staging:** the integrated 4K Edge Start workload expects a generic local media file to be staged separately.
- **Deployment/security:** binary provenance, signing, version selection, update, rollback, and trust policy remain deployment responsibilities.

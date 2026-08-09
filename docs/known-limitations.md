# Known limitations and validation risks

This unsupported Preview has no committed delivery date, support claim, or validated compatibility envelope.

- **Login Enterprise runtime:** workload compilation and `netstandard2.0` reflection loading are not yet validated in Script Editor/runner.
- **`FindWindows` signature:** supplied documentation conflicts on `classname`/`processname` versus `className`/`processName`, and no preserved known-good call resolves it. Generated lowercase call sites remain unvalidated and are a Monday Script Editor compile gate.
- **Interactive desktop:** local unit tests do not prove actual movement, maximize behavior, focus, or verification on a desktop.
- **Window identification:** correct main-window selection remains application-specific. Splash, modal, secondary, and replacement windows can invalidate an earlier HWND.
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
- **DLL staging:** automatic distribution is not implemented because no supported custom DLL delivery contract was established in supplied docs.
- **Media staging:** the integrated 4K Edge Start workload expects a generic local media file to be staged separately.
- **Deployment/security:** binary provenance, signing, version selection, update, rollback, and trust policy remain deployment responsibilities.

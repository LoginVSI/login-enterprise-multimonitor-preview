# Known limitations and validation risks

Status: Planned investigation. This unsupported Preview has no committed product delivery date, support claim, or validated compatibility envelope.

- **Window identification:** the correct main window may be ambiguous.
- **Splash screens and secondary dialogs:** early or modal windows may be mistaken for the main window or remain elsewhere.
- **Replacement windows:** applications may replace an HWND after placement.
- **Browser multiprocess behavior:** process identity may not map directly to the intended browser window.
- **Existing browser instances:** launch behavior may reuse a window not created by the current workload.
- **Persistent Start/Run applications:** window ownership and state may span independent workloads and require full-scenario evidence.
- **Focus:** native movement or restore operations may affect foreground focus.
- **Maximize/fullscreen:** restoring, moving, maximizing, or fullscreen transitions may interact differently.
- **Later workload actions:** subsequent steps may change location, size, focus, or window identity.
- **Scenario ordering:** order and `Run once`/`Leave application running` settings may materially affect results.
- **Display topology:** enumeration order and active-monitor changes require deterministic handling.
- **DPI and scaling:** logical and physical coordinate behavior is not yet established.
- **Negative coordinates:** displays left of or above the primary require signed-coordinate validation.
- **State concurrency:** multiple writers, partial writes, and recovery remain TBD.
- **Timing and cadence:** discovery, persistence, placement, verification, and retry add overhead.
- **DLL/runtime compatibility:** target framework, Script Editor loading, and runtime compatibility are unproven.
- **Deployment/distribution:** helper location, updates, trust, and rollback are not designed.

Update this list from evidence; distinguish confirmed limitations from risks still under investigation.

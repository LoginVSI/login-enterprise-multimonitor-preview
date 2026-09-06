# Known limitations

This is an unsupported, use-at-your-own-risk engineering Preview with no GA, support, compatibility, roadmap, or release commitment.

- Runtime evidence is specific to Login Enterprise 6.8.6 and the recorded Script Editor plus Desktop Connector Console / NoRemote environments.
- Office Preview Word/Excel/PowerPoint, corrected Edge, and New Outlook launch/find/place passed on one local Login Enterprise 6.8.6 machine. The representative two-monitor external partner-lab KW Application Test passed 7/7, including adapted Classic Outlook and Edge Start/Run. Multi-loop resilience, New Outlook interaction automation, and broader compatibility remain pending. See [evidence status](evidence-status.md).
- Office examples do not automate first-run, activation, sign-in, profile creation, Protected View, localization differences, or every replacement-window path.
- Classic Outlook (`OUTLOOK` / `rctrl_renwnd32`) and New Outlook (`olk` / `START()` / `MainWindow`) are separate Preview targets. Only New Outlook launch/find/place has local runtime evidence; its post-launch interaction automation and any conversion of Classic Outlook/KW interactions remain unsupported and unvalidated.
- Word/Excel/PowerPoint examples abort when pre-existing durable windows create ownership ambiguity. Edge attempts new-window disambiguation and aborts when unique ownership cannot be established.
- Playwright and legacy Selenium use different workload-side browser-window resolution patterns. The validated Playwright `WebScriptBase` path correlates a new native top-level browser HWND through standard .NET process inspection; the legacy Selenium `ScriptBase` path uses Login Enterprise `FindWindows()`/`IWindow` APIs where available.
- Browser ownership must remain unambiguous. Pre-existing browser windows, concurrently appearing windows, process reuse, and later replacement windows can complicate correlation. The examples fail instead of choosing an arbitrary `chrome.exe` or `msedge.exe` window, but broad ownership behavior has not been qualified.
- Playwright/Chrome and legacy Selenium/Edge each passed a bounded two-monitor browser-plus-Paint flow. This does not establish universal browser, browser-version, remoting, display-topology, or workload compatibility.
- Legacy Selenium still requires normal browser/WebDriver compatibility. The successful bounded run paired Edge 152.0.4191.66 with EdgeChromium152; version installation and management are not Multi-Monitor Preview responsibilities.
- The adapted KW Edge workload intentionally substitutes `about:blank` for one customer-oriented target, reducing exact content fidelity. Its generic media path requires explicit staging.
- Preserved supplied evidence remains verbatim, including historical corporate example addresses. They are not credentials and must not be used as active recipients; adapted public content uses reserved placeholders.
- Three-monitor partner evidence demonstrates Word, Excel, and PowerPoint on indices `0,1,2`; it does not qualify every three-monitor topology or the full KW flow. Mixed DPI, broader/negative physical topologies, other Windows/Office/LE versions, and Citrix/Horizon/RDP/other protocols remain pending unless specifically recorded in [evidence status](evidence-status.md).
- A two-monitor one-user Load Test initially followed the expected pattern, but monitored multi-loop resilience is still in progress. An observed EUX reporting interruption has no established causal link to placement.
- A raw launch PID may not own modern durable UI. Existing instances, process reuse, delayed windows, and later app repositioning remain integration risks.
- Maintenance requires a valid prior allocation. Missing/malformed state may be repaired without advancing; monitor-count change loses the prior target and maintenance fails safely.
- `PlaceLastUsed` reapplies the global `LastUsedIndex`, not a per-application allocation. If another application allocates between a Start and its Run, Run follows that most recent allocation. In the representative Knowledge Worker sequence this happens from the second Continuous/Load loop onward, because Edge Start is `Run once` while Classic Outlook allocates every loop: on two monitors the indices coincide, on three or more monitors Edge Run moves Edge onto the monitor Outlook just received.
- State serializes through a five-second local file lock. Lock behavior is unit-tested; real concurrent platform workloads and non-local filesystems remain runtime pending.
- State tracks monitor count/index, not display identity; same-count topology changes are not represented.
- Placement restore/move/maximize/verification and maintenance add measurable overhead and may affect focus/cadence.
- Canonical generic cleanup is bounded and skips ambiguous multiple-window matches. The Office examples avoid broad process-killing cleanup.
- Preview staging has no version negotiation, signing policy, fleet update, rollback, or automatic refresh. Updating the appliance copy alone does not replace a retained target-local DLL.
- A supported product implementation would ideally hide framework-specific native-window placement mechanics from workload authors. This is a possible productization direction, not a commitment or a requirement that legacy Selenium become a future product feature.
- The `FindWindows` casing issue is resolved: actual 6.8.6 compiler evidence requires `className` and `processName`; lowercase variants fail.

See [troubleshooting](troubleshooting.md) and [testing](testing.md) before reporting a defect.

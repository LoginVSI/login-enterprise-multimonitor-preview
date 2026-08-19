# Known limitations

This is an unsupported, use-at-your-own-risk engineering Preview with no GA, support, compatibility, roadmap, or release commitment.

- Runtime evidence is specific to Login Enterprise 6.8.6 and the recorded Script Editor plus Desktop Connector Console / NoRemote two-monitor environment.
- Office Preview and representative KW workloads are static/build validated only; partner-lab runtime validation is pending.
- Office examples do not automate first-run, activation, sign-in, profile creation, Protected View, localization differences, or every replacement-window path.
- Outlook scope is classic Win32 Outlook (`OUTLOOK` / `rctrl_renwnd32`). New Outlook is unsupported/unvalidated.
- Word/Excel/PowerPoint examples abort when pre-existing durable windows create ownership ambiguity. Edge attempts new-window disambiguation and aborts when unique ownership cannot be established.
- The adapted KW Edge workload intentionally substitutes `about:blank` for one customer-oriented target, reducing exact content fidelity. Its generic media path requires explicit staging.
- Preserved supplied evidence remains verbatim, including historical corporate example addresses. They are not credentials and must not be used as active recipients; adapted public content uses reserved placeholders.
- Mixed DPI, broader/negative physical topologies, 3+ displays, other Windows/Office/LE versions, and Citrix/Horizon/RDP/other protocols remain pending unless specifically recorded in [testing](testing.md).
- A raw launch PID may not own modern durable UI. Existing instances, process reuse, delayed windows, and later app repositioning remain integration risks.
- Maintenance requires a valid prior allocation. Missing/malformed state may be repaired without advancing; monitor-count change loses the prior target and maintenance fails safely.
- State serializes through a five-second local file lock. Lock behavior is unit-tested; real concurrent platform workloads and non-local filesystems remain runtime pending.
- State tracks monitor count/index, not display identity; same-count topology changes are not represented.
- Placement restore/move/maximize/verification and maintenance add measurable overhead and may affect focus/cadence.
- Canonical generic cleanup is bounded and skips ambiguous multiple-window matches. The Office examples avoid broad process-killing cleanup.
- Preview staging has no version negotiation, signing policy, fleet update, rollback, or automatic refresh. Updating the appliance copy alone does not replace a retained target-local DLL.
- The `FindWindows` casing issue is resolved: actual 6.8.6 compiler evidence requires `className` and `processName`; lowercase variants fail.

See [troubleshooting](troubleshooting.md) and [testing](testing.md) before reporting a defect.

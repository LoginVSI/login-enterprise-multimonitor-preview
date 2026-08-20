# Troubleshooting

## Capture safe, useful context

Record the workload/application, Login Enterprise version, Windows or Windows Server version, Office/app version, VDI or connection method, monitor count/resolution/layout/scaling, existing application instances, first-run/profile state, selected durable title/class/process, structured placement result, and sanitized screenshots/video where useful.

Raw Engine logs can contain authentication, session, infrastructure, or personal material. Keep them in ignored local `artifacts/`; never publish them. Share only a reviewed minimal excerpt through an approved private channel.

## Common failures

- **DLL missing:** upload it to appliance ScriptContent and run Prepare. Confirm the staged target-local path.
- **Checksum mismatch:** download the DLL and `SHA256SUMS.txt` again; do not deploy until they match.
- **No durable window:** inspect launch handoff, first-run/setup UI, localization, process/class identity, and readiness.
- **Multiple matching windows:** close unrelated instances or strengthen bounded ownership logic; never guess.
- **Launch PID exited:** modern apps may hand off or reuse a process. Resolve the durable top-level window instead of trusting the first PID.
- **Classic Outlook not found:** complete profile/sign-in/setup and verify the Classic Outlook `OUTLOOK`/`rctrl_renwnd32` Explorer window. Do not substitute `olk` into a Classic interaction workload.
- **New Outlook not found:** use the separate `TARGET:olk` Office example with New Outlook installed and configured. Its evidence covers `START()`/`MainWindow` launch/find/place only, not Classic Outlook controls or interactions.
- **State reset:** missing, malformed, out-of-range, or monitor-count-changed state repairs to `LastUsedIndex=-1`. A topology change intentionally loses the prior maintenance target.
- **Verification failed:** capture target/verified indices, topology/scaling, application behavior after restore/move/maximize, and whether the HWND changed.
- **Application closed between workloads:** correct `Leave application running`; preserve `Run once` semantics for Continuous/Load Tests.
- **Cleanup skipped:** bounded Close logic avoids ambiguous multiple-window termination. Record existing instances and close them safely.

Use [testing](testing.md) for the evidence to capture before changing a workload's runtime status.

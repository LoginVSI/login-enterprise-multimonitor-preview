# Getting started

The Multi-Monitor Preview separates application knowledge from generic monitor placement. Your Login Enterprise workload finds the correct durable/base application window. `LoginVSI.MultiMonitor.dll` chooses a monitor, moves and verifies that window, then records the successful allocation.

## Prerequisites and evidence boundary

You need:

- a controlled Login Enterprise environment;
- a standard-user Windows interactive test session where every intended display is visible to that session;
- applications, profiles, first-run setup, content, and permissions prepared for the workloads you plan to run;
- the ability to add a file to Login Enterprise ScriptContent and import C# workloads.

The recorded local baseline is Login Enterprise 6.8.6. External partner evidence covers a representative two-monitor Knowledge Worker Application Test and three-monitor Office placement. This does not establish broad version, application, VDI, mixed-DPI, or topology compatibility. Read [evidence status](evidence-status.md) before testing.

## End-to-end path

1. Clone or download this repository and keep its directory structure intact.
2. Locate [`dist/LoginVSI.MultiMonitor.dll`](../dist/LoginVSI.MultiMonitor.dll). Optionally compare its SHA-256 hash with [`dist/SHA256SUMS.txt`](../dist/SHA256SUMS.txt).
3. Upload the DLL to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll` on the Login Enterprise appliance. This uses Login Enterprise ScriptContent. It is not a new Preview or product distribution service.
4. Import and run [`workloads/dll-backed/00-Prepare-MultiMonitor.cs`](../workloads/dll-backed/00-Prepare-MultiMonitor.cs). Prepare copies the appliance ScriptContent file to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` in the target session.
5. Choose a workload path: [small Office examples](../workloads/office-preview/README.md), [representative Knowledge Worker adaptations](../workloads/knowledge-worker-multimonitor/README.md), or [your own manual adaptation](adapt-your-own-workload.md).
6. Configure the scenario lifecycle, run it, and inspect the structured placement result. The [test-lab quickstart](test-lab-quickstart.md) gives exact settings and expected sequences.

## Staging and refresh behavior

Prepare always stages a missing target-local DLL. When `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` already exists, the default `ForceRefreshMultiMonitorDll = false` retains it. This prevents every consumer from downloading or replacing the DLL.

To intentionally replace a staged copy:

1. Upload the replacement DLL to the same appliance ScriptContent path.
2. Set `ForceRefreshMultiMonitorDll = true` in a disposable/imported copy of Prepare.
3. Run Prepare and confirm its remove, copy, and verification messages.
4. Return the toggle to `false`.

Updating only the appliance copy does not replace an existing retained target-local DLL. A stale DLL is therefore a diagnosable setup issue, not an automatic refresh feature.

## Placement state

Successful allocations use `%TEMP%\LoginPI\MultiMonitor\state.txt`:

```text
MonitorCount=<integer>
LastUsedIndex=<integer>
```

Use [`01-Reset-Placement-State.cs`](../workloads/office-preview/01-Reset-Placement-State.cs) before a fresh, deterministic demonstration. Do not reset between Start and Run, between applications in one intended sequence, or on every Continuous/Load loop. Preparation and Close do not reset. If the active monitor count changes, the helper safely starts a new primary-first sequence.

## What success looks like

Each call logs a structured result. Inspect at least:

- `MonitorCount`: displays discovered for this placement;
- `Target`: selected primary-first monitor index;
- `Verified`: monitor containing the window after movement;
- `StateAdvanced`: `True` only for a successful allocating `PlaceNext`;
- `Message`: success or failure detail.

Starting from fresh state, five applications should allocate `0,1,0,1,0` on two monitors and `0,1,2,0,1` on three. A maintenance call for the same application should keep its prior target and report `StateAdvanced=False`.

## Next steps

Use [adapt your own workload](adapt-your-own-workload.md) for a manual integration or the [copy/paste agent prompt](agentic-workload-adaptation.md) for Codex, Claude Code, or another capable coding agent. Review [troubleshooting](troubleshooting.md) when application readiness, window ownership, lifecycle settings, DLL freshness, or content paths do not match the expected result.

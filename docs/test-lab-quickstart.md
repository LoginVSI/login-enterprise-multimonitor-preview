# Test-lab quickstart

The generic framework is runtime-proven in the recorded Login Enterprise 6.8.6 Desktop Connector environment. Office and Knowledge Worker workloads remain **generated/build-tested/static-validated; partner-lab runtime validation pending**.

## 1. Get and verify the files

Download or clone:

- [`dist/LoginVSI.MultiMonitor.dll`](../dist/LoginVSI.MultiMonitor.dll) and [`SHA256SUMS.txt`](../dist/SHA256SUMS.txt);
- [`00-Prepare-MultiMonitor.cs`](../workloads/dll-backed/00-Prepare-MultiMonitor.cs);
- the [Office examples](../workloads/office-preview/) or [Knowledge Worker adaptations](../workloads/knowledge-worker-multimonitor/).

Verify the DLL:

```powershell
(Get-FileHash .\LoginVSI.MultiMonitor.dll -Algorithm SHA256).Hash.ToLowerInvariant()
Get-Content .\SHA256SUMS.txt
```

The hashes must match. Optional: `.\scripts\New-TestLabBundle.ps1 -Zip` creates an ignored convenience bundle under `artifacts/`.

## 2. Upload and stage

Upload the DLL to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`. Run Prepare before consumers. It stages `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`; missing copies stage automatically, existing copies retain by default, and `ForceRefreshMultiMonitorDll=true` performs the proven remove/copy/verify refresh. Return the toggle to `false` after deliberate refresh.

For Script Editor development, use its local engine ScriptContent directory and disposable workload copies; no installation-specific path is a product requirement.

## 3. Configure the session and scenario

Use an Application Test with a normal Connector/test-lab interactive desktop that exposes the intended monitor topology. Console / NoRemote is the proven generic baseline, not proof of other protocols.

Office order: Prepare, Reset, Word, Excel, PowerPoint, classic Outlook, Edge. Close pre-existing Word/Excel/PowerPoint/classic Outlook durable windows first. Office examples expect `0,1,0,1,0` on two monitors and `0,1,2,0,1` on three.

Knowledge Worker: add Prepare, then use adapted files in the immutable scenario order with original enabled/`Run once`/`Leave application running` intent. Outlook, Edge Start, Excel, PowerPoint, and Word allocate; Edge Run maintains without allocation; preparation/Close are neutral.

Application Test defaults `Leave application running` off. Turn it on for a Start/Open workload that must hand an app to Run/Close, then explicitly close it later. Continuous/Load Tests also expose `Run once`; preserve the original one-time intent. Never use incidental process linger as persistence.

## 4. Inspect results

Confirm the intended durable base window, target/verified monitor, `StateAdvanced`, final state, application events/results, and cleanup. Secondary/transient windows must not allocate. Capture environment dimensions listed in [troubleshooting](troubleshooting.md).

Keep raw Engine logs only under ignored `artifacts/` and never publish them. Use a reviewed, minimized excerpt through an approved private channel.

Passing build, static contracts, or GitHub Actions does not change runtime status. Record partner-lab evidence per workload before marking it proven.

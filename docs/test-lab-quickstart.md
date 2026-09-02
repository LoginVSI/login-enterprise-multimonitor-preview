# Test-lab quickstart

This is the shortest from-zero path for an unsupported customer evaluation. It assumes a standard-user Windows interactive session where two or more displays are visible to the Login Enterprise session. Application installation, licensing, profile readiness, first-run setup, and test content remain your responsibility.

The proven baseline and partner results are bounded. Read [evidence status](evidence-status.md) before interpreting a pass or failure.

## 1. Get and verify the Preview

Clone or download the repository. The required binary is [`dist/LoginVSI.MultiMonitor.dll`](../dist/LoginVSI.MultiMonitor.dll). SHA verification is optional but recommended:

```powershell
(Get-FileHash .\dist\LoginVSI.MultiMonitor.dll -Algorithm SHA256).Hash.ToLowerInvariant()
Get-Content .\dist\SHA256SUMS.txt
```

The values must match. `.\scripts\New-TestLabBundle.ps1 -Zip` can create an ignored convenience bundle under `artifacts/`.

## 2. Upload and stage the DLL

Upload the DLL to:

```text
/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll
```

This is the existing Login Enterprise ScriptContent surface, not a new product distribution mechanism.

Import and run [`00-Prepare-MultiMonitor.cs`](../workloads/dll-backed/00-Prepare-MultiMonitor.cs). It copies the ScriptContent DLL to:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

A missing file stages automatically. An existing file is retained while `ForceRefreshMultiMonitorDll=false`. To replace it deliberately, upload the new appliance copy, set the Prepare toggle to `true`, run Prepare once, confirm remove/copy/verify success, then return the toggle to `false`. Consumer workloads only verify and load the staged DLL.

## 3. Initialize state only when intended

Placement state is `%TEMP%\LoginPI\MultiMonitor\state.txt`. Run [`01-Reset-Placement-State.cs`](../workloads/office-preview/01-Reset-Placement-State.cs) for a fresh Office demonstration. Do not reset between Start and Run, between applications in one round-robin sequence, or on every Continuous/Load loop.

## 4. Choose a workload set

For the smallest evaluation, import the [Office Preview workloads](../workloads/office-preview/README.md) in this order:

1. Prepare
2. Reset
3. Word
4. Excel
5. PowerPoint
6. optionally one Outlook flavor
7. Edge

Do not run both Classic Outlook and New Outlook in the standard sequence. Close ambiguous pre-existing durable windows first. The simple Classic Outlook example assumes an installed, clean, configured profile. It does not perform the PRF/PST/profile bootstrap found in the representative Knowledge Worker workload.

For the representative flow, add Prepare and import the [Knowledge Worker adaptations](../workloads/knowledge-worker-multimonitor/README.md) in the immutable [`workload-sequence.txt`](../reference/test-scenario/workload-sequence.txt) order, preserving its enabled, `Run once`, and `Leave application running` intent. Classic Outlook, Edge Start, Excel, PowerPoint, and Word allocate. Edge Run performs non-allocating maintenance. Preparation and Close are state-neutral.

## 5. Configure lifecycle settings

In an Application Test, a Start/Open workload that hands an application to a later Run or Close needs `Leave application running: ON`. Prepare and Close do not. Application Test defaults may be off, so check them explicitly.

Continuous Tests and Load Tests also expose `Run once`. Preserve deliberate one-time preparation, startup, and cleanup behavior. Do not use incidental process linger as the application-persistence contract.

## 6. Run and inspect results

For every allocation, confirm the intended durable/base window and inspect:

- `MonitorCount`
- `Target`
- `Verified`
- `StateAdvanced`
- `Message`

Starting from fresh state, five allocating applications should use `0,1,0,1,0` on two monitors and `0,1,2,0,1` on three. `Verified` should equal `Target`. A successful `PlaceNext` should report `StateAdvanced=True`; `PlaceLastUsed` or `PlaceOnMonitor` maintenance should report `False`. Secondary windows must not consume another index.

Keep placement outside application-response, EUX, and performance timers wherever practical. Passing build/static checks or GitHub Actions does not prove this runtime behavior.

## 7. Common failures

- **First-run, profile, sign-in, or activation UI:** prepare the application lifecycle before judging placement. The helper does not create profiles or dismiss arbitrary setup dialogs.
- **Ambiguous pre-existing windows:** close them or adapt the workload to identify ownership safely. Do not move an unrelated window.
- **Transient launch process:** the spawned PID may hand the durable UI to another process. Resolve the real `MainWindow` or stable class/process combination.
- **Self-repositioning application:** place once after the durable window exists, then use non-allocating maintenance after known restore/maximize/focus changes.
- **Wrong scenario lifecycle:** verify `Leave application running`, `Run once`, Start/Run/Close order, and explicit cleanup.
- **Stale staged DLL:** use the Prepare force-refresh procedure, then return its toggle to `false`.
- **Missing demo media:** the representative Edge video is optional content. Point the workload to an already staged file or copy it to `%TEMP%\LoginPI\MultiMonitor\Big Buck Bunny Demo.mp4`.

See [troubleshooting](troubleshooting.md) for environment capture and deeper diagnosis. Use [manual adaptation](adapt-your-own-workload.md) or the [AI-assisted adaptation guide](agentic-workload-adaptation.md) for your own workload. Keep raw Engine logs under ignored `artifacts/` and never publish them.

# Login Enterprise Multi-Monitor Preview

Login Enterprise workloads can run in sessions with multiple displays, but workload authors need an intentional, reusable way to distribute durable application windows across them. This repository provides that mechanism as a functional engineering Preview.

> This Preview explores reusable multi-monitor workload placement for Login Enterprise. It is not generally available or officially supported, and it carries no support, compatibility, roadmap, or release commitment. It may inform future Login Enterprise capabilities, but no future product inclusion is implied. Evaluate it at your own risk.

## Choose your path

- **I just want to test it:** start with the [test-lab quickstart](docs/test-lab-quickstart.md).
- **I want to understand the basics:** read [getting started](docs/getting-started.md).
- **I want to adapt my own workload manually:** use [adapt your own workload](docs/adapt-your-own-workload.md).
- **I want an AI/coding agent to adapt workloads:** use the [agentic adaptation guide](docs/agentic-workload-adaptation.md) and repository [skill](skills/login-enterprise-multimonitor/SKILL.md).
- **I want implementation details:** read the [architecture](docs/architecture.md).
- **I want to know what is proven:** read [testing and validation](docs/testing.md).
- **I want simple Office examples:** browse [Office Preview](workloads/office-preview/README.md).
- **I want the representative Knowledge Worker adaptations:** browse [Knowledge Worker Multi-Monitor](workloads/knowledge-worker-multimonitor/README.md).

## What the Preview does

The dependency-free `netstandard2.0` DLL discovers active monitors, orders the primary first, allocates durable/base application windows round robin, moves and optionally maximizes them, verifies the destination, and advances persistent state only after success. Workloads keep ownership of application launch, correct window identification, interaction, timing, and cleanup.

The reflection-friendly API is:

- `PlaceNext` — allocate the next monitor, verify placement, then advance state;
- `PlaceLastUsed` — reapply the last allocated monitor without advancing;
- `PlaceOnMonitor` — reapply a specified monitor index without advancing;
- `ResetState` — deliberately initialize a fresh sequence.

State is stored at `%TEMP%\LoginPI\MultiMonitor\state.txt` as `MonitorCount` and `LastUsedIndex`. No HWND or monitor handle is persisted. A **durable/base window** is the long-lived top-level application window the workload actually intends to exercise. Splash screens, setup UI, dialogs, Outlook compose/read/reminder windows, popups, child windows, and temporary launchers never allocate.

Start/open workloads allocate exactly once. Later Run workloads use non-allocating maintenance only when justified. Preparation and Close workloads do not touch placement state. Placement belongs outside EUX/application-response timers wherever practical.

## Validation status

| Capability | Evidence |
| --- | --- |
| Generic DLL loading, ScriptContent delivery, all Prepare branches | Runtime-proven in Login Enterprise 6.8.6 |
| Physical two-monitor round robin and cross-workload state | Runtime-proven |
| Generic Prepare -> Open/Place -> Close and cleanup | Runtime-proven in Desktop Connector Console / NoRemote |
| Generic Notepad / Paint / Edge flow | Runtime-proven |
| Office Preview workloads | Generated/build-tested/static-validated; partner-lab runtime validation pending |
| Representative Knowledge Worker adaptations | Generated/build-tested/static-validated; partner-lab runtime validation pending |
| Other LE/Office/Windows versions, VDI protocols, 3+ physical displays, mixed DPI | Pending unless specifically recorded in [testing](docs/testing.md) |

Automated checks do not simulate Login Enterprise or prove durable application behavior.

## Partner/Test Lab Files

- [Preview DLL](dist/LoginVSI.MultiMonitor.dll)
- [DLL SHA-256](dist/SHA256SUMS.txt)
- [Prepare workload](workloads/dll-backed/00-Prepare-MultiMonitor.cs)
- [Office Preview workloads](workloads/office-preview/)
- [Knowledge Worker adaptations](workloads/knowledge-worker-multimonitor/)
- [Test-lab quickstart](docs/test-lab-quickstart.md)

Upload the DLL to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`; Prepare stages it to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`. The repository is directly usable, or `scripts/New-TestLabBundle.ps1 -Zip` can create an ignored convenience bundle under `artifacts/`.

## Validate the repository

```powershell
.\scripts\Test-Repository.ps1
```

Use `-Fast` during editing for integrity and static contracts. The full command restores, builds, runs all tests, verifies protected evidence, checks public safety and workload contracts, and validates the distributable checksum. See [testing](docs/testing.md).

## Safety, support, and license

Never publish raw Login Enterprise Engine logs; they can contain authentication, session, infrastructure, or personal material. See [SECURITY.md](SECURITY.md) and [troubleshooting](docs/troubleshooting.md).

License selection is pending. Public visibility does not grant an open-source license or reuse rights; see [LICENSE.md](LICENSE.md). Product/engineering handoff facts are in [product-handoff.md](docs/product-handoff.md), without private roadmap or planning commitments.

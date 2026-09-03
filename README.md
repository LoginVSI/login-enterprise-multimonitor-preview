# Login Enterprise Multi-Monitor Preview

Login Enterprise workloads often keep durable application windows on one display even when the test session exposes several. This repository provides an unsupported engineering Preview that distributes those windows across active monitors in a deterministic, primary-first round robin.

The Preview discovers monitors, selects the next target, moves and optionally maximizes the durable/base application window, verifies the destination, and advances file-backed state only after success. The workload still owns application launch, profile readiness, correct window identification, business interactions, timers, and cleanup.

> **Preview expectations:** This unsupported engineering Preview is intended for evaluation and feedback. Workloads generated or adapted with the included guidance or AI skill still need review and validation in your own Login Enterprise environment. Application and window behavior, profiles, first-run state, display topology, and other environment-specific factors can affect results. Static validation is useful, but it does not make an AI-assisted adaptation automatically correct or supported.
>
> During the Preview, please share issues, unexpected behavior, and feedback with the Login VSI product team in the [Login VSI Customer Slack](https://join.slack.com/t/lvsi-customers/shared_invite/zt-3acoc4xmq-NcLJT33APZwrZrcppl8YQw) so the approach can be improved. There is currently no committed date for this capability to become a supported native Login Enterprise feature.

It is useful when a multi-display Login Enterprise session should exercise Word, Excel, PowerPoint, Outlook, Edge, or another compatible application on more than the primary display. It does not configure Windows displays, create application profiles, automate first-run setup, guarantee every application/version/session combination, or provide a supported DLL distribution channel.

## Choose your path

- **Quick evaluation:** [test-lab quickstart](docs/test-lab-quickstart.md)
- **From-zero setup and concepts:** [getting started](docs/getting-started.md)
- **Manual workload adaptation:** [adapt your own workload](docs/adapt-your-own-workload.md)
- **AI-assisted adaptation:** [copy/paste agent prompt and guide](docs/agentic-workload-adaptation.md)
- **Implementation and architecture:** [architecture](docs/architecture.md)
- **Testing and current evidence:** [evidence status](docs/evidence-status.md) and [validation method](docs/testing.md)
- **Examples:** [Office Preview](workloads/office-preview/README.md) and [representative Knowledge Worker adaptations](workloads/knowledge-worker-multimonitor/README.md)

## Runtime evidence at a glance

Local Login Enterprise 6.8.6 testing proved DLL staging/loading, two-monitor placement and state, the canonical Prepare -> Open/Place -> Close lifecycle, Word/Excel/PowerPoint, corrected Edge, and New Outlook launch/find/place.

In an external partner lab, the representative two-monitor Knowledge Worker Application Test passed 7/7 actions with zero failures. Its allocating sequence was Classic Outlook `0`, Edge `1`, Excel `0`, PowerPoint `1`, and Word `0`; Edge Run maintained the existing Edge target. Partner testing also demonstrated Word `0`, Excel `1`, and PowerPoint `2` across three monitors. Multi-loop Load/Continuous resilience and the external session-metrics comparison remain in progress.

These are bounded results, not broad compatibility claims. [Evidence status](docs/evidence-status.md) is the source of truth, including the Outlook environment distinction and the observed EUX reporting interruption that has not been tied to this Preview.

## Allocation contract

The reflection-friendly API exposes `PlaceNext`, `PlaceLastUsed`, `PlaceOnMonitor`, and `ResetState`.

- A Start/open workload calls `PlaceNext` once after it identifies the long-lived durable/base window.
- A later Run workload may call `PlaceLastUsed` or `PlaceOnMonitor` to maintain that same target without allocating again. `PlaceLastUsed` reapplies the single global `LastUsedIndex`, so it equals the Start target only while no other application has allocated in between.
- Preparation and Close workloads do not allocate or reset placement state.
- Splash screens, setup UI, dialogs, Classic Outlook compose/read/reminder windows, popups, child windows, and temporary launchers do not allocate.

State is stored at `%TEMP%\LoginPI\MultiMonitor\state.txt` as `MonitorCount` and `LastUsedIndex`. No window or monitor handle is persisted. Keep placement outside EUX, application-response, and performance timers wherever practical.

## Preview files

- [Distributable DLL](dist/LoginVSI.MultiMonitor.dll) and [SHA-256 manifest](dist/SHA256SUMS.txt)
- [Prepare workload](workloads/dll-backed/00-Prepare-MultiMonitor.cs)
- [Quickstart](docs/test-lab-quickstart.md)

The Preview uses existing Login Enterprise ScriptContent. Upload the DLL to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`; Prepare copies it to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`. This is a Preview staging pattern, not a new product distribution mechanism.

## Validate the repository

```powershell
.\scripts\Test-Repository.ps1
```

Use `-Fast` while editing. The full command restores, builds, runs unit and source-contract tests, verifies protected evidence, checks documentation links and public safety, and validates the committed DLL checksum. Automated success does not establish Login Enterprise runtime proof.

Never publish raw Login Enterprise Engine logs. See [security](SECURITY.md), [troubleshooting](docs/troubleshooting.md), and [known limitations](docs/known-limitations.md). License selection is pending; public visibility does not grant open-source reuse rights. See [LICENSE.md](LICENSE.md).

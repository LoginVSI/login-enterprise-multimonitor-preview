# Knowledge Worker 2025 Multi-Monitor Preview

Complete twelve-script KW25 adaptation for evaluation. Generated and statically validated; this package has not been compiled in Script Editor or executed in Login Enterprise. It is an unsupported engineering Preview. Read [source mapping](SOURCE-MAPPING.md) and [manifest](adaptation-manifest.json) before use. Filenames are unchanged. Close workloads are excluded.

Publication is directed by the repository owner. See [publication review](PUBLICATION.md) for provenance and factual licensing notices. Customer runtime validation remains pending.

Use a dedicated standard-user test profile with Classic Outlook, Edge, and the selected Office version installed, licensed, and ready. New Outlook is not a substitute. Expose all intended displays to the Windows test session and configure its primary display before starting. Keep topology fixed throughout the scenario. English UI/title patterns and the original KW25 content endpoints must work. Prepare Edge first-run settings in the user profile. No unrelated Office or Edge windows may be open, and no manual launches should occur during the test.

## Stage the prerequisites

1. Obtain the existing repository [DLL](../../dist/LoginVSI.MultiMonitor.dll) and verify against [SHA256SUMS.txt](../../dist/SHA256SUMS.txt). Do not substitute a rebuilt DLL.
2. Upload it to appliance `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`. Upload the included `LoginVSI_BattlingRobots.bmp` to `/loginvsi/content/scriptcontent/LoginVSI_BattlingRobots.bmp` with that exact filename. For Script Editor/Standalone Engine, use that engine's configured local ScriptContent directory instead.
3. Import the existing [00-Prepare-MultiMonitor.cs](../dll-backed/00-Prepare-MultiMonitor.cs) separately. It copies the DLL to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll` for the executing test user. Default `ForceRefreshMultiMonitorDll = false` retains an existing copy. If updating a stale copy, set it true for preparation, verify the refresh, then return it to false. Restart the session before replacing an already loaded assembly.
4. Import each of the twelve `.cs` files in this directory as a separate application workload, retaining its TARGET. Select only one Office Prepare for the scenario. Do not concatenate scripts: several contain identically named helper classes. The BMP, README, manifest and mapping are not application scripts.
5. Confirm appliance KnownFiles delivery for the original Word document, workbook, presentation, PDF, Outlook PRF and PST. Starts download these into `%TEMP%\LoginEnterprise` or `%TEMP%\LoginPI` as the source specifies. Word, PowerPoint and Outlook Run copy the BMP from ScriptContent. Keep original web access enabled, including the source customer-portal URL. No optional demo MP4 is needed by these sources.

## Configure the scenario

Use Continuous/Load configuration with Run once available. Importing code does not apply lifecycle settings. Set them explicitly:

| Order | Workload | Run once | Leave application running |
| --- | --- | --- | --- |
| 1 | 00-Prepare-MultiMonitor | On | Off |
| 2 | KW25 Prep 365 Microsoft Office OR KW25 Prep 2019 Office | On | Off |
| 3 | KW25 Outlook Start | On | On |
| 4 | KW25 Edge Start | On | On |
| 5 | KW25 Excel Start | On | On |
| 6 | KW25 Word Start | On | On |
| 7 | KW25 PowerPoint Start | On | On |
| 8 | KW25 Outlook Run | Off | On |
| 9 | KW25 Edge Run | Off | On |
| 10 | KW25 Excel Run | Off | On |
| 11 | KW25 Word Run | Off | On |
| 12 | KW25 PowerPoint Run | Off | On |

Repeat only rows 8 through 12 according to test configuration. Do not include Close scripts. DLL preparation and Office preparation serve different purposes; both are required in this order. For a one-pass Application Test, retain the same order/persistence settings; it does not establish repeated-loop semantics.

Start prepares and opens each application once, then allocates its durable window. Run performs the original business actions repeatedly without allocating another monitor. Excel, Word and PowerPoint Run reopen documents and replace earlier windows. Maintenance applies the saved application index to the current durable document after maximize. Outlook and Edge similarly reassert their own assignment. Transient windows and the PowerPoint slideshow retain application-controlled placement.

## Using KW25 Office with an existing KW22 Edge workload

Import the eight KW25 Office Start/Run scripts and select one Office Prepare alternative. Skip `KW25 Edge Start.cs` and `KW25 Edge Run.cs`. Both remain in the complete package for other evaluations.

Run DLL Prepare, the chosen Office Prepare, then Outlook Start, Excel Start, Word Start and PowerPoint Start in that relative order. Repeat Outlook Run, Excel Run, Word Run and PowerPoint Run in that relative order. Office Start settings remain Run once On / Leave application running On; Office Run settings remain Run once Off / Leave application running On. Include the existing KW22 Edge workload according to its own sequence and lifecycle requirements. Its scheduling position and settings require inspection of that workload; they cannot be inferred from this package. Do not change its original automation or assume it allocates a monitor. Do not add Close workloads.

The exact mixed configuration has not been compiled or runtime-tested here. Check whether the existing Edge workload reads, advances or resets Preview state before combining it with Office. If it does not touch Preview state and there are no other allocating workloads, fresh Office-only allocations (Outlook, Excel, Word, PowerPoint) are `0,1,0,1` on two monitors, `0,1,2,0` on three, and `0,1,2,3` on four. These conditional expectations make no claim about Edge's position. If Edge allocates, the actual order of successful allocation calls determines the sequence. An Edge script that resets shared state or relies on global PlaceLastUsed requires separate compatibility review. The customer must validate this combination in Login Enterprise.

## Allocation and verification

Start from a fresh disposable session with no Preview state, or explicitly remove `%TEMP%\LoginPI\MultiMonitor\state.txt` and `KW25-*.txt` while no workloads are executing before a new evaluation. Never reset between Starts or during loops. No package workload resets state. Existing state intentionally continues the previous rotation and will change the initial sequence. After a failed Start, stop and restart the whole evaluation from clean state rather than skipping the failed application or retrying allocation in place.

| Monitors | Outlook, Edge, Excel, Word, PowerPoint |
| --- | --- |
| 1 | 0, 0, 0, 0, 0 |
| 2 | 0, 1, 0, 1, 0 |
| 3 | 0, 1, 2, 0, 1 |
| 4 | 0, 1, 2, 3, 0 |

The five-application table applies to the complete KW25 sequence, including KW25 Edge. Index 0 is primary. Remaining monitors follow the DLL's deterministic ordering. Each Start must report `Success=True`, matching `Target` and `Verified`, correct `MonitorCount`, and `StateAdvanced=True`. Each Run maintenance result must keep its own Start target and report `StateAdvanced=False`. Inspect `ElapsedMs` and `Message`. After all five Starts and throughout their Runs, state must stay at LastUsedIndex 0, 1, or 0 for 2, 3, or 4 displays respectively. App records store monitor count and index on two lines, never handles. Earlier single-index records are rejected; rerun all Starts in a fresh scenario after updating. Run rejects missing/corrupt shared state and mismatched recorded counts before placement. The DLL can reset state when it discovers a monitor-count change; stop on that failure and restart from clean state. Same-count topology changes are not detected by index records. Keep state and records private to the test user's session. Do not reuse them after a topology change or unrelated Preview workloads.

## Validate before evaluation

The customer is responsible for Script Editor compilation and actual scenario execution, and for reporting failures with reviewed diagnostic excerpts. For the full KW25 package, compile all twelve files individually in the intended Script Editor version, then execute Starts and Runs using staged content. In a real scenario, run at least two complete Run loops on each available 1/2/3/4-display topology. Confirm Office replacement documents return to their Start monitor, Edge stays assigned after every restore/maximize, Inbox stays assigned, secondary windows never allocate, and source actions complete. Check foreground behavior, negative-coordinate displays, scaling, first-run dialogs, missing DLL/record failures and startup ambiguity. PowerPoint slideshow display selection remains Office behavior; only its editing window is maintained. Check screen-coordinate clicks on each monitor and measure EUX/cadence impact separately from explicit timers.

The two Prepare scripts are byte-identical and placement-state neutral. All Starts add ownership guards, DLL loading, one allocation and an app-index record. All Runs add maintenance because source actions maximize or replace windows. The sole content change replaces Outlook Run's historical example recipient domains with reserved example.invalid domains for repository public safety; validate offline composition behavior. These are reserved non-deliverable placeholders. If the environment requires resolvable recipients, the customer must substitute approved test addresses locally and review the resulting mail behavior; do not publish those environment-specific addresses. No timers, URLs, business actions or source cleanup routines were changed. See the mapping for retained source limitations.

To restore the unmodified KW25 set, remove these adapted applications from the scenario, import the original twelve in-scope sources from your unchanged source checkout, retain the same Prepare/Start/Run order and persistence settings, and omit 00-Prepare-MultiMonitor. End the old test session first. No original source or existing Preview example was overwritten.

Official reference: [KW25 overview](https://docs.loginvsi.com/support-resources/kb/knowledge-worker-2025-overview), with its linked Office, Edge and preparation pages. Repository validation details are in [VALIDATION.md](VALIDATION.md).

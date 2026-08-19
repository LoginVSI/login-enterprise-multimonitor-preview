# Login Enterprise test-lab quickstart

Use this guide with Login Enterprise 6.8.6 and a normal interactive Connector/test-lab session. The generic Preview framework is runtime-proven on the recorded two-monitor Desktop Connector environment. The Office Preview and Knowledge Worker adaptations are **generated/build-tested/static-validated; partner-lab runtime validation pending**.

## 1. Download the files

From this repository, download:

- [`dist/LoginVSI.MultiMonitor.dll`](../dist/LoginVSI.MultiMonitor.dll)
- [`workloads/dll-backed/00-Prepare-MultiMonitor.cs`](../workloads/dll-backed/00-Prepare-MultiMonitor.cs)
- either the simple [`workloads/office-preview/`](../workloads/office-preview/) examples or the complete [`workloads/knowledge-worker-multimonitor/`](../workloads/knowledge-worker-multimonitor/) adaptations.

Repository files are the source of truth. For Script Editor development, test disposable copies because Script Editor may rewrite line endings or its working representation.

## 2. Upload and stage the DLL

Upload the DLL to the appliance at:

```text
/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll
```

Add `00-Prepare-MultiMonitor.cs` before every DLL consumer. It stages the target-local copy at:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

Missing copies stage automatically. Existing copies are retained while `ForceRefreshMultiMonitorDll` is `false`; set it to `true` only for an intentional remove/copy/verify refresh, then return it to `false`.

## 3. Create the test

Create an Application Test using a normal Connector/test-lab session whose interactive Windows desktop exposes the intended monitor topology. Desktop Connector Console / NoRemote is the proven generic baseline. Do not infer VDI protocol coverage from that result.

For the Office Preview examples, run:

1. Prepare.
2. `office-preview/01-Reset-Placement-State.cs` for a deterministic fresh sequence.
3. Word, Excel, PowerPoint, Outlook, then Edge.

For the complete Knowledge Worker adaptation, run the files in the order and with the enabled/`Run once`/`Leave application running` intent recorded in [`reference/test-scenario/workload-sequence.txt`](../reference/test-scenario/workload-sequence.txt). The adapted files have identical names under `workloads/knowledge-worker-multimonitor/`. Add Prepare before that sequence without editing the preserved transcription.

## 4. Preserve lifecycle semantics

- Application Test provides per-workload `Leave application running`; its default is off.
- Continuous Test and Load Test also provide `Run once`.
- Start workloads that hand an application to a later Run workload must leave it running.
- Run workloads reuse or reassert the Start destination and do not allocate again.
- Close workloads explicitly clean up and do not consume or reset monitor state.
- A process that happens to linger is not a lifecycle contract.

## 5. Expected round robin

The primary monitor is logical index 0. Remaining monitors are ordered by signed desktop coordinates. Starting from `LastUsedIndex=-1`, verified durable application windows receive `0,1,2,...` and wrap. State advances only after placement verifies.

The Office Preview sequence expects `0,1,0,1,0` on two monitors. The complete Knowledge Worker sequence allocates Outlook, Edge Start, Excel, PowerPoint, and Word; Edge Run performs maintenance placement without advancing.

## 6. Capture useful failure evidence

Record:

- Login Enterprise version, test type, session type, Windows/application versions, and monitor topology;
- workload/AppExecution result and application events;
- structured placement result: application, monitor count, target, verified index, state advancement, elapsed time, and message;
- selected durable window title/class/process and whether it was the intended base window;
- `%TEMP%\LoginPI\MultiMonitor\state.txt` before/after when state behavior is relevant;
- cleanup behavior and any existing-window ambiguity.

Engine logs can contain authentication, session, infrastructure, or personal material. Keep raw logs in the ignored `artifacts/` area for local diagnosis and **do not publish them**. Share only a reviewed, minimized excerpt through an approved channel.

## Runtime-status rule

Passing the repository build, source contracts, or GitHub Actions does not prove Login Enterprise behavior. Record partner-lab evidence per workload before changing Office/KW status from generated/static-validated to runtime-proven.

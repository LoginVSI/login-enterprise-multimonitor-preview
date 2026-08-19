# Getting started

The Multi-Monitor Preview separates application knowledge from generic monitor placement. Your workload resolves the correct durable application window; `LoginVSI.MultiMonitor.dll` chooses, moves, verifies, and records its monitor.

## First lab path

1. Read the [Preview status and limitations](known-limitations.md).
2. Follow the [test-lab quickstart](test-lab-quickstart.md).
3. Begin with the [Office examples](../workloads/office-preview/README.md) or the already-proven generic flow under `workloads/dll-backed/`.
4. Use [adapt your own workload](adapt-your-own-workload.md) for manual integration or [agentic workload adaptation](agentic-workload-adaptation.md) for AI-assisted integration.
5. Run `.\scripts\Test-Repository.ps1` before sharing changes.

The appliance DLL source is `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`. Prepare stages it to `%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll`; state lives beside it as `state.txt`.

The generic framework is runtime-proven in the recorded Login Enterprise 6.8.6 environment. Office and representative Knowledge Worker workloads are generated/build-tested/static-validated and still require partner-lab runtime evidence.

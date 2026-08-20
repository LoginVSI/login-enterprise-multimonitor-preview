# Product and Development handoff

This public technical handoff accompanies, but does not replace, any separately approved product requirements. The implementation is an unsupported Preview.

## Delivered implementation

- Generic dependency-free `netstandard2.0` placement DLL and reflection-friendly API.
- Persistent primary-first round robin with verified-success advancement and recovery.
- Runtime-proven ScriptContent preparation and canonical generic three-workload lifecycle.
- Five-application Office Preview example set.
- Complete ten-file minimal-delta Knowledge Worker/KW25 adaptation mapped to immutable originals.
- One authoritative repository validation command and Windows GitHub Actions CI.
- Public manual and agentic adaptation pathways, test-lab quickstart, troubleshooting, checksum, and explicit runtime-status boundaries.

## Proven behavior

Login Enterprise 6.8.6 evidence covers Script Editor/Standalone Engine loading, appliance delivery, all Prepare branches, physical two-monitor placement, serial Desktop Connector execution, cross-workload state, missing-state recovery, canonical scenario-controlled Open/Place-to-Close handoff, and generic cleanup.

Office Preview Word/Excel/PowerPoint passed on one local Login Enterprise 6.8.6 machine. Corrected Edge and classic Outlook remain runtime-pending. Knowledge Worker remains generated/build-tested/static-validated with partner-lab validation pending. Automated checks do not establish untested application/window compatibility.

## Architecture decisions

- Workloads own launch, durable-window discovery, sequencing, interactions, and timer boundaries.
- The generic DLL owns monitor discovery/order, state, native placement, verification, and results.
- Durable base windows allocate once; secondary/transient windows never allocate.
- Start allocates; Run uses non-allocating maintenance; preparation and Close do not access placement state.
- State stores monitor count/index only; native handles never cross workload files.
- Preview staging retains an existing target-local DLL unless forced refresh is explicitly enabled.

## Repository consumption

Use [test-lab-quickstart.md](test-lab-quickstart.md). Upload `dist/LoginVSI.MultiMonitor.dll`, run `workloads/dll-backed/00-Prepare-MultiMonitor.cs`, then choose the Office Preview or complete Knowledge Worker adaptations. Preserve the scenario transcription's order and lifecycle intent; do not modify preserved evidence.

The Knowledge Worker `adaptation-manifest.json` records source, type, allocation, durable-window method, intentional changes, public-safety substitutions, and reviewed line-delta budget. Static checks protect original hashes, ordered timer calls, targets, classes, allocation counts, Run maintenance, Close neutrality, and substitution disclosure.

## Remaining validation/product work

- Partner-lab runtime evidence for Word, Excel, PowerPoint, Outlook, Edge Start/Run, and all Close behavior.
- Application/version/localization/first-run and existing-instance behavior.
- Mixed DPI, broader/negative physical topologies, concurrency, cadence overhead, VDI protocols, and other Login Enterprise releases.
- Supported compatibility, signing, distribution/update ownership, state versioning, diagnostics policy, and support posture.
- License approval. Public readability currently grants no open-source license.

No GA, support, release, compatibility, or delivery commitment is implied.

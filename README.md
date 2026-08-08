# Login Enterprise Multi-Monitor Preview

This repository is the public engineering home for an early, unsupported Login Enterprise Multi-Monitor Preview. It is under active development. It does not represent a generally available feature, a support commitment, or a delivery commitment.

The goal is a reusable mechanism that can distribute representative application activity across available displays for broadly compatible Login Enterprise C# workloads. Knowledge Worker workloads will validate that generic mechanism; the architecture must not be coupled to Office, a browser, or a single workload set.

## Repository purposes

1. Active engineering and exploration of multi-monitor behavior.
2. Public Preview workload delivery, examples, documentation, and intentional distributables.
3. Technical companion material for a future product-management PRD and development handoff. The formal PRD lives elsewhere.

## Current status

Only repository scaffolding, guardrails, reference locations, a scenario reference, and hygiene helpers exist. No final implementation, reusable DLL, integrated workload, or validated Preview behavior is present.

## Directory map

- `reference/original-workloads/`: immutable, known-good baseline workloads supplied later; integrity is tracked with SHA-256.
- `reference/proven-pocs/`: previously successful proof-of-concept source supplied later and retained as evidence.
- `reference/login-enterprise-docs/`: supplied scripting/metalanguage documentation and representative examples; the primary API source of truth.
- `reference/test-scenario/`: preserved scenario ordering and configuration evidence.
- `workloads/script-only/`: self-contained Preview experiments before DLL complexity.
- `workloads/dll-backed/`: workloads using the future reusable managed library.
- `workloads/integrated/`: adapted copies of complete representative workloads. Originals are never edited.
- `src/LoginVSI.MultiMonitor/`: reserved reusable implementation source.
- `tests/`: future pure-logic and integration-support tests.
- `skills/login-enterprise-multimonitor/`: draft, unvalidated AI workflow instructions.
- `docs/`: architecture, testing, limitations, history, requirements context, and handoff material.
- `scripts/`: reference-integrity and public-repository hygiene helpers.
- `dist/`: intentional public Preview distributables only.
- `artifacts/`: ignored local logs, screenshots, state, diagnostics, and build scratch; only `.gitkeep` is tracked.

## Implementation principles

Use documented Login Enterprise scripting/metalanguage functionality first, compatible .NET/C# second, and native Windows APIs only where the preceding layers do not provide the required behavior. Never invent a Login Enterprise API; confirm exact functions against supplied documentation.

Application-specific code owns launch, interaction, correct main-window identification, sequencing, measurement boundaries, and the placement call site. Reusable code owns monitor discovery and ordering, persistent round-robin state, selection, native placement, suitable restore/maximize behavior, verification, and result/error information.

## Validation model

Login Enterprise Script Editor and its standalone runner validate compilation and individual-workload behavior. They do not prove state or behavior across independent workload executions. Cross-workload state, Start/Run relationships, complete sequencing, and end-to-end behavior require an actual Login Enterprise test scenario. See [docs/testing.md](docs/testing.md).

## Reference protection

Never edit, rename, delete, reformat, modernize, or add Preview logic directly to files in `reference/original-workloads/`. Create adaptations under `workloads/` and run `scripts/Verify-ReferenceHashes.ps1 -Verify` before and after major implementation passes once the owner has established the manifest.

## License and support

License selection is pending. This unsupported Preview makes no GA, support, compatibility, or delivery claim.

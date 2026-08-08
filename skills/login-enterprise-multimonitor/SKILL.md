---
name: login-enterprise-multimonitor
description: Draft workflow for adapting compatible Login Enterprise C# workloads to the Multi-Monitor Preview while preserving behavior, sequencing, and measurement integrity. Use when inspecting or adapting supplied Login Enterprise workloads, applying the approved reusable multi-monitor implementation, or validating script-only, DLL-backed, integrated, and cross-workload scenario behavior.
---

# Login Enterprise Multi-Monitor Preview

Status: **DRAFT / UNVALIDATED**. The final API is not stable, the implementation is not stable, and this skill has not been validated. Do not treat placeholders as implementation instructions or claims.

## Required workflow

1. Read the root `README.md`, `AGENTS.md`, relevant `docs/`, and every applicable supplied reference.
2. Verify `reference/original-workloads/` hashes before a major adaptation pass. Never modify immutable originals.
3. Inspect the complete workload: application launch, interaction, correct application/main-window identification, sequencing, persistent relationships, and every timer boundary.
4. Read the complete supplied scripting/metalanguage documentation and representative examples. Prefer documented Login Enterprise functionality, then compatible .NET/C#, then native Windows/P/Invoke. Never invent an API.
5. Read `product-context.md` for intended value and boundaries. Read `implementation-guidance.md` only as a TBD design checklist until an approved implementation is documented.
6. Use the approved reusable implementation after its API stabilizes. Keep application-specific responsibilities in the workload and generic monitor/state/placement responsibilities in reusable code.
7. Adapt an immutable original only by creating a new file in the appropriate `workloads/` directory. Preserve application behavior, conservative compatible syntax, sequencing, and workload intent.
8. Keep placement outside EUX, application-response, and performance measurements wherever practical. Never silently move timer boundaries or alter cadence.
9. Use `reference/test-scenario/workload-sequence.txt` when adapting the complete representative set. Preserve the reference; record experimental variants elsewhere.
10. Validate with `validation-guidance.md`. Distinguish Script Editor evidence from actual sequential Login Enterprise scenario evidence and VDI evidence.
11. Verify reference hashes again, run public-safety checks, update documentation/statuses, and retain useful evidence without publishing sensitive material.

Do not claim generated work is proven. Report failures and placement results explicitly, and avoid third-party runtime dependencies or administrator requirements.

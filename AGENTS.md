# Repository instructions

## General

- Read `README.md`, relevant `docs/`, and all supplied reference material before implementation.
- Prefer repository evidence over assumptions and retain useful complete context.
- Do not invent Login Enterprise APIs. Confirm exact names and semantics in supplied documentation and proven workloads.
- Do not claim a build or test passed without recorded evidence from that environment.
- Update documentation when behavior, decisions, validation status, or limitations change.
- Use public-safe, generic Preview language only.

## API priority

1. Documented Login Enterprise scripting/metalanguage functions.
2. Compatible normal .NET/C#.
3. Native Windows APIs and P/Invoke only when the first two layers are insufficient.

Do not bypass an appropriate Login Enterprise capability merely because managed or native alternatives exist.

## Workload constraints

- Login Enterprise workloads derive from `LoginPI.Engine.ScriptBase`; execution originates from private `Execute()`.
- Normal compatible helper methods, classes, and control flow may be used.
- `IWindow.NativeWindowHandle` may bridge Login Enterprise windows to generic native placement logic.
- Preserve conservative syntax demonstrated by supplied workloads.
- A locally installed .NET SDK does not establish Script Editor compatibility.
- Target standard-user, non-administrator execution and avoid third-party runtime dependencies.

## Test model

- Script Editor/standalone runner validates an individual workload.
- An actual Login Enterprise scenario validates sequential and cross-workload behavior.
- Never conflate a multi-phase single script with multiple independent workload executions.

## Architecture boundary

Application-specific workload code owns application launch and interaction, correct main-window identification, sequencing, existing measurement/timer boundaries, and the decision to invoke placement. Generic multi-monitor code owns monitor discovery, primary detection and primary-first ordering, persistent round-robin state, next-monitor selection, native placement, appropriate restore/maximize handling, verification, and result/error information.

Do not hard-code the reusable architecture around any particular application.

## Timing

- Placement adds runtime overhead; keep it outside EUX, application-response, and performance timers wherever practical.
- Never silently move measurement boundaries or change cadence.
- Preserve the original workload's intent.

## Reference protection

- Never modify, rename, delete, reformat, or modernize `reference/original-workloads/` contents.
- Put every adaptation in a new file under `workloads/`.
- Verify reference hashes before and after major implementation passes.
- Preserve `reference/test-scenario/workload-sequence.txt`; do not silently change its order or settings.
- Treat `reference/proven-pocs/` as implementation evidence, not style-cleanup targets.
- Treat `reference/login-enterprise-docs/` as the primary source of truth for Login Enterprise APIs.

## Public safety

- Do not add identities, private communications or URLs, credentials, secrets, confidential environments, internal discussions, roadmap commitments, customer-specific branding, or unsupported product claims.
- Clearly label behavior as proven, generated/not validated, planned, or a future productization possibility.
- Run `scripts/Test-PublicSafety.ps1` and perform human review before substantial completion.

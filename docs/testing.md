# Testing and validation

Nothing produced by the scaffolding pass is validated implementation behavior.

## Evidence statuses

- **Planned**: intended but not executed.
- **Generated / not validated**: produced without the required runtime evidence.
- **Proven in Script Editor**: compiled and executed as an individual workload in Script Editor/standalone runner.
- **Proven in full Login Enterprise test**: exercised across the required actual sequential scenario.
- **Proven in VDI**: exercised in a documented VDI environment.

State the environment, inputs, topology, build/source identity, outcome, and evidence location whenever assigning a proven status.

## Two validation environments

### Script Editor / standalone script runner

Use for workload compilation, single-workload execution, application launch and window identification, individual placement, DLL-loading experiments, debugging, and logs. One workload containing several internal phases does not prove behavior across independent workload executions.

### Actual Login Enterprise test scenario

Use when multiple workloads must execute in sequence. This is required for persistent state across separate files, Start/Run relationships, complete Knowledge Worker sequencing, cross-workload application behavior, and end-to-end Login Enterprise execution. A suitable local multi-monitor workstation may host the test.

## Planned validation progression

Follow the ladder in `skills/login-enterprise-multimonitor/validation-guidance.md`. Preserve the scenario in `reference/test-scenario/workload-sequence.txt`; record experimental variants elsewhere. Run reference hash verification before and after major implementation passes.

## Measurement evidence

Verify that placement remains outside existing EUX, application-response, and performance timers wherever practical. Record any unavoidable overhead or boundary change explicitly; never silently redefine the workload's measurement intent.

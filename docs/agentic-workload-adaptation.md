# AI-assisted workload adaptation

Codex, Claude Code, or another capable coding agent can adapt a Login Enterprise C# workload when it has the complete source, this repository, and clear lifecycle constraints. Human review is still required for application ownership, Login Enterprise syntax, timer placement, scenario settings, content changes, and runtime proof. For an Outlook source, the agent must first classify it as Classic Outlook or New Outlook. Merely changing `TARGET` or an executable is not a valid conversion; changing Outlook flavors is a substantive adaptation.

An agent can accelerate adaptation, but its output is not automatically correct or supported. Review the generated workload and validate it in the intended Login Enterprise scenario before relying on it.

When the tool supports repository skills, [`skills/login-enterprise-multimonitor/SKILL.md`](../skills/login-enterprise-multimonitor/SKILL.md) is the authoritative reusable workflow. It routes the agent to focused implementation, validation, and product-context guidance rather than duplicating a large instruction set in one file.

## Copy/paste prompt for a coding agent

Replace every angle-bracket field before submitting this prompt. Remove permissions you do not want to grant.

```text
You are working in the Login Enterprise Multi-Monitor Preview repository.

Repository root:
<REPO_ROOT>

Source workload(s):
- <SOURCE_PATH_1>
- <SOURCE_PATH_2_OR_REMOVE>

Write adapted workload(s) to:
- <DESTINATION_PATH_1>
- <DESTINATION_PATH_2_OR_REMOVE>

Scenario type:
<Application Test | Continuous Test | Load Test>

Lifecycle classification for each source:
<Prepare | Start/Open | Run | Close | Single-file lifecycle>

Application and expected process/window:
<APPLICATION, TARGET, PROCESS, WINDOW TITLE/CLASS IF KNOWN>

Source scenario settings and relationships:
- Run once: <ON/OFF/NOT APPLICABLE>
- Leave application running: <ON/OFF/NOT APPLICABLE>
- Start/Run/Close relationship and order: <DETAILS>

Required monitor behavior:
<FOR EXAMPLE: allocate the durable Word document window once in Start; later
Run activity must stay on that allocation without advancing round-robin state>

Known application/environment quirks:
<FIRST-RUN, PROFILE, PROCESS HANDOFF, EXISTING WINDOWS, REPLACEMENT WINDOW,
SELF-REPOSITIONING, CONTENT OR MEDIA PREREQUISITES>

Preservation requirement:
Preserve existing application behavior and all timers/EUX measurement boundaries
exactly unless a change is explicitly approved below.

Approved substitutions or behavior changes:
<NONE, OR AN EXPLICIT LIST>

Permissions:
- Run repository validation: <YES/NO>
- Edit supporting docs/contracts: <YES/NO>
- Git outcome: <PRODUCE DIFF ONLY | COMMIT LOCALLY | COMMIT AND PUSH>

Goal:
Adapt the supplied Login Enterprise C# workload(s) to use this repository's
Multi-Monitor Preview without changing their business or application behavior.

Before editing:
1. Read AGENTS.md, README.md, docs/adapt-your-own-workload.md,
   docs/agentic-workload-adaptation.md, skills/login-enterprise-multimonitor/SKILL.md,
   its linked implementation-guidance.md and validation-guidance.md, relevant
   examples, supplied Login Enterprise documentation, proven workloads, mapping
   records, and every complete source workload.
2. Verify protected reference and preserved-evidence hashes before and after the
   implementation pass. Never edit protected originals or proven evidence.
3. Classify every workload as Prepare, Start/Open, Run, Close, or single-file
   lifecycle. Trace process handoff, existing-instance ambiguity, application
   interactions, timer boundaries, scenario order, cleanup, and persistence.
4. Identify the durable/base application window. Do not assume the spawned PID owns
   the durable UI. Document the title/class/process/HWND strategy and any replacement
   or self-repositioning behavior.

Implementation requirements:
1. Preserve TARGET, the primary script class, launch intent, original interactions,
   content, and URLs except for explicitly approved substitutions.
2. Preserve timer names, timer order, EUX/application-response measurement boundaries,
   cadence, scenario sequencing, Run once, Leave application running, and cleanup
   intent. Keep placement outside measured boundaries wherever practical.
3. Use the staged DLL at
   %TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll and the current repository
   reflection API. Do not invent a different placement or distribution implementation.
4. PlaceNext exactly once in Start/Open, and only after the durable/base window is
   known. Treat verified success as consuming the allocation.
   Placement determines where the application window is located. Focus determines
   which application the workload is actively using. After successful placement,
   use IWindow.Focus() on that already-resolved durable/main window when the next
   interaction requires it in the foreground or foreground visibility is intentional.
   Prefer this Login Enterprise method over custom Win32 foreground-management code.
   Focus is optional workload behavior, not a DLL responsibility or a condition of
   placement success. Do not treat focus failure/absence as placement failure or add
   focus after every placement. Preserve original focus/lifecycle/interaction semantics.
5. Keep splash screens, first-run/setup UI, dialogs, popups, compose/read/reminder
   windows, child windows, and other secondary/transient windows non-allocating.
6. For Run or later maintenance, reacquire the durable/base window and use
   PlaceLastUsed or PlaceOnMonitor. Never call PlaceNext for maintenance.
7. Prepare only stages prerequisites. Close performs bounded cleanup and must not
   allocate, reset, or alter placement state.
8. Distinguish Classic Outlook from New Outlook before editing. Do not convert one by
   changing only TARGET or the executable. Preserve the matching profile, control,
   window, and interaction lifecycle, or report the unsupported gap.
9. Handle Edge/Chromium process handoff and existing-window ambiguity explicitly.
10. Log and evaluate the structured placement result, including Success,
    MonitorCount, TargetMonitorIndex, VerifiedMonitorIndex, StateAdvanced, and Message.
    Fail clearly rather than silently claiming placement.
11. Create or update a source-to-adaptation mapping/delta record with lifecycle,
    durable-window method, allocation behavior, content changes, timer impact,
    scenario settings, cleanup, and runtime status.

Validation and final response:
1. Run .\scripts\Test-Repository.ps1 if permission above is YES. Also run the
   repository's protected-hash checks, git diff --check, and relevant focused checks.
2. Report every file changed and the reason, what was preserved, every approved or
   unavoidable delta, and the exact static/build results.
3. Distinguish static validation, Script Editor compilation, individual runtime
   execution, and actual Login Enterprise scenario evidence. Do not claim runtime proof
   from compilation, unit tests, source contracts, or GitHub Actions.
4. Give exact human runtime steps still required: files and order, scenario type,
   Run once and Leave application running settings, expected monitor sequence/state,
   durable-window checks, secondary-window non-allocation checks, application events,
   timing, and cleanup.
5. Label generated or statically validated adaptations as not runtime-proven until
   the named Login Enterprise environment and scenario pass.
6. Follow the requested Git outcome. Do not commit or push when asked for a diff only.
```

## Filled example

This shorter request relies on the full requirements above remaining in the conversation:

```text
Use the full Multi-Monitor Preview adaptation prompt above.

Repository root:
C:\repos\login-enterprise-multimonitor-preview

Source workload:
workloads/source/MyWordWorkload.cs

Destination:
workloads/custom/MyWordWorkload-MultiMonitor.cs

Scenario type: Application Test
Lifecycle: Single-file lifecycle
Application/process: Microsoft Word, TARGET winword, process winword
Run once: OFF
Leave application running: OFF
Expected behavior: allocate the durable Word document window once after the existing
open timer, preserve all document interactions and timer boundaries, and do not place
open/save dialogs.
Known quirks: fail if a pre-existing durable Word window makes ownership ambiguous.
Approved substitutions: none.
Permissions: run repository validation YES; edit supporting mapping/contracts YES;
produce a diff only and do not commit.
```

## Multi-file Start/Run/Close scenarios

List every related file and its exact scenario order in one request. Tell the agent which Start/Open file owns the single `PlaceNext`, which Run files should use `PlaceLastUsed` or `PlaceOnMonitor`, and which Close file owns cleanup. Include the current `Run once` and `Leave application running` settings rather than asking the agent to infer them from filenames.

Ask the agent to prove that Run maintenance reports `StateAdvanced=False`, Close leaves state untouched, and no HWND or monitor handle crosses workload files. If the application replaces its base window between Start and Run, require the agent to document how the later file safely reacquires the intended durable window.

## Review the result

Compare the generated mapping against the complete source. Check `TARGET`, class name, interactions, URLs/content, ordered timer calls, EUX boundaries, lifecycle settings, and cleanup. Run the actual workload through Script Editor and the intended Login Enterprise scenario. Use [manual adaptation](adapt-your-own-workload.md), [testing](testing.md), and [evidence status](evidence-status.md) as the review contract.

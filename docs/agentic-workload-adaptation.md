# Agentic workload adaptation

This repository includes tool-neutral AI/coding-agent guidance for adapting Login Enterprise C# workloads without rewriting their intent. The authoritative agent instructions are in [`skills/login-enterprise-multimonitor/`](../skills/login-enterprise-multimonitor/).

An agent should read the supplied workloads, repository docs, Login Enterprise reference, preserved examples, and adaptation manifest; classify each lifecycle; identify the durable base window; preserve application behavior, content, timers, measurements, scenario settings, and cleanup; insert one Start/open allocation; use non-allocating Run maintenance; keep secondary windows non-allocating; and produce a mapping/delta record.

For Outlook, the agent must first classify the source as Classic Outlook or New Outlook. It must use the matching evidence-supported launch/window adapter and review all controls, identifiers, navigation, and lifecycle assumptions for that flavor. Merely changing `TARGET` or an executable is not a valid conversion; a Classic-to-New request is a substantive adaptation whose interaction equivalence and timer boundaries require New Outlook-specific evidence and runtime validation.

Example request:

> Adapt these Login Enterprise C# workloads for the Multi-Monitor Preview. Preserve existing workload behavior and measurements, allocate only the durable base application window, run the repository validation suite, and tell me what still needs runtime validation.

The agent must run `.\scripts\Test-Repository.ps1`, label generated output as **not runtime-proven**, and give the human exact Script Editor and platform scenario steps still required. No specific AI model or coding tool is required.

Human review remains mandatory for application ownership, timer placement, content substitutions, Login Enterprise syntax, scenario lifecycle, and public safety. The manual contract in [adapt your own workload](adapt-your-own-workload.md) and the repository skill must agree.

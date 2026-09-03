# Adapt your own workload manually

This guide is for a technical Login Enterprise user who wants to add Multi-Monitor Preview placement without using an AI coding agent. Keep the source workload unchanged and create the adaptation as a new file under `workloads/`.

## 1. Understand the source before editing

Classify each file as Prepare, Start/Open, Run, Close, or a single-file lifecycle. Trace its `TARGET`, launch method, application interactions, URLs/content, timers and EUX measurements, cadence, cleanup, `Run once`, and `Leave application running` intent.

Identify the long-lived durable/base application window. The process returned at launch may be a short-lived bootstrapper, especially for Edge and other Chromium applications. A splash screen, first-run/setup UI, open/save dialog, popup, child window, or Outlook compose/read/reminder window is not the base window.

For Outlook, determine the flavor first. Classic Outlook and New Outlook use different executable, window, control, and lifecycle models. Classic Outlook uses `outlook.exe`, process `OUTLOOK`, and Classic controls such as `rctrl_renwnd32`. New Outlook uses `olk` and a different UI/lifecycle model. Do not silently substitute one for the other. Remember that launch success does not prove interaction compatibility, and changing only `TARGET` or the executable is not an adaptation.

## 2. Preserve the original workload contract

Retain the original `TARGET`, primary script class, launch intent, interactions, content and URLs, timer names and boundaries, EUX measurements, sequencing, cadence, and cleanup unless a specific substitution has been approved. Record every intentional delta.

Placement adds runtime overhead. Put it after the durable window is known and outside EUX, application-response, and performance timer boundaries wherever practical. Never silently move an existing boundary.

## 3. Stage the current Preview DLL

Upload [`dist/LoginVSI.MultiMonitor.dll`](../dist/LoginVSI.MultiMonitor.dll) to `/loginvsi/content/scriptcontent/LoginVSI.MultiMonitor.dll`, then run [`workloads/dll-backed/00-Prepare-MultiMonitor.cs`](../workloads/dll-backed/00-Prepare-MultiMonitor.cs). Consumers load:

```text
%TEMP%\LoginPI\MultiMonitor\LoginVSI.MultiMonitor.dll
```

They use state at `%TEMP%\LoginPI\MultiMonitor\state.txt`. Do not paste a separate Win32 implementation into the adapted workload.

## 4. Before and after

A small source workload may look like this:

```csharp
// TARGET:winword
using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;

public class MyWordWorkload : ScriptBase
{
    void Execute()
    {
        StartTimer("Open_Document");
        IWindow word = START(mainWindowTitle: "*", processName: "winword");
        StopTimer("Open_Document");

        word.TypeText("Existing business interaction");
        STOP();
    }
}
```

The adapted shape keeps the target and interaction intact, loads the staged DLL, and allocates only after `START` returns the durable base window:

```csharp
// TARGET:winword
using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class MyWordWorkload : ScriptBase
{
    private Type _placer;
    private string _statePath;

    void Execute()
    {
        LoadMultiMonitorPreview();

        StartTimer("Open_Document");
        IWindow word = START(mainWindowTitle: "*", processName: "winword");
        StopTimer("Open_Document");

        // Allocate once, after the durable base window exists and after the timer.
        PlaceAndRequireSuccess("PlaceNext", word, "Microsoft Word", null);

        word.TypeText("Existing business interaction");
        STOP();
    }

    private void LoadMultiMonitorPreview()
    {
        string previewDirectory = Path.Combine(
            GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(
            previewDirectory, "LoginVSI.MultiMonitor.dll");
        _statePath = Path.Combine(previewDirectory, "state.txt");

        if (!FileExists(assemblyPath))
        {
            ABORT("Multi-Monitor Preview DLL is missing at " + assemblyPath +
                ". Run 00-Prepare-MultiMonitor first.");
        }

        _placer = Assembly.LoadFrom(assemblyPath).GetType(
            "LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
    }

    private int PlaceAndRequireSuccess(
        string methodName, IWindow window, string applicationName, int? monitorIndex)
    {
        object[] arguments = monitorIndex.HasValue
            ? new object[] { window.NativeWindowHandle, applicationName, _statePath,
                monitorIndex.Value, true, 350 }
            : new object[] { window.NativeWindowHandle, applicationName, _statePath,
                true, 350 };

        object result = _placer.GetMethod(
            methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, arguments);

        Log(applicationName + ": Success=" + Property(result, "Success") +
            ", MonitorCount=" + Property(result, "MonitorCount") +
            ", Target=" + Property(result, "TargetMonitorIndex") +
            ", Verified=" + Property(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + Property(result, "StateAdvanced") +
            ", Message=" + Property(result, "Message"));

        if (!(bool)Property(result, "Success"))
        {
            ABORT("Multi-Monitor Preview placement failed: " +
                Property(result, "Message"));
        }

        return Convert.ToInt32(Property(result, "TargetMonitorIndex"));
    }

    private static object Property(object result, string name)
    {
        return result.GetType().GetProperty(
            name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }
}
```

Treat this as a minimal pattern, not a drop-in replacement for application-specific window discovery. Current repository workloads contain compiler-checked reflection examples and more defensive application ownership handling.

## 5. Choose allocation or maintenance

| Lifecycle/window | Call | Advances state? |
| --- | --- | --- |
| Start/Open durable base window | `PlaceNext` exactly once | Yes, after verified success |
| Single-file lifecycle durable base window | `PlaceNext` exactly once | Yes, after verified success |
| Run workload reacquiring its Start window | `PlaceLastUsed` | No |
| Later base-window reassertion with a recorded index | `PlaceOnMonitor` | No |
| Dialog, popup, compose/read/reminder, child, or secondary window | No placement by default | No |
| Prepare or Close | No placement or reset | No |

A secondary window should not allocate unless it is intentionally the independently exercised durable application surface. Most dialogs and transient windows should stay under application and Windows placement control.

For a Start/Run pair, keep the monitor index returned by Start only within the same workload if useful. Across independent files, reacquire the durable window and use `PlaceLastUsed`; never persist an HWND or monitor handle. `PlaceLastUsed` reapplies the global `LastUsedIndex`, so it matches the Start target only when no other application allocated in between; otherwise the Run workload needs its own record of the Start index for `PlaceOnMonitor`. Reassert after a known application restore, maximize, focus, or replacement behavior only when runtime evidence justifies it.

## 6. Scenario lifecycle

Use `Leave application running: ON` when Start/Open hands the application to Run or Close. Preserve the source's `Run once` semantics in Continuous and Load Tests. Close should perform bounded explicit cleanup and must not allocate, reset, or touch placement state.

Application/profile readiness remains in the workload. For example, the representative Classic Outlook adaptation retains its PRF/PST/import and first-run handling, while the simple Office example assumes a configured profile. Generic monitor discovery and placement cannot replace either lifecycle.

## 7. Validate and record the delta

Create a source-to-adaptation note that records source file, destination file, lifecycle, durable-window method, allocation/maintenance calls, content substitutions, timer impact, scenario settings, and runtime status.

Run:

```powershell
.\scripts\Test-Repository.ps1
```

Then compile and run a disposable copy in Script Editor. Finally, run the actual intended Application Test, Continuous Test, or Load Test through its Connector/session. Record structured placement results and prove that secondary windows did not advance state. Static checks and compilation are not runtime proof. See [testing](testing.md), [evidence status](evidence-status.md), and [troubleshooting](troubleshooting.md).

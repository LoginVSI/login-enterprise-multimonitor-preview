// TARGET:msedge.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class DllMultiMonitorPreviewOpenPlaceApplications : ScriptBase
{
    private const int WindowTimeoutSeconds = 30;
    private const int PreflightTimeoutSeconds = 2;
    private const int StabilizationDelayMilliseconds = 350;

    // This canonical demonstration starts a fresh, deterministic Preview run.
    // Set false only when a scenario deliberately continues an existing sequence.
    private const bool ResetStateForFreshPreviewRun = true;

    private void Execute()
    {
        string previewDirectory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string statePath = Path.Combine(previewDirectory, "state.txt");
        string assemblyPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        if (!FileExists(assemblyPath))
        {
            ABORT("Multi-monitor Preview DLL is missing at: " + assemblyPath + ". Run 00-Prepare-MultiMonitor before this workload.");
            return;
        }

        // The Close workload can safely act only when this demonstration owns the
        // sole matching base window for each application.
        RequireNoExistingPreviewWindows();

        DllPreviewPlacement placement = new DllPreviewPlacement(assemblyPath);
        if (ResetStateForFreshPreviewRun)
        {
            RequireSuccess(placement, placement.ResetState(statePath));
            Log("Fresh Preview run: round-robin state reset before the first application placement.");
        }

        LaunchNotepad();
        IWindow notepad = RequireUniqueNotepadWindow(WindowTimeoutSeconds);
        RequireSuccess(placement, placement.PlaceNext(notepad.NativeWindowHandle, "Notepad", statePath, true, StabilizationDelayMilliseconds));

        // Paint's existing ShellExecute plus class/process discovery path is the
        // path already proven on the tested Windows configuration.
        ShellExecute("mspaint.exe", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: true);
        IWindow paint = RequireUniquePaintWindow(WindowTimeoutSeconds);
        RequireSuccess(placement, placement.PlaceNext(paint.NativeWindowHandle, "Paint", statePath, true, StabilizationDelayMilliseconds));

        // One workload has one associated TARGET. Edge is that target because the
        // proven START/MainWindow path avoids the short-lived raw launch PID.
        START(processName: "msedge", timeout: WindowTimeoutSeconds);
        IWindow edge = MainWindow;
        IWindow resolvedEdge = RequireUniqueEdgeWindow(WindowTimeoutSeconds);
        if (edge.NativeWindowHandle != resolvedEdge.NativeWindowHandle)
        {
            ABORT("Microsoft Edge MainWindow did not match the sole durable Edge base window. No Edge destination was consumed.");
            return;
        }

        RequireSuccess(placement, placement.PlaceNext(edge.NativeWindowHandle, "Microsoft Edge", statePath, true, StabilizationDelayMilliseconds));
        Log("Open/Place complete. Configure this workload with Leave application running ON so the following Close workload can clean up explicitly.");
    }

    private void LaunchNotepad()
    {
        // Raw Login Enterprise ShellExecute PID tracking was not durable for modern
        // Notepad. This compatible launch pattern comes from the preserved POC; the
        // durable base window is resolved independently before placement.
        Process launchedProcess = Process.Start(
            new ProcessStartInfo
            {
                FileName = "notepad.exe",
                UseShellExecute = true
            });

        if (launchedProcess != null)
        {
            launchedProcess.Dispose();
        }
    }

    private void RequireNoExistingPreviewWindows()
    {
        int notepadCount = CountNotepadWindows(PreflightTimeoutSeconds);
        int paintCount = CountPaintWindows(PreflightTimeoutSeconds);
        int edgeCount = CountEdgeWindows(PreflightTimeoutSeconds);
        if (notepadCount != 0 || paintCount != 0 || edgeCount != 0)
        {
            ABORT(
                "Preview ownership preflight failed. Close existing matching base windows before running this demonstration. " +
                "Notepad=" + notepadCount + ", Paint=" + paintCount + ", MicrosoftEdge=" + edgeCount + ".");
        }
    }

    private IWindow RequireUniqueNotepadWindow(int timeoutSeconds)
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(processName: "notepad", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        return RequireUniqueWindow(candidate, count, "Notepad");
    }

    private IWindow RequireUniquePaintWindow(int timeoutSeconds)
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:MSPaintApp", processName: "mspaint", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        return RequireUniqueWindow(candidate, count, "Paint");
    }

    private IWindow RequireUniqueEdgeWindow(int timeoutSeconds)
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:Chrome_WidgetWin_1", processName: "msedge", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        return RequireUniqueWindow(candidate, count, "Microsoft Edge");
    }

    private IWindow RequireUniqueWindow(IWindow candidate, int count, string applicationName)
    {
        if (count != 1)
        {
            ABORT(applicationName + " durable base-window resolution was ambiguous. Expected=1, Actual=" + count + ". No destination was consumed.");
            return null;
        }

        return candidate;
    }

    private int CountNotepadWindows(int timeoutSeconds)
    {
        int count = 0;
        var windows = FindWindows(processName: "notepad", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private int CountPaintWindows(int timeoutSeconds)
    {
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:MSPaintApp", processName: "mspaint", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private int CountEdgeWindows(int timeoutSeconds)
    {
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:Chrome_WidgetWin_1", processName: "msedge", timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private void RequireSuccess(DllPreviewPlacement placement, object result)
    {
        Log(placement.FormatResult(result));
        if (!placement.IsSuccess(result))
        {
            ABORT("Multi-monitor Preview placement failed: " + placement.GetMessage(result));
        }
    }
}

internal sealed class DllPreviewPlacement
{
    private readonly Type _placerType;

    internal DllPreviewPlacement(string assemblyPath)
    {
        Assembly assembly = Assembly.LoadFrom(assemblyPath);
        _placerType = assembly.GetType("LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
    }

    internal object ResetState(string statePath)
    {
        return Invoke("ResetState", new object[] { statePath });
    }

    internal object PlaceNext(IntPtr handle, string applicationName, string statePath, bool maximize, int delayMilliseconds)
    {
        return Invoke("PlaceNext", new object[] { handle, applicationName, statePath, maximize, delayMilliseconds });
    }

    internal bool IsSuccess(object result)
    {
        return (bool)GetProperty(result, "Success");
    }

    internal string GetMessage(object result)
    {
        return Convert.ToString(GetProperty(result, "Message"));
    }

    private object Invoke(string methodName, object[] arguments)
    {
        try
        {
            return _placerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    private static object GetProperty(object result, string name)
    {
        return result.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }

    internal string FormatResult(object result)
    {
        return Convert.ToString(GetProperty(result, "ApplicationName")) +
            ": Success=" + GetProperty(result, "Success") +
            ", MonitorCount=" + GetProperty(result, "MonitorCount") +
            ", Initial=" + GetProperty(result, "InitialMonitorIndex") +
            ", Target=" + GetProperty(result, "TargetMonitorIndex") +
            ", Verified=" + GetProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + GetProperty(result, "StateAdvanced") +
            ", ElapsedMs=" + GetProperty(result, "ElapsedMilliseconds") +
            ", Message=" + GetProperty(result, "Message");
    }
}

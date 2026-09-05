// TARGET:msedge.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class OfficeMultiMonitorVisualDemo : ScriptBase
{
    private const int WindowTimeoutSeconds = 60;
    private const int PreflightTimeoutSeconds = 2;
    private const int PlacementDelayMilliseconds = 350;
    private const int BetweenApplicationsSeconds = 3;
    private const int FinalHoldSeconds = 8;

    private void Execute()
    {
        DemoPlacement placement = LoadPlacement();
        RequireNoExistingDemoWindows();

        string statePath = Path.Combine(
            GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor", "state.txt");
        RequireSuccess(placement, placement.ResetState(statePath));

        IWindow word = LaunchOfficeApplication(
            "winword.exe", "Win32 Window:OpusApp", "WINWORD", "Microsoft Word");
        PlaceNext(placement, word, "Microsoft Word", statePath);
        word.Focus();
        Wait(BetweenApplicationsSeconds);

        // Edge owns this workload's TARGET so it can retain the proven
        // START/MainWindow path instead of following a transient launch PID.
        START(processName: "msedge", timeout: WindowTimeoutSeconds);
        IWindow edge = MainWindow;
        IWindow resolvedEdge = RequireUniqueWindow(
            "Win32 Window:Chrome_WidgetWin_1", "msedge", "Microsoft Edge", WindowTimeoutSeconds);
        if (edge.NativeWindowHandle != resolvedEdge.NativeWindowHandle)
        {
            ABORT("Microsoft Edge MainWindow did not match the sole durable Edge base window. No Edge destination was consumed.");
            return;
        }

        PlaceNext(placement, edge, "Microsoft Edge", statePath);
        edge.Focus();
        Wait(BetweenApplicationsSeconds);

        IWindow excel = LaunchOfficeApplication(
            "excel.exe", "*XLMAIN*", "EXCEL", "Microsoft Excel");
        PlaceNext(placement, excel, "Microsoft Excel", statePath);
        excel.Focus();
        Wait(BetweenApplicationsSeconds);

        IWindow powerPoint = LaunchOfficeApplication(
            "powerpnt.exe", "*PPTFrameClass*", "POWERPNT", "Microsoft PowerPoint");
        PlaceNext(placement, powerPoint, "Microsoft PowerPoint", statePath);
        powerPoint.Focus();

        Log("All demo applications are open. Holding the layout for recording.");
        Wait(FinalHoldSeconds);

        CloseOpenedWindow(powerPoint, "Microsoft PowerPoint");
        CloseOpenedWindow(excel, "Microsoft Excel");
        CloseOpenedWindow(edge, "Microsoft Edge");
        CloseOpenedWindow(word, "Microsoft Word");
        Log("Visual demo cleanup complete.");
    }

    private IWindow LaunchOfficeApplication(
        string executable, string windowClass, string processName, string applicationName)
    {
        // A workload has one TARGET. Secondary Office apps use the documented
        // shell launch, followed by the Office Preview's durable-window lookup.
        ShellExecute(
            executable,
            waitForProcessEnd: false,
            timeout: WindowTimeoutSeconds,
            forceKillOnExit: true);

        return RequireUniqueWindow(
            windowClass, processName, applicationName, WindowTimeoutSeconds);
    }

    private void RequireNoExistingDemoWindows()
    {
        RequireNoExistingWindow("Win32 Window:OpusApp", "WINWORD", "Microsoft Word");
        RequireNoExistingWindow("Win32 Window:Chrome_WidgetWin_1", "msedge", "Microsoft Edge");
        RequireNoExistingWindow("*XLMAIN*", "EXCEL", "Microsoft Excel");
        RequireNoExistingWindow("*PPTFrameClass*", "POWERPNT", "Microsoft PowerPoint");
    }

    private void RequireNoExistingWindow(
        string windowClass, string processName, string applicationName)
    {
        int count = CountWindows(windowClass, processName, PreflightTimeoutSeconds);
        if (count > 0)
        {
            ABORT(
                applicationName + " already has " + count +
                " durable base window(s). Close them before running this ownership-safe visual demo.");
        }
    }

    private IWindow RequireUniqueWindow(
        string windowClass, string processName, string applicationName, int timeoutSeconds)
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(
            className: windowClass,
            processName: processName,
            timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        if (count != 1)
        {
            ABORT(
                applicationName + " durable base-window resolution was ambiguous. " +
                "Expected=1, Actual=" + count + ". No destination was consumed.");
            return null;
        }

        return candidate;
    }

    private int CountWindows(string windowClass, string processName, int timeoutSeconds)
    {
        int count = 0;
        var windows = FindWindows(
            className: windowClass,
            processName: processName,
            timeout: timeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private DemoPlacement LoadPlacement()
    {
        string directory = Path.Combine(
            GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(directory, "LoginVSI.MultiMonitor.dll");
        if (!FileExists(assemblyPath))
        {
            ABORT(
                "Multi-monitor Preview DLL is missing at " + assemblyPath +
                ". Run 00-Prepare-MultiMonitor first.");
        }

        return new DemoPlacement(assemblyPath);
    }

    private void PlaceNext(
        DemoPlacement placement, IWindow window, string applicationName, string statePath)
    {
        object result = placement.PlaceNext(
            window.NativeWindowHandle,
            applicationName,
            statePath,
            true,
            PlacementDelayMilliseconds);
        RequireSuccess(placement, result);
    }

    private void RequireSuccess(DemoPlacement placement, object result)
    {
        Log(placement.Format(result));
        if (!placement.Success(result))
        {
            ABORT("Multi-monitor Preview operation failed: " + placement.Message(result));
        }
    }

    private void CloseOpenedWindow(IWindow window, string applicationName)
    {
        try
        {
            window.Close();
            Wait(1);
            Log(applicationName + " cleanup: close request completed for the window opened by this demo.");
        }
        catch
        {
            Log(
                applicationName +
                " cleanup: the owned window could not be closed normally; no broad process termination was attempted.");
        }
    }
}

internal sealed class DemoPlacement
{
    private readonly Type _type;

    internal DemoPlacement(string assemblyPath)
    {
        _type = Assembly.LoadFrom(assemblyPath).GetType(
            "LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
    }

    internal object ResetState(string statePath)
    {
        return Invoke("ResetState", new object[] { statePath });
    }

    internal object PlaceNext(
        IntPtr handle,
        string applicationName,
        string statePath,
        bool maximize,
        int delayMilliseconds)
    {
        return Invoke(
            "PlaceNext",
            new object[] { handle, applicationName, statePath, maximize, delayMilliseconds });
    }

    internal bool Success(object result)
    {
        return (bool)Get(result, "Success");
    }

    internal string Message(object result)
    {
        return Convert.ToString(Get(result, "Message"));
    }

    internal string Format(object result)
    {
        return Convert.ToString(Get(result, "ApplicationName")) +
            ": Success=" + Get(result, "Success") +
            ", MonitorCount=" + Get(result, "MonitorCount") +
            ", Initial=" + Get(result, "InitialMonitorIndex") +
            ", Target=" + Get(result, "TargetMonitorIndex") +
            ", Verified=" + Get(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + Get(result, "StateAdvanced") +
            ", ElapsedMs=" + Get(result, "ElapsedMilliseconds") +
            ", Message=" + Get(result, "Message");
    }

    private object Invoke(string methodName, object[] arguments)
    {
        try
        {
            return _type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Static).Invoke(null, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    private static object Get(object result, string name)
    {
        return result.GetType().GetProperty(
            name,
            BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }
}

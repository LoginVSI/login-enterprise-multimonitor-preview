// TARGET:msedge.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class OfficePreviewPlaceMicrosoftEdge : ScriptBase
{
    private const int WindowTimeoutSeconds = 60;

    private void Execute()
    {
        OfficePreviewPlacement placement = LoadPlacement();
        RequireNoExistingEdgeWindow();

        // Runtime-proven in the generic Login Enterprise 6.8.6 flow: START
        // resolves the durable application UI instead of tracking Edge's
        // short-lived raw launch PID.
        START(processName: "msedge", timeout: WindowTimeoutSeconds);
        IWindow edge = MainWindow;
        IWindow resolvedEdge = RequireUniqueEdgeWindow(WindowTimeoutSeconds);
        if (edge.NativeWindowHandle != resolvedEdge.NativeWindowHandle)
        {
            ABORT("Microsoft Edge MainWindow did not match the sole durable Edge base window. No Edge destination was consumed.");
            return;
        }

        Place(placement, edge, "Microsoft Edge");
    }

    private void RequireNoExistingEdgeWindow()
    {
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:Chrome_WidgetWin_1", processName: "msedge", timeout: 2);
        foreach (IWindow window in windows) { count++; }
        if (count > 0) { ABORT("Microsoft Edge already has a durable base window. Close existing Edge windows before running this ownership-safe Preview example."); }
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

        if (count != 1) { ABORT("Expected exactly one durable Microsoft Edge base window after START, but found " + count + ". No Edge destination was consumed."); }
        return candidate;
    }

    private OfficePreviewPlacement LoadPlacement()
    {
        string directory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(directory, "LoginVSI.MultiMonitor.dll");
        if (!FileExists(assemblyPath)) { ABORT("Multi-monitor Preview DLL is missing. Run 00-Prepare-MultiMonitor first."); }
        return new OfficePreviewPlacement(assemblyPath, Path.Combine(directory, "state.txt"));
    }

    private void Place(OfficePreviewPlacement placement, IWindow window, string applicationName)
    {
        object result = placement.PlaceNext(window.NativeWindowHandle, applicationName);
        Log(placement.Format(result));
        if (!placement.Success(result)) { ABORT("Office Preview placement failed: " + placement.Message(result)); }
    }
}

internal sealed class OfficePreviewPlacement
{
    private readonly Type _type; private readonly string _statePath;
    internal OfficePreviewPlacement(string assemblyPath, string statePath) { _type = Assembly.LoadFrom(assemblyPath).GetType("LoginVSI.MultiMonitor.MultiMonitorPlacer", true); _statePath = statePath; }
    internal object PlaceNext(IntPtr handle, string name) { return _type.GetMethod("PlaceNext", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { handle, name, _statePath, true, 350 }); }
    internal bool Success(object result) { return (bool)Get(result, "Success"); }
    internal string Message(object result) { return Convert.ToString(Get(result, "Message")); }
    private static object Get(object result, string name) { return result.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null); }
    internal string Format(object result) { return Get(result, "ApplicationName") + ": Success=" + Get(result, "Success") + ", MonitorCount=" + Get(result, "MonitorCount") + ", Target=" + Get(result, "TargetMonitorIndex") + ", Verified=" + Get(result, "VerifiedMonitorIndex") + ", StateAdvanced=" + Get(result, "StateAdvanced") + ", ElapsedMs=" + Get(result, "ElapsedMilliseconds") + ", Message=" + Get(result, "Message"); }
}

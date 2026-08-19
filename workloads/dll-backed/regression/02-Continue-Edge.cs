// TARGET:msedge.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class DllMultiMonitorPreviewContinueEdge : ScriptBase
{
    private const int WindowTimeoutSeconds = 30;
    private const int StabilizationDelayMilliseconds = 350;

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

        DllPreviewPlacement placement = new DllPreviewPlacement(assemblyPath);

        START(processName: "msedge", timeout: WindowTimeoutSeconds);
        IWindow edge = MainWindow;
        RequireSuccess(placement, placement.PlaceNext(edge.NativeWindowHandle, "Microsoft Edge", statePath, true, StabilizationDelayMilliseconds));
        Log("DLL-backed phase 2 complete. Round-robin state is retained.");
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

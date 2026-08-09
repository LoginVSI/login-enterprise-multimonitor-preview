// TARGET:notepad.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class DllMultiMonitorPreviewInitializeNotepadPaint : ScriptBase
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
            ABORT("Multi-monitor Preview DLL was not staged at: " + assemblyPath);
            return;
        }

        DllPreviewPlacement placement = new DllPreviewPlacement(assemblyPath);

        RequireSuccess(placement, placement.ResetState(statePath));

        ShellExecute("notepad.exe", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: false);
        IWindow notepad = FindWindow(processName: "notepad", timeout: WindowTimeoutSeconds);
        RequireSuccess(placement, placement.PlaceNext(notepad.NativeWindowHandle, "Notepad", statePath, true, StabilizationDelayMilliseconds));

        ShellExecute("mspaint.exe", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: false);
        IWindow paint = FindWindow(processName: "mspaint", timeout: WindowTimeoutSeconds);
        RequireSuccess(placement, placement.PlaceNext(paint.NativeWindowHandle, "Paint", statePath, true, StabilizationDelayMilliseconds));

        Log("DLL-backed phase 1 complete. Applications and state are intentionally retained for the next workload file.");
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

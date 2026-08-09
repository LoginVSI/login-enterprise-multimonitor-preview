// TARGET:notepad.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class DllMultiMonitorPreviewContinueCmdEdge : ScriptBase
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

        ShellExecute("cmd.exe /k title Login Enterprise Multi-Monitor Preview Command", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: false);
        IWindow commandPrompt = FindWindow(title: "*Login Enterprise Multi-Monitor Preview Command*", processName: "cmd", timeout: WindowTimeoutSeconds);
        RequireSuccess(placement, placement.PlaceNext(commandPrompt.NativeWindowHandle, "Command Prompt", statePath, true, StabilizationDelayMilliseconds));

        HashSet<IntPtr> existingEdgeHandles = CaptureEdgeWindowHandles();
        ShellExecute("msedge.exe --new-window about:blank", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: false);
        IWindow edge = FindNewEdgeWindow(existingEdgeHandles);
        if (edge == null)
        {
            ABORT("A newly created Microsoft Edge window could not be distinguished from pre-existing Edge windows.");
            return;
        }

        RequireSuccess(placement, placement.PlaceNext(edge.NativeWindowHandle, "Microsoft Edge", statePath, true, StabilizationDelayMilliseconds));
        Log("DLL-backed phase 2 complete. Applications and state are intentionally retained.");
    }

    private void RequireSuccess(DllPreviewPlacement placement, object result)
    {
        Log(placement.FormatResult(result));
        if (!placement.IsSuccess(result))
        {
            ABORT("Multi-monitor Preview placement failed: " + placement.GetMessage(result));
        }
    }

    private HashSet<IntPtr> CaptureEdgeWindowHandles()
    {
        HashSet<IntPtr> handles = new HashSet<IntPtr>();
        var windows = FindWindows(classname: "Win32 Window:Chrome_WidgetWin_1", processname: "msedge", timeout: 2);
        foreach (var window in windows)
        {
            handles.Add(window.NativeWindowHandle);
        }

        return handles;
    }

    private IWindow FindNewEdgeWindow(HashSet<IntPtr> existingHandles)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            var windows = FindWindows(classname: "Win32 Window:Chrome_WidgetWin_1", processname: "msedge", timeout: 2);
            foreach (var window in windows)
            {
                if (!existingHandles.Contains(window.NativeWindowHandle))
                {
                    return window;
                }
            }

            Wait(0.5);
        }

        return null;
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

// BROWSER:EdgeChromium
// URL:https://example.com/

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class Legacy_Selenium_Edge_MultiMonitor : ScriptBase
{
    private const int BrowserWindowTimeoutSeconds = 30;
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;

    private void Execute()
    {
        InitializeMultiMonitorPreview();

        HashSet<IntPtr> existingEdgeHandles = CaptureEdgeWindowHandles();
        StartBrowser();

        try
        {
            IWindow browserWindow = FindSingleNewEdgeWindow(
                existingEdgeHandles, BrowserWindowTimeoutSeconds);
            if (browserWindow == null)
            {
                ABORT("A newly created Edge top-level window could not be attributed to this workload.");
                return;
            }

            PlaceNext(browserWindow.NativeWindowHandle, "Microsoft Edge (legacy Selenium)");

            // Focus is workload behavior for the foreground interaction, not a
            // placement success condition or a responsibility of the DLL.
            browserWindow.Focus();
            Navigate(url: "https://example.com/", timerName: "Selenium_After_Placement");
        }
        finally
        {
            StopBrowser();
        }
    }

    private HashSet<IntPtr> CaptureEdgeWindowHandles()
    {
        HashSet<IntPtr> handles = new HashSet<IntPtr>();
        var windows = FindWindows(
            className: "Win32 Window:Chrome_WidgetWin_1",
            processName: "msedge",
            timeout: 2);
        foreach (var window in windows)
        {
            if (window.NativeWindowHandle != IntPtr.Zero)
            {
                handles.Add(window.NativeWindowHandle);
            }
        }

        return handles;
    }

    private IWindow FindSingleNewEdgeWindow(
        HashSet<IntPtr> existingHandles, int timeoutSeconds)
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (timer.Elapsed.TotalSeconds < timeoutSeconds)
        {
            IWindow candidate = null;
            int candidateCount = 0;
            var windows = FindWindows(
                className: "Win32 Window:Chrome_WidgetWin_1",
                processName: "msedge",
                timeout: 1);

            foreach (var window in windows)
            {
                IntPtr handle = window.NativeWindowHandle;
                if (handle != IntPtr.Zero && !existingHandles.Contains(handle))
                {
                    candidate = window;
                    candidateCount++;
                }
            }

            if (candidateCount > 1)
            {
                ABORT("More than one new Edge top-level window appeared; ownership is ambiguous.");
                return null;
            }

            if (candidateCount == 1)
            {
                return candidate;
            }

            Wait(0.5);
        }

        return null;
    }

    private void InitializeMultiMonitorPreview()
    {
        string previewDirectory = Path.Combine(
            GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        _multiMonitorStatePath = Path.Combine(previewDirectory, "state.txt");

        if (!FileExists(assemblyPath))
        {
            ABORT("Multi-Monitor Preview DLL is missing at " + assemblyPath +
                ". Run 00-Prepare-MultiMonitor first.");
        }

        _multiMonitorPlacerType = Assembly.LoadFrom(assemblyPath).GetType(
            "LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
    }

    private void PlaceNext(IntPtr windowHandle, string applicationName)
    {
        object result = _multiMonitorPlacerType.GetMethod(
            "PlaceNext", BindingFlags.Public | BindingFlags.Static).Invoke(
                null, new object[] { windowHandle, applicationName,
                    _multiMonitorStatePath, true, 350 });

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
    }

    private static object Property(object result, string name)
    {
        return result.GetType().GetProperty(
            name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }
}

// BROWSER:Chrome
// URL:https://example.com/

using LoginPI.Engine.ScriptBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class Playwright_Chrome_MultiMonitor : WebScriptBase
{
    private const int BrowserWindowTimeoutSeconds = 30;
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;

    private async Task Execute()
    {
        InitializeMultiMonitorPreview();

        // StartBrowser owns the browser session. The snapshot prevents this
        // workload from claiming a Chrome window that was already present.
        HashSet<IntPtr> existingChromeHandles = CaptureTopLevelWindowHandles("chrome");
        await StartBrowser();

        try
        {
            IntPtr browserWindowHandle = await FindSingleNewTopLevelWindow(
                "chrome", existingChromeHandles, BrowserWindowTimeoutSeconds);
            if (browserWindowHandle == IntPtr.Zero)
            {
                ABORT("A newly created Chrome top-level window could not be attributed to this workload.");
                return;
            }

            PlaceNext(browserWindowHandle, "Google Chrome (Playwright)");

            // Keep the source workload's Web Recorder-generated actions here.
            // This browser action intentionally occurs after native placement.
            await NavigateAsync(url: "https://example.com/", timerName: "Playwright_After_Placement");
            await TypeAsync(input: "{Tab}", cpm: 0);
        }
        finally
        {
            await StopBrowser();
        }
    }

    private async Task<IntPtr> FindSingleNewTopLevelWindow(
        string processName, HashSet<IntPtr> existingHandles, int timeoutSeconds)
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (timer.Elapsed.TotalSeconds < timeoutSeconds)
        {
            HashSet<IntPtr> currentHandles = CaptureTopLevelWindowHandles(processName);
            currentHandles.ExceptWith(existingHandles);

            if (currentHandles.Count > 1)
            {
                ABORT("More than one new Chrome top-level window appeared; ownership is ambiguous.");
                return IntPtr.Zero;
            }

            foreach (IntPtr handle in currentHandles)
            {
                return handle;
            }

            await Task.Delay(500);
        }

        return IntPtr.Zero;
    }

    private static HashSet<IntPtr> CaptureTopLevelWindowHandles(string processName)
    {
        HashSet<IntPtr> handles = new HashSet<IntPtr>();
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (Process process in processes)
        {
            try
            {
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    handles.Add(handle);
                }
            }
            catch (InvalidOperationException)
            {
                // The browser process exited while it was being inspected.
            }
            catch (Win32Exception)
            {
                // A process that cannot be inspected is not an ownership candidate.
            }
            finally
            {
                process.Dispose();
            }
        }

        return handles;
    }

    private void InitializeMultiMonitorPreview()
    {
        string previewDirectory = Path.Combine(
            Environment.GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        _multiMonitorStatePath = Path.Combine(previewDirectory, "state.txt");

        if (!File.Exists(assemblyPath))
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

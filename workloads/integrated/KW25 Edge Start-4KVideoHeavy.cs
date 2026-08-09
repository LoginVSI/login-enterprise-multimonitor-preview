// TARGET:msedge
// START_IN:

/////////////
// Edge Start
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

// Multi-Monitor Preview adaptation of the immutable reference workload.

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

public class Edge_Start : ScriptBase
{
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;
    // =====================================================
    // Configurable Variables
    // =====================================================
    // Browser settings
    string browserExecutable = "msedge.exe";          // Browser executable name
    int tabsToOpen = 2;                              // Number of browser tabs to open
    int waitMessageboxInSeconds = 2;                  // Duration for onscreen wait messages
    int globalWaitInSeconds = 3;                      // Standard wait time between actions

    // Browser launch and initialization timing
    int waitTimeoutInSecondsMsedgeLaunch = 60;         // Maximum wait time (in seconds) for the browser to initially appear
    int waitInSecondsBrowserInitialize = 120;           // Wait time (in seconds) to allow the browser to fully load the defined tabs/URLs

    // =====================================================
    // Execute Method
    // =====================================================
    void Execute()
    {
        InitializeMultiMonitorPreview();
        Log("Starting browser open process.");
        Wait(seconds: waitMessageboxInSeconds, showOnScreen: true, onScreenText: "Starting browser open process.");

        // =====================================================
        // Setup: Create Directory and Copy PDF
        // =====================================================
        // Get the current user's TEMP folder path.
        string tempPath = GetEnvironmentVariable("TEMP");
        Log("Retrieved TEMP folder: " + tempPath);

        // Define the subdirectory path for LoginEnterprise.
        string loginEnterpriseDir = Path.Combine(tempPath, "LoginEnterprise");
        Directory.CreateDirectory(loginEnterpriseDir);
        Log("Ensured directory exists: " + loginEnterpriseDir);

        // Define the destination path for the PDF file.
        string pdfDestination = Path.Combine(loginEnterpriseDir, "loginvsi.pdf");

        // Copy the PDF file from the Login Enterprise appliance to the destination.
        Log("Copying PDF file from KnownFiles.PdfFile to " + pdfDestination);
        CopyFile(KnownFiles.PdfFile, pdfDestination, continueOnError: false, overwrite: true);
        Log("PDF file copied successfully.");

        // =====================================================
        // Build URL List with Hardcoded PDF Path
        // =====================================================
        // Construct the local file URL for the PDF.
        string pdfUrl = "file:///" + pdfDestination.Replace("\\", "/");
        Log("Constructed local PDF URL: " + pdfUrl);

        // Build the URL list.
        string videoPath = Path.Combine(tempPath, "LoginPI", "MultiMonitor", "Big Buck Bunny Demo.mp4");
        string videoUrl = "file:///" + videoPath.Replace("\\", "/").Replace(" ", "%20");
        string urlsDefined =
            videoUrl + ";" +
            "about:blank;" +
            pdfUrl + ";" +
            "https://images.nasa.gov/;" +
            "https://www.google.com/search?q=beautiful+mountains&udm=2;" +
            "https://www.google.com/search?q=login+vsi&udm=2;" +
            "https://www.bing.com/images/search?q=login%20vsi&lq=0&ghsh=0&ghacc=0&first=1;" +
            "https://www.microsoft.com;";
        Log("URL list constructed.");
        // The local 4K video must be staged at videoPath before running this workload.

        // Split the defined URLs into an array using semicolon as the delimiter.
        string[] urlArray = urlsDefined.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        Log("URL array created with " + urlArray.Length + " entries.");

        string firstCommand = browserExecutable + " --guest --no-session-restore";

        // Build the command using the helper method (includes URLs).
        string secondCommand = BuildCommand(urlArray, tabsToOpen);
        Log("Command built: " + secondCommand);

        StartTimer("Browser_Start");
        // Launch the msedge instance.
        HashSet<IntPtr> existingEdgeHandles = CaptureEdgeWindowHandles();
        ShellExecute(secondCommand, waitForProcessEnd: false, continueOnError: false, forceKillOnExit: false);
        IWindow browserWindow = FindNewEdgeWindow(existingEdgeHandles);
        if (browserWindow == null)
        {
            CancelTimer("Browser_Start");
            ABORT("A newly created Microsoft Edge window could not be distinguished from pre-existing Edge windows.");
            return;
        }
        StopTimer("Browser_Start");

        Wait(waitInSecondsBrowserInitialize, onScreenText: "Waiting for browser to fully load tabs");
        Log("Waited " + waitInSecondsBrowserInitialize + " seconds for browser initialization.");

        PlaceNext(browserWindow, "Microsoft Edge");
        browserWindow.Maximize();
        browserWindow.Focus();
        Wait(globalWaitInSeconds);
        Log("Browser open process completed.");
    }

    // =====================================================
    // Helper: Build Command String
    // =====================================================
    // Constructs the command string for launching the browser with multiple URLs.
    string BuildCommand(string[] urls, int tabs)
    {
        StringBuilder cmdBuilder = new StringBuilder();
        cmdBuilder.Append(browserExecutable);
        cmdBuilder.Append(" --guest --no-session-restore");
        for (int i = 0; i < tabs; i++)
        {
            string url = urls[i % urls.Length].Trim();
            cmdBuilder.Append(" " + url);
        }
        Log("BuildCommand completed.");
        return cmdBuilder.ToString();
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
        Stopwatch discoveryTimer = Stopwatch.StartNew();
        while (discoveryTimer.Elapsed.TotalSeconds < waitTimeoutInSecondsMsedgeLaunch)
        {
            // Keep each search short so the complete failure path remains close to the configured timeout.
            var windows = FindWindows(classname: "Win32 Window:Chrome_WidgetWin_1", processname: "msedge", timeout: 1);
            foreach (var window in windows)
            {
                if (!existingHandles.Contains(window.NativeWindowHandle))
                {
                    return window;
                }
            }

            if (discoveryTimer.Elapsed.TotalSeconds < waitTimeoutInSecondsMsedgeLaunch)
            {
                Wait(0.5);
            }
        }

        return null;
    }

    private void InitializeMultiMonitorPreview()
    {
        string previewDirectory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        _multiMonitorStatePath = Path.Combine(previewDirectory, "state.txt");
        if (!FileExists(assemblyPath))
        {
            ABORT("Multi-monitor Preview DLL was not staged at: " + assemblyPath);
        }

        _multiMonitorPlacerType = Assembly.LoadFrom(assemblyPath).GetType("LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
    }

    private int PlaceNext(IWindow window, string applicationName)
    {
        object result = _multiMonitorPlacerType.GetMethod("PlaceNext", BindingFlags.Public | BindingFlags.Static).Invoke(
            null, new object[] { window.NativeWindowHandle, applicationName, _multiMonitorStatePath, true, 350 });
        LogPlacement(result);
        if (!(bool)GetPlacementProperty(result, "Success"))
        {
            ABORT("Multi-monitor Preview placement failed: " + GetPlacementProperty(result, "Message"));
        }

        return Convert.ToInt32(GetPlacementProperty(result, "TargetMonitorIndex"));
    }

    private static object GetPlacementProperty(object result, string name)
    {
        return result.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }

    private void LogPlacement(object result)
    {
        Log(GetPlacementProperty(result, "ApplicationName") + ": Success=" + GetPlacementProperty(result, "Success") +
            ", Target=" + GetPlacementProperty(result, "TargetMonitorIndex") + ", Verified=" + GetPlacementProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + GetPlacementProperty(result, "StateAdvanced") + ", ElapsedMs=" + GetPlacementProperty(result, "ElapsedMilliseconds") +
            ", Message=" + GetPlacementProperty(result, "Message"));
    }
}

// TARGET:msedge
// START_IN:

/////////////
// Edge Run
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

// Multi-Monitor Preview adaptation of the immutable reference workload.

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public class Edge_Run : ScriptBase
{
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;
    // =====================================================
    // Import and Constants for mouse scrolling
    // =====================================================
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);
    public const uint MOUSEEVENTF_WHEEL = 0x0800; // Constant for a mouse wheel event

    // =====================================================
    // Configurable Variables
    // =====================================================
    // Global timings and iterations
    int ctrlTabIterations = 5;                      // Number of iterations for tab switching and scrolling interactions
    int ctrlTabWaitSecondsBeforeScroll = 30;         // Wait time before scrolling to allow the page to load
    int ctrlTabWaitSecondsAfterScroll = 1;          // Wait time after scrolling before next iteration
    string browserProcessName = "msedge";           // Process name for Microsoft Edge

    // Scrolling parameters
    int scrollDownCount = 15;                       // Number of scroll events for scrolling down
    int scrollUpCount = 15;                         // Number of scroll events for scrolling up
    double scrollWaitTime = 0.2;                    // Wait time between each scroll event

    // Additional global wait times
    int globalWaitInSeconds = 3;                    // Standard wait time between actions
    int waitMessageboxInSeconds = 2;                // Duration for onscreen wait messages
    int startMenuWaitInSeconds = 5;                // Duration for Start Menu wait between interactions

    private void Execute()
    {
        InitializeMultiMonitorPreview();
        // =====================================================
        // Simulate Start Menu Interaction
        // =====================================================
        Log("Simulating Start Menu interaction.");
        Wait(startMenuWaitInSeconds);
        Type("{LWIN}",hideInLogging:false);
        Wait(seconds: startMenuWaitInSeconds);
        Type("{LWIN}",hideInLogging:false);
        Wait(seconds: 1);
        Type("{ESC}",hideInLogging:false);
        Wait(startMenuWaitInSeconds);

        // =====================================================
        // Bring Browser Window into Focus
        // =====================================================
        var browserWindow = FindWindow(
            className: "Win32 Window:Chrome_WidgetWin_1",
            title: "*Microsoft​ Edge",
            processName: browserProcessName);
        Wait(globalWaitInSeconds);
        browserWindow.Minimize();
        Wait(globalWaitInSeconds);
        browserWindow.Maximize();
        browserWindow.Focus();
        Wait(globalWaitInSeconds);
        int previewMonitorIndex = PlaceLastUsed(browserWindow, "Microsoft Edge");

        // =====================================================
        // Setup Iteration Message and Wait Time
        // =====================================================
        int totalWaitPerIteration = ctrlTabWaitSecondsBeforeScroll + ctrlTabWaitSecondsAfterScroll;
        int totalCtrlTabTime = ctrlTabIterations * totalWaitPerIteration;
        string message = $"Performing {ctrlTabIterations} iterations with {ctrlTabWaitSecondsBeforeScroll} sec wait after scrolling and {ctrlTabWaitSecondsAfterScroll} sec wait after scrolling. Total wait time: {totalCtrlTabTime} sec.";
        Wait(seconds: waitMessageboxInSeconds, showOnScreen: true, onScreenText: message);
        Log(message);

        // =====================================================
        // Iterate Over Tabs with Scrolling Interactions
        // =====================================================
        for (int i = 0; i < ctrlTabIterations; i++)
        {
            Log($"Iteration {i + 1} started.");
            Wait(seconds: ctrlTabWaitSecondsBeforeScroll);

            // Ensure browser window is maximized and in focus\
            browserWindow.Focus();
            browserWindow.Maximize();
            PlaceOnMonitor(browserWindow, "Microsoft Edge", previewMonitorIndex);
            Wait(seconds: 1);
            browserWindow.MoveMouseToCenter(continueOnError: true);
            RightClick(x:300,y:300);
            Wait(0.5);
            Type("{ESC}", hideInLogging: false); // Close the context menu if it appears, to ensure we are on the page.

            if (i > 0)
            {
                Wait(seconds: 0.5);
                Log("Switching to next tab with Ctrl+Tab.");
                browserWindow.Type("{ctrl+tab}", hideInLogging: false);
                Wait(seconds: 0.5);
                browserWindow.Type("{f5}",hideInLogging:false);
                Wait(seconds: ctrlTabWaitSecondsBeforeScroll);
                browserWindow.Focus();
                browserWindow.Maximize();
                PlaceOnMonitor(browserWindow, "Microsoft Edge", previewMonitorIndex);
                browserWindow.MoveMouseToCenter(continueOnError: true);
                Log("Switched tab and refocused window.");
            }

            // =====================================================
            // Helper: Scroll Function
            // =====================================================
            // Usage of Scroll():
            //   - direction: "Down" to scroll down or "Up" to scroll up.
            //   - scrollCount: Number of scroll events to send.
            //   - notches: Number of notches per event (1 notch is typically 120).
            //   - waitTime: Time in seconds to wait between each scroll event.
            // Example:
            //   Scroll("Down", 20, 1, 0.2);
            //   Scroll("Up", 10, 2, 0.3);
            // =====================================================
            // Scroll Interactions on Active Tab
            // =====================================================
            Log("Starting scroll interactions on the active tab.");
            Wait(seconds: 1);
            Scroll("Down", scrollDownCount, 1, scrollWaitTime);
            Scroll("Up", scrollUpCount, 1, scrollWaitTime);
            Log("Scroll interactions completed for this iteration.");
            Wait(seconds: ctrlTabWaitSecondsAfterScroll, showOnScreen: true, onScreenText: "Waiting after scrolling");
            Log($"Iteration {i + 1} complete. Waiting {ctrlTabWaitSecondsAfterScroll} seconds before next iteration.");
        }
        Log("All iterations completed.");
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

    private int PlaceLastUsed(IWindow window, string applicationName)
    {
        object result = InvokePlacement("PlaceLastUsed", new object[] { window.NativeWindowHandle, applicationName, _multiMonitorStatePath, true, 350 });
        return Convert.ToInt32(GetPlacementProperty(result, "TargetMonitorIndex"));
    }

    private void PlaceOnMonitor(IWindow window, string applicationName, int targetMonitorIndex)
    {
        InvokePlacement("PlaceOnMonitor", new object[] { window.NativeWindowHandle, applicationName, _multiMonitorStatePath, targetMonitorIndex, true, 350 });
    }

    private object InvokePlacement(string methodName, object[] arguments)
    {
        object result = _multiMonitorPlacerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, arguments);
        Log(GetPlacementProperty(result, "ApplicationName") + ": Success=" + GetPlacementProperty(result, "Success") +
            ", Target=" + GetPlacementProperty(result, "TargetMonitorIndex") + ", Verified=" + GetPlacementProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + GetPlacementProperty(result, "StateAdvanced") + ", ElapsedMs=" + GetPlacementProperty(result, "ElapsedMilliseconds") +
            ", Message=" + GetPlacementProperty(result, "Message"));
        if (!(bool)GetPlacementProperty(result, "Success"))
        {
            ABORT("Multi-monitor Preview placement failed: " + GetPlacementProperty(result, "Message"));
        }

        return result;
    }

    private static object GetPlacementProperty(object result, string name)
    {
        return result.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }

    void Scroll(string direction, int scrollCount, int notches, double waitTime)
    {
        if (waitTime <= 0)
        {
            throw new ArgumentException("Scroll waitTime must be greater than 0 seconds.");
        }

        int sign = direction.Equals("Down", StringComparison.OrdinalIgnoreCase) ? -1 : 1;
        int delta = sign * 120 * notches;

        Log($"Scrolling mouse {direction} {scrollCount} times, {notches} notch(es) per scroll, with {waitTime} sec between each scroll.");
        for (int i = 0; i < scrollCount; i++)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta, UIntPtr.Zero);
            Wait(seconds: waitTime);
        }
        Log($"Completed scrolling mouse {direction} {scrollCount} times.");
    }
}

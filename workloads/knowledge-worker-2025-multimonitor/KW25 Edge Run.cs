// TARGET:msedge
// START_IN:

/////////////
// Edge Run
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

using LoginPI.Engine.ScriptBase;
// BEGIN KW25 PREVIEW
using LoginPI.Engine.ScriptBase.Components;
using System.IO;
// END KW25 PREVIEW
using System;
using System.Runtime.InteropServices;

public class Edge_Run : ScriptBase
{

    // BEGIN KW25 PREVIEW HELPERS
    private System.Type _kwPlacer;
    private string _kwDirectory;
    private int _kwMonitor;
    private int _kwMonitorCount;
    private void KwInitialize(string app, bool start)
    {
        _kwDirectory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string dll = Path.Combine(_kwDirectory, "LoginVSI.MultiMonitor.dll");
        if (!FileExists(dll)) { ABORT("Run 00-Prepare-MultiMonitor first."); return; }
        _kwPlacer = System.Reflection.Assembly.LoadFrom(dll).GetType("LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
        string record = Path.Combine(_kwDirectory, "KW25-" + app + ".txt");
        if (start) { ABORT("Run cannot initialize a Start assignment."); return; }
        else
        {
            if (!File.Exists(record)) { ABORT("Missing KW25 monitor record. Run its Start first."); return; }
            string[] assignment = File.ReadAllLines(record);
            if (assignment.Length != 2 || !int.TryParse(assignment[0], out _kwMonitorCount) ||
                !int.TryParse(assignment[1], out _kwMonitor) || _kwMonitorCount < 1 ||
                _kwMonitor < 0 || _kwMonitor >= _kwMonitorCount)
            { ABORT("Invalid KW25 monitor record. Restart the scenario from clean state."); return; }
            KwValidateState();
        }
    }
    private void KwValidateState()
    {
        string state = Path.Combine(_kwDirectory, "state.txt");
        if (!File.Exists(state)) { ABORT("KW25 allocation state is missing. Restart the scenario."); return; }
        string[] lines = File.ReadAllLines(state);
        int count;
        int last;
        if (lines.Length != 2 || !lines[0].StartsWith("MonitorCount=") || !lines[1].StartsWith("LastUsedIndex=") ||
            !int.TryParse(lines[0].Substring(13), out count) || !int.TryParse(lines[1].Substring(14), out last) ||
            count != _kwMonitorCount || last < 0 || last >= count)
        { ABORT("KW25 allocation state changed or is invalid. Restart the scenario from clean state."); }
    }
    private void KwPlace(IWindow window, string app, bool start)
    {
        if (window == null || window.NativeWindowHandle == System.IntPtr.Zero)
        { ABORT("KW25 durable window is missing."); return; }
        if (!start) { KwValidateState(); }
        IWindow current = KwRequireUnique("msedge", "Win32 Window:Chrome_WidgetWin_1", "*Microsoft​ Edge");
        if (current == null || current.NativeWindowHandle != window.NativeWindowHandle)
        { ABORT("KW25 durable window is absent, replaced or ambiguous. No placement attempted."); return; }
        object[] args = new object[] { window.NativeWindowHandle, app, Path.Combine(_kwDirectory, "state.txt"), _kwMonitor, true, 350 };
        object result = _kwPlacer.GetMethod("PlaceOnMonitor").Invoke(null, args);
        Log(app + ": Success=" + KwProperty(result, "Success") + ", MonitorCount=" + KwProperty(result, "MonitorCount") +
            ", Target=" + KwProperty(result, "TargetMonitorIndex") + ", Verified=" + KwProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + KwProperty(result, "StateAdvanced") + ", ElapsedMs=" + KwProperty(result, "ElapsedMilliseconds") +
            ", Message=" + KwProperty(result, "Message"));
        if (!(bool)KwProperty(result, "Success")) { ABORT("KW25 placement failed: " + KwProperty(result, "Message")); return; }

    }
    private static object KwProperty(object result, string name)
    { return result.GetType().GetProperty(name).GetValue(result, null); }
    private void KwRequireEmpty(string process, string windowClass)
    {
        foreach (var window in FindWindows(className: windowClass, processName: process, timeout: 2))
        { ABORT("Close existing " + process + " base windows before KW25 Start."); return; }
    }
    private IWindow KwRequireUnique(string process, string windowClass, string title)
    {
        IWindow selected = null;
        int count = 0;
        foreach (var window in FindWindows(className: windowClass, title: title, processName: process, timeout: 2))
        { selected = window; count++; }
        if (count != 1) { ABORT("KW25 base window ownership is absent or ambiguous."); return null; }
        return selected;
    }
    // END KW25 PREVIEW HELPERS
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
    int ctrlTabWaitSecondsBeforeScroll = 3;         // Wait time before scrolling to allow the page to load
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
// BEGIN KW25 PREVIEW
        KwInitialize("Edge", false);
// END KW25 PREVIEW

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
// BEGIN KW25 PREVIEW
        var kwBase = KwRequireUnique("msedge", "Win32 Window:Chrome_WidgetWin_1", "*Microsoft​ Edge");
        if (kwBase == null || kwBase.NativeWindowHandle != browserWindow.NativeWindowHandle) { ABORT("Ambiguous Edge Run window."); return; }
// END KW25 PREVIEW
        Wait(globalWaitInSeconds);
        browserWindow.Minimize();
        Wait(globalWaitInSeconds);
        browserWindow.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(browserWindow, "Edge", false);
// END KW25 PREVIEW
        browserWindow.Focus();
        Wait(globalWaitInSeconds);

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
// BEGIN KW25 PREVIEW
        KwPlace(browserWindow, "Edge", false);
// END KW25 PREVIEW
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
// BEGIN KW25 PREVIEW
        KwPlace(browserWindow, "Edge", false);
// END KW25 PREVIEW
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

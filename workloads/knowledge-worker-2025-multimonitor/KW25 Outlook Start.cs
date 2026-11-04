// TARGET:outlook.exe /importprf %TEMP%\LoginPI\Outlook.prf
// START_IN:

/////////////
// Outlook Start
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

using LoginPI.Engine.ScriptBase;
// BEGIN KW25 PREVIEW
using LoginPI.Engine.ScriptBase.Components;
// END KW25 PREVIEW
using System.IO;
using System;

public class Outlook_Start : ScriptBase
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
        if (start) { if (File.Exists(record)) { File.Delete(record); } }
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
        IWindow current = KwRequireUnique("OUTLOOK", "Win32 Window:rctrl_renwnd32", "Inbox*");
        if (current == null || current.NativeWindowHandle != window.NativeWindowHandle)
        { ABORT("KW25 durable window is absent, replaced or ambiguous. No placement attempted."); return; }
        object[] args = start
            ? new object[] { window.NativeWindowHandle, app, Path.Combine(_kwDirectory, "state.txt"), true, 350 }
            : new object[] { window.NativeWindowHandle, app, Path.Combine(_kwDirectory, "state.txt"), _kwMonitor, true, 350 };
        object result = _kwPlacer.GetMethod(start ? "PlaceNext" : "PlaceOnMonitor").Invoke(null, args);
        Log(app + ": Success=" + KwProperty(result, "Success") + ", MonitorCount=" + KwProperty(result, "MonitorCount") +
            ", Target=" + KwProperty(result, "TargetMonitorIndex") + ", Verified=" + KwProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + KwProperty(result, "StateAdvanced") + ", ElapsedMs=" + KwProperty(result, "ElapsedMilliseconds") +
            ", Message=" + KwProperty(result, "Message"));
        if (!(bool)KwProperty(result, "Success")) { ABORT("KW25 placement failed: " + KwProperty(result, "Message")); return; }
        if (start)
        {
            // Persist topology count and index only, never native handles.
            string record = Path.Combine(_kwDirectory, "KW25-" + app + ".txt");
            File.WriteAllLines(record, new string[] { KwProperty(result, "MonitorCount").ToString(), KwProperty(result, "TargetMonitorIndex").ToString() });
        }
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
    // Configurable Variables
    // =====================================================
    int globalWaitInSeconds = 3;                    // Wait time between actions
    int waitMessageboxInSeconds = 2;                // Duration for onscreen wait messages

    // =====================================================
    // Execute Method
    // =====================================================
    private void Execute()
    {
// BEGIN KW25 PREVIEW
        KwInitialize("Outlook", true);
        KwRequireEmpty("OUTLOOK", "Win32 Window:rctrl_renwnd32");
// END KW25 PREVIEW

        // =====================================================
        // Setup: Create Directory and Prepare Files
        // =====================================================
        Log("Preparing Outlook configuration.");
        // Retrieve the TEMP environment variable.
        var temp = GetEnvironmentVariable("TEMP");

        // Define the target directory.
        string targetDir = $"{temp}\\LoginPI";
        // Create the "Login PI" directory if it doesn't exist.
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
            Log("Created directory: " + targetDir);
        }

        // =====================================================
        // Download PRF and PST Files
        // =====================================================
        string prfFile = Path.Combine(targetDir, "Outlook.prf");
        string pstFile = Path.Combine(targetDir, "Outlook.pst");

        // Attempt to delete the files if they exist, but continue on error
        try
        {
            if (File.Exists(prfFile))
            {
                File.Delete(prfFile);
            }
        }
        catch (Exception ex)
        {
            Log($"Error deleting file {prfFile}: {ex.Message}");
        }

        try
        {
            if (File.Exists(pstFile))
            {
                File.Delete(pstFile);
            }
        }
        catch (Exception ex)
        {
            Log($"Error deleting file {pstFile}: {ex.Message}");
        }

        Wait(seconds: waitMessageboxInSeconds, showOnScreen: true, onScreenText: "Get PRF & PST");
        Log("Downloading PRF & PST files.");
        CopyFile(KnownFiles.OutlookConfiguration, prfFile, continueOnError: true, overwrite: false);
        CopyFile(KnownFiles.OutlookData, pstFile, continueOnError: true, overwrite: false);

        // =====================================================
        // Update PRF File
        // =====================================================
        // Replace the placeholder %TEMP% with the actual TEMP path.
        string prfContent = File.ReadAllText(prfFile).Replace("%TEMP%", temp);
        File.WriteAllText(prfFile, prfContent);
        Log("Updated PRF file with current TEMP path.");

        // =====================================================
        // Launch Outlook
        // =====================================================
        Wait(seconds: waitMessageboxInSeconds, showOnScreen: true, onScreenText: "Starting Outlook");
        Log("Starting Outlook.");
        START(mainWindowTitle: "Inbox*", mainWindowClass: "Win32 Window:rctrl_renwnd32", processName: "OUTLOOK", timeout: 60, continueOnError: true);
        Wait(globalWaitInSeconds);
        MainWindow.Maximize();
        MainWindow.Focus();

        // =====================================================
        // Dismiss Activation Dialogs
        // =====================================================
        int activateOfficeRetryCount = 2;
        for (int i = 0; i < activateOfficeRetryCount; i++)
        {
            var activateOfficeWindow = MainWindow.FindControlWithXPath(
                "Win32 Window:BasicEmbeddedBrowser",
                timeout: 3,
                continueOnError: true);
            if (activateOfficeWindow != null)
            {
                Wait(seconds: globalWaitInSeconds);
                activateOfficeWindow.Type("{ESC}", hideInLogging: false);
                Log("Dismissed an Activate Office dialog.");
            }
        }

        Wait(seconds: globalWaitInSeconds);
        SkipFirstRunDialogs();
        MainWindow.Maximize();
// BEGIN KW25 PREVIEW
        IWindow kwBase = KwRequireUnique("OUTLOOK", "Win32 Window:rctrl_renwnd32", "Inbox*");
        if (kwBase == null || kwBase.NativeWindowHandle != MainWindow.NativeWindowHandle) { ABORT("KW25 startup window changed before placement."); return; }
        KwPlace(kwBase, "Outlook", true);
// END KW25 PREVIEW
        MainWindow.Focus();
        Log("Outlook is now ready.");
    }

    // =====================================================
    // Helper: Skip First-Run Dialogs
    // =====================================================
    private void SkipFirstRunDialogs()
    {
        int firstRunRetryCount = 2;
        for (int i = 0; i < firstRunRetryCount; i++)
        {
            var signinWindow = MainWindow.FindControlWithXPath(
                "Win32 Window:NUIDialog",
                timeout: 3,
                continueOnError: true);
            if (signinWindow != null)
            {
                Wait(seconds: globalWaitInSeconds);
                signinWindow.Type("{ESC}", hideInLogging: false);
                Log("Dismissed a first-run dialog.");
            }
        }
    }
}

// TARGET:msedge
// START_IN:

/////////////
// Edge Start
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

using LoginPI.Engine.ScriptBase;
// BEGIN KW25 PREVIEW
using LoginPI.Engine.ScriptBase.Components;
// END KW25 PREVIEW
using System.Text;
using System.IO;
using System.Diagnostics;

public class Edge_Start : ScriptBase
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
        IWindow current = KwRequireUnique("msedge", "Win32 Window:Chrome_WidgetWin_1", "*Microsoft​ Edge");
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
    // Browser settings
    string browserExecutable = "msedge.exe";          // Browser executable name
    int tabsToOpen = 10;                              // Number of browser tabs to open
    int waitMessageboxInSeconds = 2;                  // Duration for onscreen wait messages
    int globalWaitInSeconds = 3;                      // Standard wait time between actions

    // Browser launch and initialization timing
    int waitTimeoutInSecondsMsedgeLaunch = 60;         // Maximum wait time (in seconds) for the browser to initially appear
    int waitInSecondsBrowserInitialize = 15;           // Wait time (in seconds) to allow the browser to fully load the defined tabs/URLs

    // =====================================================
    // Execute Method
    // =====================================================
    void Execute()
    {
// BEGIN KW25 PREVIEW
        KwInitialize("Edge", true);
        KwRequireEmpty("msedge", "Win32 Window:Chrome_WidgetWin_1");
// END KW25 PREVIEW

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
        string urlsDefined =
            "https://euc.loginvsi.com/customer-portal/knowledge-worker-2023;" +
            pdfUrl + ";" +
            "https://images.nasa.gov/;" +
            "https://www.google.com/search?q=beautiful+mountains&udm=2;" +
            "https://www.google.com/search?q=login+vsi&udm=2;" +
            "https://www.bing.com/images/search?q=login%20vsi&lq=0&ghsh=0&ghacc=0&first=1;" +
            "https://www.microsoft.com;";
        Log("URL list constructed.");
        // Good GPU impact site using WebGL: https://webglsamples.org/aquarium/aquarium.html;
        // And one for a high def autoplaying streaming video (Big Buck Bunny streaming of a rendered video): http://distribution.bbb3d.renderfarming.net/video/mp4/bbb_sunflower_2160p_30fps_normal.mp4    or  http://distribution.bbb3d.renderfarming.net/video/mp4/bbb_sunflower_1080p_30fps_normal.mp4
        // Note: for more Big Buck Bunny (BBB) video types (resolutions and fps), refer to http://bbb3d.renderfarming.net/download.html (note the HTTP links)

        // Split the defined URLs into an array using semicolon as the delimiter.
        string[] urlArray = urlsDefined.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        Log("URL array created with " + urlArray.Length + " entries.");

        string firstCommand = browserExecutable + " --guest --no-session-restore";

        // Build the command using the helper method (includes URLs).
        string secondCommand = BuildCommand(urlArray, tabsToOpen);
        Log("Command built: " + secondCommand);

        StartTimer("Browser_Start");
        // Launch the msedge instance.
        ShellExecute(secondCommand, waitForProcessEnd: false, continueOnError: false, forceKillOnExit: false);
        string browserProcessName = Path.GetFileNameWithoutExtension(browserExecutable);
        var browserWindow = FindWindow(
            className: "Win32 Window:Chrome_WidgetWin_1",
            title: "*Microsoft​ Edge",
            processName: browserProcessName,
            timeout: waitTimeoutInSecondsMsedgeLaunch);
        StopTimer("Browser_Start");

        Wait(waitInSecondsBrowserInitialize, onScreenText: "Waiting for browser to fully load tabs");
        Log("Waited " + waitInSecondsBrowserInitialize + " seconds for browser initialization.");

        browserWindow.Maximize();
// BEGIN KW25 PREVIEW
        IWindow kwBase = KwRequireUnique("msedge", "Win32 Window:Chrome_WidgetWin_1", "*Microsoft​ Edge");
        if (kwBase == null || kwBase.NativeWindowHandle != browserWindow.NativeWindowHandle) { ABORT("KW25 startup window changed before placement."); return; }
        KwPlace(kwBase, "Edge", true);
// END KW25 PREVIEW
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
}

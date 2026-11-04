// TARGET:powerpnt.exe /n
// START_IN:

/////////////
// PowerPoint Run
// Workload: Knowledge Worker 2025
// Version: 0.1.0
/////////////

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
using LoginPI.Engine.ScriptBase.Constants;

public class PowerPoint_Run : ScriptBase
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
        IWindow current = KwRequireUnique("POWERPNT", "*PPTFrameClass*", "*loginvsi*PowerPoint*");
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
    // Configurable Variables
    // =====================================================
    int globalTimeoutInSeconds = 60;
    int globalWaitInSeconds = 3;
    int waitMessageboxInSeconds = 2;
    int charactersPerMinuteToType = 15;
    int waitInBetweenKeyboardShortcuts = 4;
    int slideshowCharactersPerMinuteToType = 12;
    int pageScrollCpm = 60;
    int transitionPopupCharactersPerMinuteToType = 60;
    int waitForPopupShowingInSeconds = 10;
    int waitSlideshowStart = 10;
    int typingTextCharacterPerMinute = 600;
    int startMenuWaitInSeconds = 5;

    private void Execute()
    {
// BEGIN KW25 PREVIEW
        KwInitialize("PowerPoint", false);
// END KW25 PREVIEW

        // ----- (File download for pptx is handled by the Start script) -----
        var temp = GetEnvironmentVariable("TEMP");
        string loginEnterpriseDir = $"{temp}\\LoginEnterprise";
        string pptxFile = $"{loginEnterpriseDir}\\loginvsi.pptx";

        // ----- Copy BMP from Appliance scriptcontent if needed -----
        string bmpFile = Path.Combine(loginEnterpriseDir, "LoginVSI_BattlingRobots.bmp");
        if (!FileExists(bmpFile))
        {
            Log("Copying BMP from Appliance ScriptContent");
            CopyFile(
                sourcePath      : UrnBaseForFiles.UrnBase + "LoginVSI_BattlingRobots.bmp",
                destinationPath : bmpFile
            );
        }
        else
        {
            Log("BMP file already exists");
        }

        // =====================================================
        // Launch new blank PowerPoint presentation
        // =====================================================
        try
        {
            ShellExecute("powerpnt /n", waitForProcessEnd: false, timeout: globalTimeoutInSeconds, continueOnError: true, forceKillOnExit: false);
        }
        catch (Exception ex)
        {
            ABORT("Error starting process: " + ex.Message);
        }

        Wait(globalWaitInSeconds);
        var newPowerpointWindow = FindWindow(title:"*Presentation*PowerPoint*", processName:"POWERPNT", continueOnError:false, timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);

        // =====================================================
        // Simulate Start Menu Interaction
        // =====================================================
        Log("Simulating Start Menu interaction.");
        Wait(startMenuWaitInSeconds);
        Type("{LWIN}", hideInLogging:false);
        Wait(startMenuWaitInSeconds);
        Type("{LWIN}", hideInLogging:false);
        Wait(1);
        Type("{ESC}", hideInLogging:false);
        Wait(startMenuWaitInSeconds);

        // =====================================================
        // Close any extraneous PowerPoint windows ("*loginvsi*" or "*edited*")
        // =====================================================
        CloseExtraWindows("POWERPNT", "*loginvsi*");
        CloseExtraWindows("POWERPNT", "*edited*");

        // =====================================================
        // Skip First-Run Dialogs before Bringing PowerPoint into Focus
        // =====================================================
        Wait(globalWaitInSeconds);
        SkipFirstRunDialogs();

        // =====================================================
        // Bring new PowerPoint instance into focus and open file via dialog
        // =====================================================
        Wait(startMenuWaitInSeconds);
        newPowerpointWindow.Focus();
        newPowerpointWindow.Maximize();
        Log("Opening PPTX file via open file dialog");
        Wait(waitMessageboxInSeconds, true, "Open pptx file");
        newPowerpointWindow.Type("{CTRL+O}", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        newPowerpointWindow.Type("{ALT+O+O}", cpm: charactersPerMinuteToType, hideInLogging:false);
        StartTimer("Open_Window");
        var openWindow = FindWindow(className:"Win32 Window:#32770", processName:"POWERPNT", continueOnError:false, timeout:globalTimeoutInSeconds);
        StopTimer("Open_Window");
        Wait(globalWaitInSeconds);
        var fileNameBox = openWindow.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        openWindow.Type("{ALT+N}", hideInLogging:false);
        fileNameBox.Click();
        Wait(globalWaitInSeconds);
        SetTextBoxText(fileNameBox, pptxFile, cpm: typingTextCharacterPerMinute);
        Type("{ENTER}", hideInLogging:false);
        StartTimer("Open_Powerpoint_Document");
        var newPowerpoint = FindWindow(className:"Win32 Window:PPTFrameClass", title:"loginvsi*", processName:"POWERPNT", timeout:globalTimeoutInSeconds);
        StopTimer("Open_Powerpoint_Document");
        Wait(globalWaitInSeconds);
        newPowerpoint.Focus();
        newPowerpoint.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(newPowerpoint, "PowerPoint", false);
// END KW25 PREVIEW
        newPowerpoint.FindControl(className:"TabItem:NetUIRibbonTab", title:"Insert", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);

        // Close any stray "Presentation" windows
        CloseExtraWindows("POWERPNT", "*Presentation*");

        // =====================================================
        // Continue with the rest of the workflow (transitions, slideshow, Save As, etc.)
        // =====================================================
        // --- Add new slide ---
        newPowerpoint.Focus();
        newPowerpoint.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(newPowerpoint, "PowerPoint", false);
// END KW25 PREVIEW
        Wait(globalWaitInSeconds);
        newPowerpoint.Type("{CTRL+M}", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);

        // --- Insert BMP into New Slide ---
        Log("Inserting BMP into new slide");
        newPowerpoint.Type("{ALT}NP1", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(waitForPopupShowingInSeconds);
        Type("d", cpm: charactersPerMinuteToType, hideInLogging:false);
        StartTimer("Insert_Picture_Dialog");
        var addPictureDialog = FindWindow(className:"Win32 Window:#32770", processName:"POWERPNT", title:"Insert Picture", timeout:globalTimeoutInSeconds);
        StopTimer("Insert_Picture_Dialog");
        addPictureDialog.Focus();
        var fileNameBox2 = addPictureDialog.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        fileNameBox2.Click();
        newPowerpoint.Type("{ALT+N}", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        ScriptHelpers.SetTextBoxText(this, fileNameBox2, bmpFile, cpm: typingTextCharacterPerMinute);
        fileNameBox2.Type("{ENTER}", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(globalWaitInSeconds);
        var stillExists = FindWindow(className:"Win32 Window:#32770", title:"Insert Picture", processName:"POWERPNT", timeout:2, continueOnError:true);
        if (stillExists != null)
        {
            newPowerpoint.Type("{ESC}", cpm: charactersPerMinuteToType, hideInLogging:false);
        }

        // --- Add 'Honeycomb' Transition ---
        Log("Adding 'Honeycomb' transition");
        newPowerpoint.Type("{ALT+K}{ALT+T}", cpm: charactersPerMinuteToType, hideInLogging:false);
        newPowerpoint.FindControl(title:"Honeycomb", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        newPowerpoint.Type("{DOWN}", cpm: transitionPopupCharactersPerMinuteToType, hideInLogging:false);
        newPowerpoint.Type("{RIGHT}".Repeat(16), cpm: transitionPopupCharactersPerMinuteToType, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        newPowerpoint.Type("{ENTER}", hideInLogging:false);
        Wait(globalWaitInSeconds);

        // --- Add transition to all slides ---
        Log("Adding transition to all slides");
        newPowerpoint.Type("{ALT+K}{ALT+L}", hideInLogging:false);
        Wait(globalWaitInSeconds);

        // --- Scroll, Minimize, and Maximize Presentation ---
        Wait(waitMessageboxInSeconds, true, "Scroll, minimize, and maximize");
        Log("Scrolling through presentation");
        newPowerpoint.Type("{PAGEDOWN}".Repeat(10), cpm: pageScrollCpm, hideInLogging:false);
        Wait(globalWaitInSeconds);
        newPowerpoint.Type("{PAGEUP}".Repeat(10), cpm: pageScrollCpm, hideInLogging:false);
        Wait(globalWaitInSeconds);
        newPowerpoint.Minimize();
        Wait(globalWaitInSeconds);
        newPowerpoint.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(newPowerpoint, "PowerPoint", false);
// END KW25 PREVIEW
        newPowerpoint.Focus();
        Wait(globalWaitInSeconds);

        // --- Run the Slideshow ---
        Wait(waitMessageboxInSeconds, true, "Run the slideshow");
        Log("Starting slideshow");
        newPowerpoint.Type("{F5}", cpm: charactersPerMinuteToType, hideInLogging:false);
        Wait(waitSlideshowStart);
        newPowerpoint.Type("{DOWN}".Repeat(5), cpm: slideshowCharactersPerMinuteToType, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        Type("{ESC}", hideInLogging:false);
        Wait(globalWaitInSeconds);
        Type("{HOME}", hideInLogging:false);
        Wait(globalWaitInSeconds);

        // --- Save the Edited Slideshow ---
        Log("Saving the edited slideshow");
        Wait(waitMessageboxInSeconds, true, "Save the slideshow");
        string saveFilename = $"{loginEnterpriseDir}\\edited.pptx";
        if (FileExists(saveFilename))
        {
            Log("Removing existing file: " + saveFilename);
            RemoveFile(saveFilename);
        }
        else
        {
            Log("No existing file to remove at: " + saveFilename);
        }
        newPowerpoint.Type("{F12}", hideInLogging:false);
        StartTimer("Save_As_Dialog");
        var saveAs = FindWindow(className:"Win32 Window:#32770", processName:"POWERPNT", timeout:globalTimeoutInSeconds);
        StopTimer("Save_As_Dialog");
        var saveFileNameBox = saveAs.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        Type("{ALT+N}", hideInLogging:false);
        saveFileNameBox.Click();
        Wait(globalWaitInSeconds);
        ScriptHelpers.SetTextBoxText(this, saveFileNameBox, saveFilename, cpm: typingTextCharacterPerMinute);
        saveAs.Type("{ENTER}", hideInLogging:false);
        StartTimer("Saving_file");
        FindWindow(title:"edited*", processName:"POWERPNT", timeout:globalTimeoutInSeconds);
        StopTimer("Saving_file");
        Wait(globalWaitInSeconds);

        Log("Script complete. PowerPoint remains open.");
    }

    void CloseExtraWindows(string processName, string titleMask)
    {
        int timeoutSeconds = 2;      // Timeout for re-checking the window
        int maxAttempts = 1;         // Maximum number of attempts to close the window

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var extraWindow = FindWindow(title: titleMask, processName: processName, timeout: 3, continueOnError: true);
            if (extraWindow == null)
            {
                // Window is already closed
                break;
            }

            Wait(globalWaitInSeconds);
            extraWindow.Focus();
            extraWindow.Maximize();
            Wait(globalWaitInSeconds);
            extraWindow.Type("{ESC}", hideInLogging: false);
            Wait(globalWaitInSeconds);
            extraWindow.Type("{ALT+F4}", hideInLogging: false);
            Wait(globalWaitInSeconds);

            // Check if the window is still present
            var checkWindow = FindWindow(title: titleMask, processName: processName, timeout: timeoutSeconds, continueOnError: true);
            if (checkWindow != null)
            {
                Wait(globalWaitInSeconds);
                checkWindow.Type("{ALT+N}", hideInLogging: false);
                Wait(globalWaitInSeconds);
            }
        }
    }

    void SetTextBoxText(IWindow textBox, string text, int cpm = 600)
    {
        int localWait = 3;
        int numTries = 1;
        string currentText = null;
        do
        {
            textBox.Type("{CTRL+a}", hideInLogging:false);
            Wait(localWait);
            textBox.Type(text, cpm: cpm);
            Wait(localWait);
            currentText = textBox.GetText();
            if (currentText != text)
                CreateEvent($"Typing error in attempt {numTries}", $"Expected '{text}', got '{currentText}'");
        }
        while (++numTries < 5 && currentText != text);
        if (currentText != text)
            ABORT($"Unable to set the correct text '{text}', got '{currentText}'");
    }

    private void SkipFirstRunDialogs()
    {
        int loopCount = 2;
        for (int i = 0; i < loopCount; i++)
        {
            var dialog = FindWindow(className:"Win32 Window:NUIDialog", processName:"POWERPNT", continueOnError:true, timeout:3);
            while (dialog != null)
            {
                Wait(globalWaitInSeconds);
                dialog.Close();
                dialog = FindWindow(className:"Win32 Window:NUIDialog", processName:"POWERPNT", continueOnError:true, timeout:3);
            }
        }
    }
}

// =====================================================
// Helper Class for TextBox Operations
// =====================================================
public static class ScriptHelpers
{
    public static void SetTextBoxText(ScriptBase script, IWindow textBox, string text, int cpm = 600)
    {
        int localWait = 3;
        int numTries = 1;
        string currentText = null;
        do
        {
            textBox.Type("{CTRL+a}", hideInLogging:false);
            script.Wait(localWait);
            textBox.Type(text, cpm: cpm, hideInLogging:false);
            script.Wait(localWait);
            currentText = textBox.GetText();
            if (currentText != text)
                script.CreateEvent($"Typing error in attempt {numTries}", $"Expected '{text}', got '{currentText}'");
        }
        while (++numTries < 5 && currentText != text);
        if (currentText != text)
            script.ABORT($"Unable to set the correct text '{text}', got '{currentText}'");
    }
}

// TARGET:winword.exe /t
// START_IN:

/////////////
// Word Run
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
using System.Runtime.InteropServices;
using System.Diagnostics;
using LoginPI.Engine.ScriptBase.Constants;

public class Word_Run : ScriptBase
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
        IWindow current = KwRequireUnique("WINWORD", "Win32 Window:OpusApp", "*loginvsi*Word*");
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
    // Import and Constants
    // =====================================================
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);
    public const uint MOUSEEVENTF_WHEEL = 0x0800; // Constant for a mouse wheel event

    // =====================================================
    // Configurable Variables
    // =====================================================
    int globalTimeoutInSeconds = 60;          // Timeout for actions (e.g., finding the app window)
    int waitMessageboxInSeconds = 2;          // Duration for onscreen wait messages (in seconds)
    double globalWaitInSeconds = 3;           // Standard wait time between actions
    int keyboardShortcutsCPM = 15;            // Typing speed for keyboard shortcuts
    int waitInBetweenKeyboardShortcuts = 3;   // Wait time between keyboard shortcuts
    int typingTextCPM = 600;                  // Typing speed for document text
    int copyPasteRepetitions = 1;             // Number of times to perform the copy-paste action
    int waitForCopyPasteInSeconds = 5;        // Wait time after copy-paste actions
    int startMenuWaitInSeconds = 5;           // Duration for Start Menu wait

    // Document content to be typed into Word, broken into separate lines.
    string[] documentContentLines = new string[]
    {
        "About Login VSI",
        "The VDI and DaaS industry has transformed incredibly, and Login VSI has evolved alongside the world of remote and hybrid work.",
        "Through an innovative and dynamic culture, the Login VSI team is passionate about helping enterprises worldwide understand, build, and maintain amazing digital workspaces.",
        "Trusted globally for 360° proactive visibility of performance, cost, and capacity of virtual desktops and applications, Login Enterprise is accepted as the industry standard and used by major vendors to spot problems quicker, avoid unexpected downtime, and deliver next-level digital experiences for end-users.",
        "Our Mission",
        "The paradigm for remote computing has shifted with virtual app delivery coupled with the growth in Web and SaaS-based applications.",
        "Now more than ever, organizations rely on digital workspaces to function. We give our customers 360° insights into the entire stack of virtual desktops and applications – in production or delivery and across various settings and infrastructure.",
        "We aim to empower IT teams to take control of their virtual desktops and applications’ performance, cost, and capacity wherever they reside – traditional, hybrid, or cloud."
    };

    private void Execute()
    {
// BEGIN KW25 PREVIEW
        KwInitialize("Word", false);
// END KW25 PREVIEW

        // ----- (File download for docx is handled by the Start script) -----
        var temp = GetEnvironmentVariable("TEMP");
        string loginEnterpriseDir = $"{temp}\\LoginEnterprise";
        string docxFile = $"{loginEnterpriseDir}\\loginvsi.docx";

        // ----- Copy BMP from Appliance scriptcontent if needed -----
        string bmpFile = $"{loginEnterpriseDir}\\LoginVSI_BattlingRobots.bmp";
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
        // Launch new blank Word document
        // =====================================================
        try
        {
            ShellExecute("winword /t", waitForProcessEnd: false, timeout: globalTimeoutInSeconds, continueOnError: true, forceKillOnExit: false);
        }
        catch (Exception ex)
        {
            ABORT("Error starting process: " + ex.Message);
        }

        var newExcelWindow = FindWindow(title:"*Document*Word*", processName:"WINWORD", continueOnError:false, timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);

        // =====================================================
        // Close any extraneous Word windows ("*loginvsi*" or "*edited*")
        // =====================================================
        CloseExtraWindows("WINWORD", "*loginvsi*");
        CloseExtraWindows("WINWORD", "*edited*");

        // =====================================================
        // Call new method to close a specific Microsoft Word confirmation window
        // =====================================================
        CloseMicrosoftWordDialog();

        // =====================================================
        // Skip First-Run Dialogs before Bringing Word into Focus
        // =====================================================
        Wait(globalWaitInSeconds);
        SkipFirstRunDialogs();

        // =====================================================
        // Bring Word Application into Focus
        // =====================================================
        var MainWindow = FindWindow(className:"Win32 Window:OpusApp", title:"*Document*", processName:"WINWORD", timeout:globalTimeoutInSeconds);
        Wait(waitMessageboxInSeconds, true, "Interacting with existing Word");
        MainWindow.Maximize();
        MainWindow.Focus();
        Wait(globalWaitInSeconds);

        // =====================================================
        // Open .docx File via Open File Dialog using MainWindow
        // =====================================================
        Log("Opening .docx file via open file dialog");
        Wait(waitMessageboxInSeconds, true, "Open .docx file");
        MainWindow.Type("{CTRL+O}", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        MainWindow.Type("{ALT+O+O}", cpm: keyboardShortcutsCPM, hideInLogging:false);
        StartTimer("Open_Window");
        var openWindow = FindWindow(className:"Win32 Window:#32770", processName:"WINWORD", continueOnError:false, timeout:globalTimeoutInSeconds);
        StopTimer("Open_Window");
        Wait(globalWaitInSeconds);
        var fileNameBox = openWindow.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        openWindow.Type("{ALT+N}", hideInLogging:false);
        fileNameBox.Click();
        Wait(globalWaitInSeconds);
        ScriptHelpers.SetTextBoxText(this, fileNameBox, docxFile, cpm: typingTextCPM);
        Type("{ENTER}", hideInLogging:false);
        StartTimer("Open_Word_Document");
        var newWord = FindWindow(className:"Win32 Window:OpusApp", title:"loginvsi*", processName:"WINWORD", timeout:globalTimeoutInSeconds);
        StopTimer("Open_Word_Document");

        // Close any stray "Document" windows
        CloseExtraWindows("WINWORD", "*Document*");
        Wait(globalWaitInSeconds);
        newWord.Focus();
        newWord.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(newWord, "Word", false);
// END KW25 PREVIEW

        // =====================================================
        // Skip First-Run Dialogs before Checking for an Existing Edited Window
        // =====================================================
        Wait(globalWaitInSeconds);
        SkipFirstRunDialogs();

        // =====================================================
        // Typing content, inserting BMP, scrolling, Save As, etc.
        // =====================================================
        // Type initial commands
        newWord.Type("{ALT}wi", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        newWord.Type("{ALT}w1", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        foreach (var line in documentContentLines)
        {
            newWord.Type(line, cpm: typingTextCPM, hideInLogging:false);
            newWord.Type("{Enter}", cpm: typingTextCPM, hideInLogging:false);
        }
        Wait(globalWaitInSeconds);

        // Insert BMP image into document
        Log("Inserting BMP image");
        newWord.Type("{ALT}np", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        Type("d", cpm: keyboardShortcutsCPM, hideInLogging:false); // Opens insert file dialog
        StartTimer("Insert_Picture_Dialog");
        var addPictureDialog = FindWindow(className:"Win32 Window:#32770", processName:"WINWORD", timeout:globalTimeoutInSeconds);
        StopTimer("Insert_Picture_Dialog");
        addPictureDialog.Focus();
        var fileNameBoxPic = addPictureDialog.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        addPictureDialog.Type("{ALT+N}", hideInLogging:false);
        fileNameBoxPic.Click();
        ScriptHelpers.SetTextBoxText(this, fileNameBoxPic, bmpFile, cpm: typingTextCPM);
        fileNameBoxPic.Type("{ENTER}", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts, true, "Picture inserted");

        // Copy & paste operations
        Log("Performing copy and paste operations");
        Wait(globalWaitInSeconds);
        newWord.Type("{CTRL+a}{CTRL+c}", cpm: keyboardShortcutsCPM, hideInLogging:false);
        Wait(waitInBetweenKeyboardShortcuts);
        for (int i = 0; i < copyPasteRepetitions; i++)
        {
            newWord.Type("{CTRL+V}", cpm: keyboardShortcutsCPM, hideInLogging:false);
            Wait(waitForCopyPasteInSeconds, true, "Pasting content");
        }

        // Scroll function
        Log("Scrolling the document");
        newWord.Type("{CTRL+HOME}", hideInLogging:false);
        Wait(waitForCopyPasteInSeconds, true, "Scrolling...");
        Scroll("Down", 40, 1, 0.1);
        Scroll("Up", 40, 1, 0.1);
        newWord.Type("{CTRL+HOME}", hideInLogging:false);
        Wait(waitForCopyPasteInSeconds, true, "Scrolling...");
        Scroll("Down", 40, 1, 0.1);
        Scroll("Up", 40, 1, 0.1);
        newWord.Type("{CTRL+HOME}", hideInLogging:false);
        Wait(globalWaitInSeconds);

        // Minimize/Maximize and Save As
        Log("Minimizing and maximizing Word window");
        newWord.Minimize();
        Wait(globalWaitInSeconds);
        newWord.Maximize();
// BEGIN KW25 PREVIEW
        KwPlace(newWord, "Word", false);
// END KW25 PREVIEW
        newWord.Focus();
        Wait(globalWaitInSeconds);

        Log("Saving the edited Word document");
        Wait(waitMessageboxInSeconds, true, "Save the document");
        string saveFilename = $"{loginEnterpriseDir}\\edited.docx";
        if (FileExists(saveFilename))
        {
            Log("Removing existing file: " + saveFilename);
            RemoveFile(saveFilename);
        }
        else
        {
            Log("No existing file to remove at: " + saveFilename);
        }
        newWord.Type("{F12}", cpm: keyboardShortcutsCPM, hideInLogging:false);
        StartTimer("Save_As_Dialog");
        var saveAs = FindWindow(className:"Win32 Window:#32770", processName:"WINWORD", continueOnError:true, timeout:globalTimeoutInSeconds);
        var saveFileNameBox = saveAs.FindControl(className:"Edit:Edit", title:"File name:", timeout:globalTimeoutInSeconds);
        Wait(globalWaitInSeconds);
        StopTimer("Save_As_Dialog");
        saveFileNameBox.Click();
        saveAs.Type("{ALT+N}", hideInLogging:false);
        ScriptHelpers.SetTextBoxText(this, saveFileNameBox, saveFilename, cpm: typingTextCPM);
        saveAs.Type("{ENTER}", hideInLogging:false);
        StartTimer("Saving_file");
        FindWindow(title:"edited*", processName:"WINWORD", timeout:globalTimeoutInSeconds);
        StopTimer("Saving_file");
        Wait(globalWaitInSeconds);

        Log("Script complete. Word remains open.");
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

    // Handling a specific Microsoft Word confirmation dialog: "Do you want to keep the last item you copied?"
    void CloseMicrosoftWordDialog()
    {
        int timeoutSeconds = 2;
        var msWordWindow = FindWindow(className: "Win32 Window:#32770", title: "Microsoft Word", processName: "WINWORD", timeout: timeoutSeconds, continueOnError: true);
        if (msWordWindow != null)
        {
            msWordWindow.Focus();
            msWordWindow.Maximize();
            Wait(globalWaitInSeconds);
            msWordWindow.Type("{ALT+N}", hideInLogging: false);
            Wait(globalWaitInSeconds);
        }
    }

    private void SkipFirstRunDialogs()
    {
        int loopCount = 2;
        for (int i = 0; i < loopCount; i++)
        {
            var dialog = FindWindow(className:"Win32 Window:NUIDialog", processName:"WINWORD", continueOnError:true, timeout:3);
            while (dialog != null)
            {
                Wait(globalWaitInSeconds);
                dialog.Close();
                dialog = FindWindow(className:"Win32 Window:NUIDialog", processName:"WINWORD", continueOnError:true, timeout:3);
            }
        }
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

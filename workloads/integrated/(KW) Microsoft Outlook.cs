// TARGET:outlook.exe /importprf %TEMP%\LoginPI\outlook.prf
// START_IN:

/////////////
//Windows Application
// Workload: KnowledgeWorker
// Version: 1.0
//
/////////////

// Multi-Monitor Preview adaptation of the immutable reference workload.
// App execution should be set to outlook.exe /importprf %TEMP%\LoginPI\outlook.prf.

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System.IO;
using System;
using System.Reflection;

public class M365Outlook524 : ScriptBase
{
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;

    private void Execute()
    {
        // This is a language dependent script. English is required.
        InitializeMultiMonitorPreview();

        // Outlook has a fixed commandline that is using the %temp% environment variable
        // because there is no environment variable for 'my documents'
        var temp = GetEnvironmentVariable("TEMP");

        // Download the PRF and PST file from the appliance through the KnownFiles method
        // Outlook is known to sometimes corrupt the pst file, so we
        // will always start from a clean file by overwriting it
        Wait(seconds:3, showOnScreen:true, onScreenText:"Get PRF & PST");
        Log("Downloading File");
        CopyFile(KnownFiles.OutlookConfiguration, $"{temp}\\LoginPI\\Outlook.prf",  overwrite:true, continueOnError:true);
        CopyFile(KnownFiles.OutlookData, $"{temp}\\LoginPI\\Outlook.pst",  overwrite:true, continueOnError:true);

        // Looks for the %TEMP% string in the prf file and replaces it with the {temp} variable.
        File.WriteAllText($"{temp}\\LoginPI\\Outlook.prf", File.ReadAllText($"{temp}\\LoginPI\\Outlook.prf").Replace("%TEMP%", $"{temp}"));

        // Click the Start Menu
        Wait(seconds:3, showOnScreen:true, onScreenText:"Start Menu");
        Type("{LWIN}");
        Wait(3);
        Type("{ESC}");
        Log(CommandLine);
        // Start Application
        //Log("Starting Outlook");
        Wait(seconds:3, showOnScreen:true, onScreenText:"Starting Outlook");
        START(mainWindowTitle:"Inbox*", mainWindowClass:"Win32 Window:rctrl_renwnd32", processName:"OUTLOOK", timeout:60, continueOnError:true);
        MainWindow.Maximize();
        int previewMonitorIndex = PlaceNext(MainWindow, "Microsoft Outlook");

        // Look for the Activate Office popup dialog and click on it to bring to the top, then hit ESC -- do we need a try/catch here?
        try {var signinWindow = MainWindow.FindControlWithXPath(xPath : "Win32 Window:NUIDialog", timeout:10); signinWindow.Type("{ESC}",cpm:50);} catch {}
        SkipFirstRunDialogs();

        // Select an item in the Inbox
        Wait(seconds:3, showOnScreen:true, onScreenText:"Select An Item");
        //var InboxWindow=MainWindow.FindControlWithXPath(xPath : "Table:SuperGrid/Group:GroupHeader/DataItem:LeafRow");
        var inboxWindow=MainWindow.FindControlWithXPath(xPath : "Table:SuperGrid");
        inboxWindow.Click();
        Wait(2);

        // Scroll through E-mail inbox
        Wait(seconds:3, showOnScreen:true, onScreenText:"Scroll Inbox");
        inboxWindow.Type("{DOWN}".Repeat(3),cpm:80);

        Wait(2);

        DismissReminders();

        inboxWindow.Type("{DOWN}".Repeat(4),cpm:80);
        inboxWindow.Type("{UP}".Repeat(8),cpm:80);
        Wait(2);

        DismissReminders();

        //Open an email read it and close it
        Wait(seconds:3, showOnScreen:true, onScreenText:"Open and Read an Email");
        inboxWindow.Focus();
        inboxWindow.Click();
        inboxWindow.Type("{DOWN}");
        inboxWindow.Type("{ENTER}");
        Wait(2);
        var openEmail=FindWindow(className : "Win32 Window:rctrl_renwnd32", title : "Login Enterprise Continuity & Application Load Testing - Message (HTML) ", processName : "OUTLOOK");
        openEmail.Focus();
        openEmail.Type("{DOWN}".Repeat(5),cpm:500);
        Wait(2);
        openEmail.Type("{UP}".Repeat(3),cpm:500);
        Wait(2);
        openEmail.Type("{ESC}",cpm:50);
        Wait(2);

        MainWindow.Minimize();
        Wait(2);
        MainWindow.Maximize();
        PlaceOnMonitor(MainWindow, "Microsoft Outlook", previewMonitorIndex);

        //Compose a new email with words from Vonnegut's 2-B-R-0-2-B
        Wait(seconds:3, showOnScreen:true, onScreenText:"Compose a new email with words from Vonnegut's 2-B-R-0-2-B");
        //MainWindow.FindControlWithXPath(xPath : "Pane:MsoCommandBarDock/ToolBar:MsoCommandBar/Pane:MsoWorkPane/Pane:NUIPane/Pane:NetUIHWNDElement/Pane:NetUInetpane/Pane:NetUIPanViewer/Custom:NetUIOrderedGroup/Group:NetUIChunk/SplitButton:NetUISplitButtonAnchor/Button:NetUIRibbonButton").Click();
        //MainWindow.FindControl(className : "Button:NetUIRibbonButton", title : "New Email").Click();
        MainWindow.Type("{CTRL+N}");
        Wait(2);
        var typingSpeed=900;
        var newEmail=FindWindow(className : "Win32 Window:rctrl_renwnd32", title : "Untitled - Message (HTML) ", processName : "OUTLOOK").Focus();
        newEmail.FindControl(className : "*RichEdit20WPT", title : "To").Type("preview.user1@example.invalid; preview.user2@example.invalid; preview.user3@example.invalid", cpm:typingSpeed);
        newEmail.Type("{TAB}".Repeat(3), 50);
        newEmail.Type("Today's Topics - Words from Vonnegut's 2-B-R-0-2-B", cpm:typingSpeed);
        newEmail.Type("{TAB}",cpm:50);
        newEmail.Type("{ENTER}",cpm:50);
        newEmail.Type("{CTRL+B}",cpm:50);
        newEmail.Type("Young Wehling was hunched in his chair, his head in his hand. He was so rumpled, so still and colorless as to be virtually invisible.{ENTER}", cpm:typingSpeed);
        newEmail.Type("His camouflage was perfect, since the waiting room had a disorderly and demoralized air, too. {ENTER}Chairs and ashtrays had been moved away from the walls.", cpm:typingSpeed);
        newEmail.Type("Chairs and ashtrays had been moved away from the walls.", cpm:typingSpeed);
        newEmail.Type("Sincerely sincere,",cpm : typingSpeed);
        newEmail.Type("{ENTER}",cpm:50);
        newEmail.Type("{CTRL+B}Mr. KURT VONNEGUT, JR.",cpm : typingSpeed);
        Wait(2);
        newEmail.Type("{ESC}");
        Wait(2);
        newEmail.Type("{ENTER}");
        Wait(3);

        STOP();
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
        object result = InvokePlacement("PlaceNext", new object[] { window.NativeWindowHandle, applicationName, _multiMonitorStatePath, true, 350 });
        return Convert.ToInt32(GetPlacementProperty(result, "TargetMonitorIndex"));
    }

    private void PlaceOnMonitor(IWindow window, string applicationName, int targetMonitorIndex)
    {
        InvokePlacement("PlaceOnMonitor", new object[] { window.NativeWindowHandle, applicationName, _multiMonitorStatePath, targetMonitorIndex, true, 350 });
    }

    private object InvokePlacement(string methodName, object[] arguments)
    {
        object result = _multiMonitorPlacerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, arguments);
        Log(applicationPlacementMessage(result));
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

    private static string applicationPlacementMessage(object result)
    {
        return GetPlacementProperty(result, "ApplicationName") + ": Success=" + GetPlacementProperty(result, "Success") +
            ", Target=" + GetPlacementProperty(result, "TargetMonitorIndex") + ", Verified=" + GetPlacementProperty(result, "VerifiedMonitorIndex") +
            ", StateAdvanced=" + GetPlacementProperty(result, "StateAdvanced") + ", ElapsedMs=" + GetPlacementProperty(result, "ElapsedMilliseconds") +
            ", Message=" + GetPlacementProperty(result, "Message");
    }

    private void DismissReminders()
    {
        //Dismiss all reminders
        // Reminders occur at unpredictable times, so we do this at severa places in the script
        var reminderWindow = FindWindow(className: "Win32 Window:#32770", title: "*Reminder(s)", processName: "OUTLOOK", timeout: 2, continueOnError: true);
        if (reminderWindow != null)
        {
            Wait(1);
            reminderWindow.Focus();
            reminderWindow.FindControl(className: "Button:Button", title: "Dismiss &All").Click();
            Wait(1);
            reminderWindow.FindControl(className: "Button:Button", title: "&Yes").Click();
        }
    }

    private void SkipFirstRunDialogs()
    {
        var dialog = FindWindow(className: "Win32 Window:NUIDialog", processName: "OUTLOOK", continueOnError: true, timeout: 1);
        while (dialog != null)
        {
            dialog.Close();
            dialog = FindWindow(className: "Win32 Window:NUIDialog", processName: "OUTLOOK", continueOnError: true, timeout: 10);
        }
    }
}

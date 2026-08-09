// TARGET:powerpnt.exe
// START_IN:

/////////////
// Windows Application
// Workload: KnowledgeWorker
// Version: 1.0
//
/////////////

// Multi-Monitor Preview adaptation of the immutable reference workload.

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.IO;
using System.Reflection;

public class M365PowerPoint524 : ScriptBase
{
    private Type _multiMonitorPlacerType;
    private string _multiMonitorStatePath;

    private void Execute()
    {
        // This is a language dependent script. English is required.
        InitializeMultiMonitorPreview();

        var temp = GetEnvironmentVariable("TEMP");

        // Optionally you can use the MyDocuments folder for file storage by setting the temp folder as follows
        // var temp = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // Directory.CreateDirectory($"{temp}\\LoginPI");

        // Download file from the appliance through the KnownFiles method, if it already exists: Skip Download.
        Wait(seconds:3, showOnScreen:true, onScreenText:"Get .pptx file");
        if(!(FileExists($"{temp}\\LoginPI\\loginvsi.pptx")))
        {
            Log("Downloading File");
            CopyFile(KnownFiles.PowerPointPresentation, $"{temp}\\LoginPI\\loginvsi.pptx");
        }
        else
        {
            Log("File already exists");
        }

        // Click the Start Menu
        Wait(seconds:3, showOnScreen:true, onScreenText:"Start Menu");
        Type("{LWIN}");
        Wait(3);
        Type("{ESC}");

        // Start Application
        //Log("Starting PowerPoint");
        Wait(seconds:3, showOnScreen:true, onScreenText:"Starting PowerPoint");
        START(mainWindowTitle:"*PowerPoint*", mainWindowClass:"*PPTFrameClass*", timeout:30);
        MainWindow.Maximize();

        var newDocName = "edited";
        var appWasLeftOpen = MainWindow.GetTitle().Contains(newDocName);
        if (appWasLeftOpen)
        {
            Log("PowerPoint was left open from previous run");
        }
        else
        {
            Wait(10);

            SkipFirstRunDialogs();
        }

        // Open "Open File" window and start measurement.
        Wait(seconds:3, showOnScreen:true, onScreenText:"Open File Window");
        MainWindow.Type("{CTRL+O}");
        MainWindow.Type("{ALT+O+O}");
        StartTimer("Open_Window");
        var openWindow = get_file_dialog();

        StopTimer("Open_Window");
        Wait(1);
        openWindow.Click();

        // Navigate to copied PPTX file and press Open, measure time to open the file.
        Wait(seconds:3, showOnScreen:true, onScreenText:"Open File");
        var fileNameBox = openWindow.FindControl(className: "Edit:Edit", title: "File name:");
        fileNameBox.Click();
        Wait(1);
        ScriptHelpers.SetTextBoxText(this, fileNameBox ,$"{temp}\\LoginPI\\loginvsi.pptx", cpm:600);
        Wait(1);
        openWindow.FindControl(className : "SplitButton:Button", title : "&Open").Click();
        StartTimer("Open_Powerpoint_Document");
        var newPowerpoint = FindWindow(className : "Win32 Window:PPTFrameClass", title : "loginvsi*", processName : "POWERPNT");
        newPowerpoint.Focus();
        newPowerpoint.FindControl(className : "TabItem:NetUIRibbonTab", title : "Insert");
        StopTimer("Open_Powerpoint_Document");
        int previewMonitorIndex = PlaceNext(newPowerpoint, "Microsoft PowerPoint");

        if (appWasLeftOpen)
        {
            MainWindow.Close();
            Wait(1);
        }

        //Scroll through Powerpoint presentation
        Wait(seconds:3, showOnScreen:true, onScreenText:"Scroll");
        newPowerpoint.Focus();
        newPowerpoint.Type("{PAGEDOWN}".Repeat(6),cpm:100);
        Wait(2);
        newPowerpoint.Type("{PAGEUP}".Repeat(3),cpm:100);
        Wait(2);
        newPowerpoint.Type("{PAGEDOWN}".Repeat(3),cpm:100);
        Wait(2);
        newPowerpoint.Type("{PAGEUP}".Repeat(6),cpm:100);
        Wait(2);


        newPowerpoint.Minimize();
        Wait(2);
        newPowerpoint.Maximize();
        PlaceOnMonitor(newPowerpoint, "Microsoft PowerPoint", previewMonitorIndex);

        //Reformat slides to 16:9
        Wait(seconds:3, showOnScreen:true, onScreenText:"Reformat Slide Size");
        newPowerpoint.Type("{ALT+G}");
        newPowerpoint.Type("{ALT+S}");
        Type("{DOWN}");
        Wait(1);
        Type("{ENTER}");
        Wait(1);
        Type("{ENTER}");
        Wait(2);

        //Reformat slides to green
        Wait(seconds:3, showOnScreen:true, onScreenText:"Reformat Slide Color");
        newPowerpoint.Type("{ALT+G}");
        newPowerpoint.Type("{ALT+H}");
        Wait(1);
        Type("{DOWN}");
        Wait(1);
        Type("{ENTER}");
        Wait(5);

        //Reformat first slide transition
        Wait(seconds:3, showOnScreen:true, onScreenText:"Reformat Slide Transition");
        newPowerpoint.Type("{ALT+K}");
        newPowerpoint.Type("{ALT+T}");
        Wait(2);
        Type("{DOWN}");
        Wait(1);
        Type("{DOWN}");
        Wait(1);
        Type("{LEFT}");
        Wait(1);
        Type("{LEFT}");
        Wait(1);
        Type("{ENTER}");
        Wait(2);

        // Let's do a slideshow
        Wait(seconds:3, showOnScreen:true, onScreenText:"Slideshow");
        newPowerpoint.Type("{F5}",cpm:0);
        Wait(10);
        Type("{DOWN}");
        Wait(2);
        Type("{DOWN}");
        Wait(2);
        Type("{DOWN}");
        Wait(2);
        Type("{DOWN}");
        Wait(2);
        Type("{DOWN}");
        Wait(2);
        Type("{ESC}");
        Wait(2);
        Type("{HOME}");
        Wait(2);

        // Saving the file in temp
        Wait(seconds:3, showOnScreen:true, onScreenText:"Saving");
        newPowerpoint.Type("{F12}");
        Wait(1);

        var filename = $"{temp}\\LoginPI\\{newDocName}.pptx";
        // Remove file if it already exists
        if (FileExists(filename))
        {
            Log("Removing file");
            RemoveFile(path: filename);
        }
        else
        {
            Log("File already removed");
        }

        // Saving the file in temp
        var saveAs = get_file_dialog();

        fileNameBox = saveAs.FindControl(className: "Edit:Edit", title: "File name:");
        fileNameBox.Click();
        Wait(1);
        ScriptHelpers.SetTextBoxText(this, fileNameBox, filename, cpm: 300);
        StartTimer("Saving_file");
        saveAs.Type("{ENTER}");
        FindWindow(title: $"{newDocName}*", processName: "POWERPNT");
        StopTimer("Saving_file");
        Wait(2);

        // Stop application
        Wait(seconds:3, showOnScreen:true, onScreenText:"Stop App");
        Wait(2);
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

    private void SkipFirstRunDialogs()
    {
        var dialog = FindWindow(className: "Win32 Window:NUIDialog", processName: "POWERPNT", continueOnError: true, timeout: 1);
        while (dialog != null)
        {
            dialog.Close();
            dialog = FindWindow(className: "Win32 Window:NUIDialog", processName: "POWERPNT", continueOnError: true, timeout: 10);
        }
    }

    private IWindow get_file_dialog()
    {
        var dialog = FindWindow(className: "Win32 Window:#32770", processName: "POWERPNT", continueOnError: true, timeout:10);
        if (dialog is null)
        {
            ABORT("File dialog could not be found");
        }
        return dialog;
    }
}

public static class ScriptHelpers
{
    ///
    /// This method types the given text to the textbox (any existing text is cleared)
    /// After typing, it confirms the resulting value.
    /// If it does not match, it will clear the textbox and try again
    ///
    public static void SetTextBoxText(ScriptBase script, IWindow textBox, string text, int cpm=800)
    {
        var numTries = 1;
        string currentText = null;
        do
        {
            textBox.Type("{CTRL+a}");
            script.Wait(0.5);
            textBox.Type(text, cpm: cpm);
            script.Wait(1);
            currentText = textBox.GetText();
            if (currentText != text)
                script.CreateEvent($"Typing error in attempt {numTries}", $"Expected '{text}', got '{currentText}'");
        }
        while (++numTries < 5 && currentText != text);
        if (currentText != text)
            script.ABORT($"Unable to set the correct text '{text}', got '{currentText}'");
    }
}

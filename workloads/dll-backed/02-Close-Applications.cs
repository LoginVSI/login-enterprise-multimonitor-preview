// TARGET:none
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;

public class DllMultiMonitorPreviewCloseApplications : ScriptBase
{
    private const int WindowTimeoutSeconds = 2;

    private void Execute()
    {
        CloseUniqueNotepadWindow();
        CloseUniquePaintWindow();
        CloseUniqueEdgeWindow();
        Log("Preview cleanup complete. Ambiguous matches, if any, were deliberately left untouched.");
    }

    private void CloseUniqueNotepadWindow()
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(processName: "notepad", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        if (CloseOnlyMatch(candidate, count, "Notepad"))
        {
            Log("Notepad cleanup verification: RemainingMatches=" + CountNotepadWindows() + ".");
        }
    }

    private void CloseUniquePaintWindow()
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:MSPaintApp", processName: "mspaint", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        if (CloseOnlyMatch(candidate, count, "Paint"))
        {
            Log("Paint cleanup verification: RemainingMatches=" + CountPaintWindows() + ".");
        }
    }

    private void CloseUniqueEdgeWindow()
    {
        IWindow candidate = null;
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:Chrome_WidgetWin_1", processName: "msedge", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            candidate = window;
            count++;
        }

        if (CloseOnlyMatch(candidate, count, "Microsoft Edge"))
        {
            Log("Microsoft Edge cleanup verification: RemainingMatches=" + CountEdgeWindows() + ".");
        }
    }

    private bool CloseOnlyMatch(IWindow candidate, int count, string applicationName)
    {
        if (count == 0)
        {
            Log(applicationName + " cleanup: no matching base window was present; treating it as already closed.");
            return false;
        }

        if (count > 1)
        {
            Log(applicationName + " cleanup skipped: " + count + " matching base windows make ownership ambiguous.");
            return false;
        }

        try
        {
            Log(applicationName + " cleanup: closing the sole matching base window.");
            candidate.Close();
            Wait(1);
            Log(applicationName + " cleanup: close request completed.");
            return true;
        }
        catch
        {
            Log(applicationName + " cleanup: the close request did not complete normally; no broad process termination was attempted.");
            return false;
        }
    }

    private int CountNotepadWindows()
    {
        int count = 0;
        var windows = FindWindows(processName: "notepad", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private int CountPaintWindows()
    {
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:MSPaintApp", processName: "mspaint", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }

    private int CountEdgeWindows()
    {
        int count = 0;
        var windows = FindWindows(className: "Win32 Window:Chrome_WidgetWin_1", processName: "msedge", timeout: WindowTimeoutSeconds);
        foreach (IWindow window in windows)
        {
            count++;
        }

        return count;
    }
}

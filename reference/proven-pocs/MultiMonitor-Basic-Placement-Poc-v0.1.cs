// TARGET:notepad.exe
// START_IN:

/////////////
// Multi-Monitor Round-Robin POC
// Version: 0.1.0
/////////////

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class MultiMonitorRoundRobinPoc : ScriptBase
{
    // =====================================================
    // User variables
    // =====================================================

    // How long to leave the applications visible for inspection.
    private int AppLingerSeconds = 2;

    // How long to wait for each application window.
    private int WindowFindTimeoutSeconds = 15;

    // Small pause after moving a window before maximizing/verifying it.
    private double PlacementDelaySeconds = 0.5;

    // Applications used by this POC.
    private string FirstApplicationExecutable = "notepad.exe";
    private string FirstApplicationProcessName = "notepad";
    private string FirstApplicationDisplayName = "Notepad";

    private string SecondApplicationExecutable = "mspaint.exe";
    private string SecondApplicationProcessName = "mspaint";
    private string SecondApplicationDisplayName = "Paint";

    // =====================================================
    // Win32 definitions
    // =====================================================

    private const uint MONITORINFOF_PRIMARY = 0x00000001;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    private const int SW_RESTORE = 9;
    private const int SW_MAXIMIZE = 3;

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int Size;
        public RECT Monitor;
        public RECT WorkArea;
        public uint Flags;
    }

    private delegate bool MonitorEnumCallback(
        IntPtr monitorHandle,
        IntPtr deviceContext,
        ref RECT bounds,
        IntPtr userData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr deviceContext,
        IntPtr clippingRectangle,
        MonitorEnumCallback callback,
        IntPtr userData);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(
        IntPtr monitorHandle,
        ref MONITORINFO monitorInfo);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(
        IntPtr windowHandle,
        int command);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(
        IntPtr windowHandle,
        uint flags);

    // =====================================================
    // Minimal internal monitor model
    // =====================================================

    private class MonitorTarget
    {
        public IntPtr Handle;
        public bool IsPrimary;
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }
    }

    // =====================================================
    // Login Enterprise entry point
    // =====================================================

    private void Execute()
    {
        Log("Starting multi-monitor round-robin POC.");

        List<MonitorTarget> monitors = DiscoverMonitors();

        if (monitors.Count == 0)
        {
            ABORT("Windows did not report any monitors.");
            return;
        }

        OrderMonitors(monitors);
        LogMonitors(monitors);

        int nextMonitorIndex = 0;

        IWindow notepadWindow = null;
        IWindow paintWindow = null;

        try
        {
            notepadWindow = LaunchAndPlace(
                FirstApplicationExecutable,
                FirstApplicationProcessName,
                FirstApplicationDisplayName,
                monitors[nextMonitorIndex],
                nextMonitorIndex);

            nextMonitorIndex =
                (nextMonitorIndex + 1) % monitors.Count;

            paintWindow = LaunchAndPlace(
                SecondApplicationExecutable,
                SecondApplicationProcessName,
                SecondApplicationDisplayName,
                monitors[nextMonitorIndex],
                nextMonitorIndex);

            nextMonitorIndex =
                (nextMonitorIndex + 1) % monitors.Count;

            Log(
                "Placement complete. Waiting " +
                AppLingerSeconds +
                " seconds for inspection.");

            Wait(
                seconds: AppLingerSeconds,
                showOnScreen: true,
                onScreenText: "Inspect Notepad and Paint placement");
        }
        finally
        {
            CloseApplication(
                paintWindow,
                SecondApplicationProcessName,
                SecondApplicationDisplayName);

            CloseApplication(
                notepadWindow,
                FirstApplicationProcessName,
                FirstApplicationDisplayName);
        }

        Log("Multi-monitor round-robin POC completed.");
    }

    // =====================================================
    // Monitor discovery
    // =====================================================

    private List<MonitorTarget> DiscoverMonitors()
    {
        var monitors = new List<MonitorTarget>();

        MonitorEnumCallback callback =
            delegate (
                IntPtr monitorHandle,
                IntPtr deviceContext,
                ref RECT bounds,
                IntPtr userData)
            {
                MONITORINFO info = new MONITORINFO();
                info.Size = Marshal.SizeOf(typeof(MONITORINFO));

                if (GetMonitorInfo(monitorHandle, ref info))
                {
                    monitors.Add(new MonitorTarget
                    {
                        Handle = monitorHandle,
                        IsPrimary =
                            (info.Flags & MONITORINFOF_PRIMARY) != 0,
                        Left = info.Monitor.Left,
                        Top = info.Monitor.Top,
                        Right = info.Monitor.Right,
                        Bottom = info.Monitor.Bottom
                    });
                }

                return true;
            };

        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            callback,
            IntPtr.Zero);

        return monitors;
    }

    private void OrderMonitors(List<MonitorTarget> monitors)
    {
        monitors.Sort(
            delegate (MonitorTarget first, MonitorTarget second)
            {
                if (first.IsPrimary && !second.IsPrimary)
                {
                    return -1;
                }

                if (!first.IsPrimary && second.IsPrimary)
                {
                    return 1;
                }

                int xComparison =
                    first.Left.CompareTo(second.Left);

                if (xComparison != 0)
                {
                    return xComparison;
                }

                return first.Top.CompareTo(second.Top);
            });
    }

    private void LogMonitors(List<MonitorTarget> monitors)
    {
        Log("Discovered " + monitors.Count + " monitor(s).");

        for (int i = 0; i < monitors.Count; i++)
        {
            MonitorTarget monitor = monitors[i];

            Log(
                "Monitor " + i +
                ": Primary=" + monitor.IsPrimary +
                ", Position=" + monitor.Left + "," + monitor.Top +
                ", Size=" + monitor.Width + "x" + monitor.Height);
        }
    }

    // =====================================================
    // Application placement
    // =====================================================

    private IWindow LaunchAndPlace(
        string executable,
        string processName,
        string displayName,
        MonitorTarget targetMonitor,
        int targetMonitorIndex)
    {
        Log(
            "Launching " +
            displayName +
            " for round-robin monitor " +
            targetMonitorIndex +
            ".");

        Process.Start(
            new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = true
            });

        IWindow applicationWindow = FindWindow(
            processName: processName,
            timeout: WindowFindTimeoutSeconds);

        IntPtr windowHandle =
            applicationWindow.NativeWindowHandle;

        IntPtr initialMonitor =
            MonitorFromWindow(
                windowHandle,
                MONITOR_DEFAULTTONEAREST);

        Log(
            displayName +
            " initially opened on monitor handle " +
            initialMonitor +
            ".");

        applicationWindow.Restore();
        Wait(PlacementDelaySeconds);

        bool moved = SetWindowPos(
            windowHandle,
            IntPtr.Zero,
            targetMonitor.Left,
            targetMonitor.Top,
            targetMonitor.Width,
            targetMonitor.Height,
            SWP_NOZORDER | SWP_SHOWWINDOW);

        Log(
            displayName +
            " SetWindowPos result: " +
            moved +
            ".");

        Wait(PlacementDelaySeconds);

        ShowWindow(
            windowHandle,
            SW_MAXIMIZE);

        Wait(PlacementDelaySeconds);

        IntPtr verifiedMonitor =
            MonitorFromWindow(
                windowHandle,
                MONITOR_DEFAULTTONEAREST);

        if (verifiedMonitor == targetMonitor.Handle)
        {
            Log(
                displayName +
                " verified on round-robin monitor " +
                targetMonitorIndex +
                ".");
        }
        else
        {
            Log(
                "WARNING: " +
                displayName +
                " did not verify on the requested monitor.");
        }

        return applicationWindow;
    }

    // =====================================================
    // Cleanup
    // =====================================================

    private void CloseApplication(
        IWindow applicationWindow,
        string processName,
        string displayName)
    {
        if (applicationWindow != null)
        {
            try
            {
                Log("Closing " + displayName + " normally.");
                applicationWindow.Close();
                Wait(1);
            }
            catch
            {
                Log(
                    displayName +
                    " did not close normally.");
            }
        }

        Process[] remainingProcesses =
            Process.GetProcessesByName(processName);

        foreach (Process process in remainingProcesses)
        {
            try
            {
                Log(
                    "Force-closing remaining " +
                    displayName +
                    " process.");

                process.Kill();
            }
            catch
            {
                Log(
                    "Could not force-close a remaining " +
                    displayName +
                    " process.");
            }
        }
    }
}
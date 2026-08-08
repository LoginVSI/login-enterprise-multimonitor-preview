// TARGET:notepad.exe
// START_IN:

/////////////
// Multi-Monitor Persistent State POC
//
// Proves:
// - Monitor discovery and primary-first ordering
// - Minimal file-backed round-robin state
// - Reusable one-line placement method
// - Placement verification
// - State continuity across separate test phases
// - Placement timing
// - One or multiple monitor support
//
// Version: 0.3.0
/////////////

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

public class MultiMonitorPersistentStatePoc : ScriptBase
{
    // =====================================================
    // User variables
    // =====================================================

    // Reset the state file once when this POC begins.
    // The second phase intentionally continues from phase one.
    private bool ResetStateAtStart = true;

    // Run a second phase to prove state continuity.
    private bool RunSecondPhase = true;

    // Time to visually inspect each completed phase.
    private int PhaseLingerSeconds = 5;

    // Maximum time to find an application window.
    private int WindowFindTimeoutSeconds = 15;

    // Small waits around restore, move, maximize, and verification.
    private double PlacementDelaySeconds = 0.35;

    // State location under the normal Login Enterprise temp folder.
    private string StateRelativeFolder = @"LoginPI\MultiMonitor";
    private string StateFileName = "state.txt";

    // =====================================================
    // Win32 constants
    // =====================================================

    private const uint MONITORINFOF_PRIMARY = 0x00000001;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    private const int SW_MAXIMIZE = 3;

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // =====================================================
    // Win32 structures and imports
    // =====================================================

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
        ref RECT monitorBounds,
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

    [DllImport("user32.dll", SetLastError = true)]
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
    // Internal models
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

    private class PlacementState
    {
        public int MonitorCount;
        public int LastUsedIndex;
    }

    private class ApplicationSpec
    {
        public string Executable;
        public string Arguments;
        public string ProcessName;
        public string DisplayName;
        public string GracefulExitText;
    }

    private class LaunchedApplication
    {
        public ApplicationSpec Spec;
        public IWindow Window;
    }

    // =====================================================
    // Login Enterprise entry point
    // =====================================================

    private void Execute()
    {
        Log("==================================================");
        Log("Starting multi-monitor persistent-state POC v0.3.");
        Log("==================================================");

        string stateFilePath = GetStateFilePath();

        List<MonitorTarget> monitors =
            DiscoverAndOrderMonitors();

        if (monitors.Count == 0)
        {
            ABORT("Windows did not report any usable monitors.");
            return;
        }

        LogMonitors(monitors);

        if (ResetStateAtStart)
        {
            InitializeState(
                stateFilePath,
                monitors.Count);
        }

        RunTestPhase(
            phaseNumber: 1,
            stateFilePath: stateFilePath);

        if (RunSecondPhase)
        {
            Log("");
            Log("State is intentionally not being reset.");
            Log("Starting phase 2 to prove file-backed continuity.");

            RunTestPhase(
                phaseNumber: 2,
                stateFilePath: stateFilePath);
        }

        LogStateFile(stateFilePath);

        Log("==================================================");
        Log("Multi-monitor persistent-state POC completed.");
        Log("State file retained at: " + stateFilePath);
        Log("==================================================");
    }

    // =====================================================
    // Test phase
    // =====================================================

    private void RunTestPhase(
        int phaseNumber,
        string stateFilePath)
    {
        Log("==================================================");
        Log("Starting test phase " + phaseNumber + ".");
        Log("==================================================");

        List<ApplicationSpec> applications =
            BuildApplicationList();

        var launchedApplications =
            new List<LaunchedApplication>();

        try
        {
            foreach (ApplicationSpec application in applications)
            {
                LaunchedApplication launched =
                    LaunchFindAndPlace(
                        application,
                        stateFilePath);

                launchedApplications.Add(launched);
            }

            LogStateFile(stateFilePath);

            Log(
                "Phase " +
                phaseNumber +
                " placement complete. Waiting " +
                PhaseLingerSeconds +
                " seconds for visual inspection.");

            Wait(
                seconds: PhaseLingerSeconds,
                showOnScreen: true,
                onScreenText:
                    "Inspect multi-monitor placement for phase " +
                    phaseNumber);
        }
        finally
        {
            for (int i = launchedApplications.Count - 1; i >= 0; i--)
            {
                CloseApplication(
                    launchedApplications[i]);
            }
        }

        Log("Completed test phase " + phaseNumber + ".");
    }

    private List<ApplicationSpec> BuildApplicationList()
    {
        return new List<ApplicationSpec>
        {
            new ApplicationSpec
            {
                Executable = "notepad.exe",
                Arguments = "",
                ProcessName = "notepad",
                DisplayName = "Notepad",
                GracefulExitText = ""
            },

            new ApplicationSpec
            {
                Executable = "mspaint.exe",
                Arguments = "",
                ProcessName = "mspaint",
                DisplayName = "Paint",
                GracefulExitText = ""
            },

            new ApplicationSpec
            {
                Executable = "cmd.exe",
                Arguments = "/k title Login Enterprise MultiMonitor POC",
                ProcessName = "cmd",
                DisplayName = "Command Prompt",
                GracefulExitText = "exit{ENTER}"
            }
        };
    }

    // =====================================================
    // State-file handling
    // =====================================================

    private string GetStateFilePath()
    {
        string tempPath =
            GetEnvironmentVariable("TEMP");

        string stateDirectory =
            Path.Combine(
                tempPath,
                StateRelativeFolder);

        Directory.CreateDirectory(
            stateDirectory);

        string stateFilePath =
            Path.Combine(
                stateDirectory,
                StateFileName);

        Log(
            "Multi-monitor state file: " +
            stateFilePath);

        return stateFilePath;
    }

    private void InitializeState(
        string stateFilePath,
        int monitorCount)
    {
        PlacementState state =
            new PlacementState
            {
                MonitorCount = monitorCount,
                LastUsedIndex = -1
            };

        WriteState(
            stateFilePath,
            state);

        Log(
            "Initialized state. MonitorCount=" +
            monitorCount +
            ", LastUsedIndex=-1.");
    }

    private PlacementState ReadState(
        string stateFilePath,
        int currentMonitorCount)
    {
        PlacementState defaultState =
            new PlacementState
            {
                MonitorCount = currentMonitorCount,
                LastUsedIndex = -1
            };

        if (!File.Exists(stateFilePath))
        {
            Log(
                "State file was missing. " +
                "Creating a new state file.");

            WriteState(
                stateFilePath,
                defaultState);

            return defaultState;
        }

        try
        {
            PlacementState state =
                new PlacementState
                {
                    MonitorCount = currentMonitorCount,
                    LastUsedIndex = -1
                };

            string[] lines =
                File.ReadAllLines(stateFilePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("MonitorCount="))
                {
                    int parsedValue;

                    if (int.TryParse(
                        line.Substring("MonitorCount=".Length),
                        out parsedValue))
                    {
                        state.MonitorCount =
                            parsedValue;
                    }
                }
                else if (line.StartsWith("LastUsedIndex="))
                {
                    int parsedValue;

                    if (int.TryParse(
                        line.Substring("LastUsedIndex=".Length),
                        out parsedValue))
                    {
                        state.LastUsedIndex =
                            parsedValue;
                    }
                }
            }

            bool invalidState =
                state.MonitorCount <= 0 ||
                state.LastUsedIndex < -1 ||
                state.LastUsedIndex >= state.MonitorCount;

            if (invalidState)
            {
                Log(
                    "State file contained invalid values. " +
                    "Resetting round-robin state.");

                WriteState(
                    stateFilePath,
                    defaultState);

                return defaultState;
            }

            if (state.MonitorCount != currentMonitorCount)
            {
                Log(
                    "Monitor count changed from " +
                    state.MonitorCount +
                    " to " +
                    currentMonitorCount +
                    ". Resetting round-robin state.");

                WriteState(
                    stateFilePath,
                    defaultState);

                return defaultState;
            }

            return state;
        }
        catch (Exception exception)
        {
            Log(
                "Unable to read state file: " +
                exception.Message +
                ". Resetting state.");

            WriteState(
                stateFilePath,
                defaultState);

            return defaultState;
        }
    }

    private void WriteState(
        string stateFilePath,
        PlacementState state)
    {
        string temporaryPath =
            stateFilePath + ".tmp";

        string[] lines =
        {
            "MonitorCount=" + state.MonitorCount,
            "LastUsedIndex=" + state.LastUsedIndex
        };

        File.WriteAllLines(
            temporaryPath,
            lines);

        if (File.Exists(stateFilePath))
        {
            File.Delete(stateFilePath);
        }

        File.Move(
            temporaryPath,
            stateFilePath);
    }

    private void LogStateFile(
        string stateFilePath)
    {
        Log("Current state-file contents:");

        if (!File.Exists(stateFilePath))
        {
            Log("  State file does not exist.");
            return;
        }

        string[] lines =
            File.ReadAllLines(stateFilePath);

        foreach (string line in lines)
        {
            Log("  " + line);
        }
    }

    // =====================================================
    // Monitor discovery and ordering
    // =====================================================

    private List<MonitorTarget> DiscoverAndOrderMonitors()
    {
        List<MonitorTarget> monitors =
            DiscoverMonitors();

        OrderMonitorsPrimaryFirst(
            monitors);

        return monitors;
    }

    private List<MonitorTarget> DiscoverMonitors()
    {
        var monitors =
            new List<MonitorTarget>();

        MonitorEnumCallback callback =
            delegate (
                IntPtr monitorHandle,
                IntPtr deviceContext,
                ref RECT monitorBounds,
                IntPtr userData)
            {
                MONITORINFO info =
                    new MONITORINFO();

                info.Size =
                    Marshal.SizeOf(
                        typeof(MONITORINFO));

                if (GetMonitorInfo(
                    monitorHandle,
                    ref info))
                {
                    monitors.Add(
                        new MonitorTarget
                        {
                            Handle =
                                monitorHandle,

                            IsPrimary =
                                (info.Flags &
                                 MONITORINFOF_PRIMARY) != 0,

                            Left =
                                info.Monitor.Left,

                            Top =
                                info.Monitor.Top,

                            Right =
                                info.Monitor.Right,

                            Bottom =
                                info.Monitor.Bottom
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

    private void OrderMonitorsPrimaryFirst(
        List<MonitorTarget> monitors)
    {
        monitors.Sort(
            delegate (
                MonitorTarget first,
                MonitorTarget second)
            {
                if (first.IsPrimary &&
                    !second.IsPrimary)
                {
                    return -1;
                }

                if (!first.IsPrimary &&
                    second.IsPrimary)
                {
                    return 1;
                }

                int xComparison =
                    first.Left.CompareTo(
                        second.Left);

                if (xComparison != 0)
                {
                    return xComparison;
                }

                return first.Top.CompareTo(
                    second.Top);
            });
    }

    private void LogMonitors(
        List<MonitorTarget> monitors)
    {
        Log(
            "Discovered " +
            monitors.Count +
            " monitor(s).");

        for (int i = 0; i < monitors.Count; i++)
        {
            MonitorTarget monitor =
                monitors[i];

            Log(
                "Monitor " +
                i +
                ": Primary=" +
                monitor.IsPrimary +
                ", Position=" +
                monitor.Left +
                "," +
                monitor.Top +
                ", Size=" +
                monitor.Width +
                "x" +
                monitor.Height);
        }
    }

    // =====================================================
    // App launch and universal placement
    // =====================================================

    private LaunchedApplication LaunchFindAndPlace(
        ApplicationSpec application,
        string stateFilePath)
    {
        Log("--------------------------------------------------");
        Log("Launching " + application.DisplayName + ".");

        Process.Start(
            new ProcessStartInfo
            {
                FileName =
                    application.Executable,

                Arguments =
                    application.Arguments,

                UseShellExecute =
                    true
            });

        // Login Enterprise metalanguage finds the target window.
        IWindow applicationWindow =
            FindWindow(
                processName:
                    application.ProcessName,

                timeout:
                    WindowFindTimeoutSeconds);

        // Universal one-line placement call.
        PlaceOnNextMonitor(
            applicationWindow,
            application.DisplayName,
            stateFilePath);

        return new LaunchedApplication
        {
            Spec = application,
            Window = applicationWindow
        };
    }

    private void PlaceOnNextMonitor(
        IWindow applicationWindow,
        string displayName,
        string stateFilePath)
    {
        Stopwatch placementTimer =
            Stopwatch.StartNew();

        List<MonitorTarget> monitors =
            DiscoverAndOrderMonitors();

        if (monitors.Count == 0)
        {
            ABORT(
                "No monitors were available while placing " +
                displayName +
                ".");

            return;
        }

        PlacementState state =
            ReadState(
                stateFilePath,
                monitors.Count);

        int nextMonitorIndex =
            (state.LastUsedIndex + 1) %
            monitors.Count;

        MonitorTarget targetMonitor =
            monitors[nextMonitorIndex];

        IntPtr windowHandle =
            applicationWindow.NativeWindowHandle;

        IntPtr initialMonitorHandle =
            MonitorFromWindow(
                windowHandle,
                MONITOR_DEFAULTTONEAREST);

        int initialMonitorIndex =
            FindMonitorIndex(
                monitors,
                initialMonitorHandle);

        Log(
            displayName +
            " initially opened on monitor " +
            initialMonitorIndex +
            ".");

        Log(
            "Round-robin destination for " +
            displayName +
            ": monitor " +
            nextMonitorIndex +
            ".");

        applicationWindow.Restore();

        Wait(
            PlacementDelaySeconds);

        bool moved =
            SetWindowPos(
                windowHandle,
                IntPtr.Zero,
                targetMonitor.Left,
                targetMonitor.Top,
                targetMonitor.Width,
                targetMonitor.Height,
                SWP_NOZORDER |
                SWP_SHOWWINDOW);

        if (!moved)
        {
            int errorCode =
                Marshal.GetLastWin32Error();

            Log(
                "WARNING: SetWindowPos failed for " +
                displayName +
                ". Win32 error=" +
                errorCode +
                ".");
        }

        Wait(
            PlacementDelaySeconds);

        ShowWindow(
            windowHandle,
            SW_MAXIMIZE);

        Wait(
            PlacementDelaySeconds);

        IntPtr verifiedMonitorHandle =
            MonitorFromWindow(
                windowHandle,
                MONITOR_DEFAULTTONEAREST);

        if (verifiedMonitorHandle ==
            targetMonitor.Handle)
        {
            state.MonitorCount =
                monitors.Count;

            state.LastUsedIndex =
                nextMonitorIndex;

            WriteState(
                stateFilePath,
                state);

            placementTimer.Stop();

            Log(
                displayName +
                " verified on monitor " +
                nextMonitorIndex +
                ".");

            Log(
                displayName +
                " placement overhead: " +
                placementTimer.ElapsedMilliseconds +
                " ms.");

            Log(
                "Updated state: MonitorCount=" +
                state.MonitorCount +
                ", LastUsedIndex=" +
                state.LastUsedIndex +
                ".");
        }
        else
        {
            placementTimer.Stop();

            Log(
                "WARNING: " +
                displayName +
                " did not verify on monitor " +
                nextMonitorIndex +
                ".");

            Log(
                "State was not advanced. Placement attempt took " +
                placementTimer.ElapsedMilliseconds +
                " ms.");
        }
    }

    private int FindMonitorIndex(
        List<MonitorTarget> monitors,
        IntPtr monitorHandle)
    {
        for (int i = 0; i < monitors.Count; i++)
        {
            if (monitors[i].Handle ==
                monitorHandle)
            {
                return i;
            }
        }

        return -1;
    }

    // =====================================================
    // Cleanup
    // =====================================================

    private void CloseApplication(
        LaunchedApplication application)
    {
        Log(
            "Closing " +
            application.Spec.DisplayName +
            ".");

        try
        {
            if (!string.IsNullOrEmpty(
                application.Spec.GracefulExitText))
            {
                application.Window.Type(
                    application.Spec.GracefulExitText,
                    cpm: 0);

                Wait(1);
            }
            else
            {
                application.Window.Close();

                Wait(1);
            }
        }
        catch (Exception exception)
        {
            Log(
                "Graceful close failed for " +
                application.Spec.DisplayName +
                ": " +
                exception.Message);
        }

        Process[] remainingProcesses =
            Process.GetProcessesByName(
                application.Spec.ProcessName);

        foreach (Process process in remainingProcesses)
        {
            try
            {
                process.Refresh();

                if (!process.HasExited)
                {
                    Log(
                        "Force-closing remaining " +
                        application.Spec.DisplayName +
                        " process.");

                    process.Kill();
                }
            }
            catch
            {
                // The process may have exited between enumeration and refresh.
            }
        }
    }
}
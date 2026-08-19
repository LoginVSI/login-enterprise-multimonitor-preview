// TARGET:notepad.exe
// START_IN:

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class MultiMonitorPreviewInitializeNotepadPaint : ScriptBase
{
    private const int WindowTimeoutSeconds = 30;
    private const int StabilizationDelayMilliseconds = 350;

    private void Execute()
    {
        string statePath = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor", "state.txt");
        PreviewMultiMonitorSupport placement = new PreviewMultiMonitorSupport();

        PreviewPlacementResult reset = placement.ResetState(statePath);
        RequireSuccess(reset);

        START(processName: "notepad", timeout: WindowTimeoutSeconds);
        IWindow notepad = MainWindow;
        RequireSuccess(placement.PlaceNext(notepad, "Notepad", statePath, true, StabilizationDelayMilliseconds));

        ShellExecute("mspaint.exe", waitForProcessEnd: false, timeout: WindowTimeoutSeconds, forceKillOnExit: false);
        IWindow paint = FindWindow(processName: "mspaint", timeout: WindowTimeoutSeconds);
        RequireSuccess(placement.PlaceNext(paint, "Paint", statePath, true, StabilizationDelayMilliseconds));

        Log("Phase 1 complete. Round-robin state is retained for the next workload file.");
    }

    private void RequireSuccess(PreviewPlacementResult result)
    {
        Log(result.ToLogMessage());
        if (!result.Success)
        {
            ABORT(result.ApplicationName + " multi-monitor operation failed: " + result.Message);
        }
    }
}

internal sealed class PreviewMultiMonitorSupport
{
    private const uint MonitorInfoPrimary = 0x00000001;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private const uint SetWindowNoZOrder = 0x0004;
    private const uint SetWindowShow = 0x0040;
    internal PreviewPlacementResult ResetState(string statePath)
    {
        Stopwatch timer = Stopwatch.StartNew();
        PreviewPlacementResult result = new PreviewPlacementResult("State reset");
        try
        {
            List<PreviewMonitor> monitors = DiscoverMonitors();
            result.MonitorCount = monitors.Count;
            if (monitors.Count == 0)
            {
                return Fail(result, timer, "Windows did not report an active monitor.");
            }

            using (AcquireStateLock(statePath))
            {
                WriteState(statePath, monitors.Count, -1);
            }

            result.Success = true;
            result.Message = "State initialized with LastUsedIndex=-1.";
            return Finish(result, timer);
        }
        catch (Exception exception)
        {
            return Fail(result, timer, exception.Message);
        }
    }

    internal PreviewPlacementResult PlaceNext(IWindow window, string applicationName, string statePath, bool maximize, int delayMilliseconds)
    {
        Stopwatch timer = Stopwatch.StartNew();
        PreviewPlacementResult result = new PreviewPlacementResult(applicationName);
        try
        {
            List<PreviewMonitor> monitors = DiscoverMonitors();
            result.MonitorCount = monitors.Count;
            if (monitors.Count == 0)
            {
                return Fail(result, timer, "Windows did not report an active monitor.");
            }

            using (AcquireStateLock(statePath))
            {
                PreviewState state = ReadAndRepairState(statePath, monitors.Count);
                int targetIndex = (state.LastUsedIndex + 1) % monitors.Count;
                PreviewMonitor target = monitors[targetIndex];
                result.TargetMonitorIndex = targetIndex;
                IntPtr windowHandle = window.NativeWindowHandle;
                if (windowHandle == IntPtr.Zero)
                {
                    return Fail(result, timer, "Login Enterprise returned a zero NativeWindowHandle.");
                }

                result.InitialMonitorIndex = FindMonitorIndex(monitors, MonitorFromWindow(windowHandle, MonitorDefaultToNearest));
                window.Restore();
                Stabilize(delayMilliseconds);

                bool moved = SetWindowPos(windowHandle, IntPtr.Zero, target.Left, target.Top, target.Width, target.Height, SetWindowNoZOrder | SetWindowShow);
                if (!moved)
                {
                    result.Win32ErrorCode = Marshal.GetLastWin32Error();
                    return Fail(result, timer, "SetWindowPos failed with Win32 error " + result.Win32ErrorCode + ".");
                }

                Stabilize(delayMilliseconds);
                if (maximize)
                {
                    window.Maximize();
                }

                Stabilize(delayMilliseconds);
                result.VerifiedMonitorIndex = FindMonitorIndex(monitors, MonitorFromWindow(windowHandle, MonitorDefaultToNearest));
                if (result.VerifiedMonitorIndex != targetIndex)
                {
                    return Fail(result, timer, "Placement did not verify on the requested monitor; state was not advanced.");
                }

                WriteState(statePath, monitors.Count, targetIndex);
                result.Success = true;
                result.StateAdvanced = true;
                result.Message = "Placement verified and state advanced.";
            }

            return Finish(result, timer);
        }
        catch (Exception exception)
        {
            return Fail(result, timer, exception.Message);
        }
    }

    private List<PreviewMonitor> DiscoverMonitors()
    {
        List<PreviewMonitor> monitors = new List<PreviewMonitor>();
        MonitorEnumCallback callback = delegate(IntPtr handle, IntPtr deviceContext, ref PreviewRect bounds, IntPtr userData)
        {
            PreviewMonitorInfo info = new PreviewMonitorInfo();
            info.Size = Marshal.SizeOf(typeof(PreviewMonitorInfo));
            if (GetMonitorInfo(handle, ref info))
            {
                monitors.Add(new PreviewMonitor(handle, (info.Flags & MonitorInfoPrimary) != 0, info.Monitor.Left, info.Monitor.Top, info.Monitor.Right, info.Monitor.Bottom));
            }

            return true;
        };

        if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero))
        {
            throw new InvalidOperationException("EnumDisplayMonitors failed with Win32 error " + Marshal.GetLastWin32Error() + ".");
        }

        monitors.Sort(delegate(PreviewMonitor first, PreviewMonitor second)
        {
            if (first.IsPrimary != second.IsPrimary)
            {
                return first.IsPrimary ? -1 : 1;
            }

            int x = first.Left.CompareTo(second.Left);
            if (x != 0) return x;
            int y = first.Top.CompareTo(second.Top);
            if (y != 0) return y;
            int right = first.Right.CompareTo(second.Right);
            if (right != 0) return right;
            int bottom = first.Bottom.CompareTo(second.Bottom);
            return bottom != 0 ? bottom : first.Handle.ToInt64().CompareTo(second.Handle.ToInt64());
        });

        return monitors;
    }

    private PreviewState ReadAndRepairState(string statePath, int monitorCount)
    {
        PreviewState reset = new PreviewState(monitorCount, -1);
        if (!File.Exists(statePath))
        {
            WriteState(statePath, monitorCount, -1);
            return reset;
        }

        try
        {
            int savedCount = 0;
            int savedIndex = -1;
            bool hasCount = false;
            bool hasIndex = false;
            string[] lines = File.ReadAllLines(statePath);
            foreach (string line in lines)
            {
                if (line.StartsWith("MonitorCount=", StringComparison.Ordinal))
                {
                    if (hasCount || !int.TryParse(line.Substring("MonitorCount=".Length), out savedCount))
                    {
                        hasCount = false;
                        break;
                    }
                    hasCount = true;
                }
                else if (line.StartsWith("LastUsedIndex=", StringComparison.Ordinal))
                {
                    if (hasIndex || !int.TryParse(line.Substring("LastUsedIndex=".Length), out savedIndex))
                    {
                        hasIndex = false;
                        break;
                    }
                    hasIndex = true;
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    hasCount = false;
                    break;
                }
            }

            if (!hasCount || !hasIndex || savedCount != monitorCount || savedIndex < -1 || savedIndex >= savedCount)
            {
                WriteState(statePath, monitorCount, -1);
                return reset;
            }

            return new PreviewState(savedCount, savedIndex);
        }
        catch
        {
            WriteState(statePath, monitorCount, -1);
            return reset;
        }
    }

    private void WriteState(string statePath, int monitorCount, int lastUsedIndex)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(statePath));
        Directory.CreateDirectory(directory);
        string temporaryPath = statePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temporaryPath, new string[] { "MonitorCount=" + monitorCount, "LastUsedIndex=" + lastUsedIndex }, new UTF8Encoding(false));
            if (File.Exists(statePath))
            {
                File.Replace(temporaryPath, statePath, null);
            }
            else
            {
                File.Move(temporaryPath, statePath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private PreviewStateLock AcquireStateLock(string statePath)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(statePath));
        Directory.CreateDirectory(directory);
        string lockPath = statePath + ".lock";
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (true)
        {
            try
            {
                return new PreviewStateLock(lockPath, new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None));
            }
            catch (IOException)
            {
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for multi-monitor state access.");
                }

                Thread.Sleep(50);
            }
        }
    }

    private static int FindMonitorIndex(List<PreviewMonitor> monitors, IntPtr handle)
    {
        for (int index = 0; index < monitors.Count; index++)
        {
            if (monitors[index].Handle == handle)
            {
                return index;
            }
        }

        return -1;
    }

    private static void Stabilize(int milliseconds)
    {
        if (milliseconds > 0)
        {
            Thread.Sleep(milliseconds);
        }
    }

    private static PreviewPlacementResult Fail(PreviewPlacementResult result, Stopwatch timer, string message)
    {
        result.Success = false;
        result.Message = message;
        return Finish(result, timer);
    }

    private static PreviewPlacementResult Finish(PreviewPlacementResult result, Stopwatch timer)
    {
        timer.Stop();
        result.ElapsedMilliseconds = timer.ElapsedMilliseconds;
        return result;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PreviewRect { public int Left; public int Top; public int Right; public int Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct PreviewMonitorInfo { public int Size; public PreviewRect Monitor; public PreviewRect WorkArea; public uint Flags; }

    private delegate bool MonitorEnumCallback(IntPtr monitorHandle, IntPtr deviceContext, ref PreviewRect monitorBounds, IntPtr userData);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumDisplayMonitors(IntPtr deviceContext, IntPtr clippingRectangle, MonitorEnumCallback callback, IntPtr userData);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr monitorHandle, ref PreviewMonitorInfo monitorInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);
}

internal sealed class PreviewMonitor
{
    internal PreviewMonitor(IntPtr handle, bool isPrimary, int left, int top, int right, int bottom)
    {
        Handle = handle; IsPrimary = isPrimary; Left = left; Top = top; Right = right; Bottom = bottom;
    }

    internal IntPtr Handle; internal bool IsPrimary; internal int Left; internal int Top; internal int Right; internal int Bottom;
    internal int Width { get { return Right - Left; } }
    internal int Height { get { return Bottom - Top; } }
}

internal sealed class PreviewState
{
    internal PreviewState(int monitorCount, int lastUsedIndex) { MonitorCount = monitorCount; LastUsedIndex = lastUsedIndex; }
    internal int MonitorCount; internal int LastUsedIndex;
}

internal sealed class PreviewStateLock : IDisposable
{
    private readonly string _path;
    private FileStream _stream;

    internal PreviewStateLock(string path, FileStream stream)
    {
        _path = path;
        _stream = stream;
    }

    public void Dispose()
    {
        if (_stream != null)
        {
            _stream.Dispose();
            _stream = null;
        }

        try
        {
            File.Delete(_path);
        }
        catch
        {
            // A zero-byte marker does not provide mutual exclusion if best-effort cleanup is delayed.
        }
    }
}

internal sealed class PreviewPlacementResult
{
    internal PreviewPlacementResult(string applicationName)
    {
        ApplicationName = applicationName; InitialMonitorIndex = -1; TargetMonitorIndex = -1; VerifiedMonitorIndex = -1; Message = string.Empty;
    }

    internal bool Success; internal string ApplicationName; internal int MonitorCount; internal int InitialMonitorIndex;
    internal int TargetMonitorIndex; internal int VerifiedMonitorIndex; internal long ElapsedMilliseconds;
    internal bool StateAdvanced; internal int Win32ErrorCode; internal string Message;

    internal string ToLogMessage()
    {
        return ApplicationName + ": Success=" + Success + ", MonitorCount=" + MonitorCount + ", Initial=" + InitialMonitorIndex +
            ", Target=" + TargetMonitorIndex + ", Verified=" + VerifiedMonitorIndex + ", StateAdvanced=" + StateAdvanced +
            ", ElapsedMs=" + ElapsedMilliseconds + ", Message=" + Message;
    }
}

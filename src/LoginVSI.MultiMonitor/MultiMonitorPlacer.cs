using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace LoginVSI.MultiMonitor
{
    public static class MultiMonitorPlacer
    {
        private const int StateLockTimeoutMilliseconds = 5000;

        public static PlacementResult ResetState(string stateFilePath)
        {
            Stopwatch timer = Stopwatch.StartNew();
            PlacementResult result = NewResult("State reset");

            try
            {
                List<MonitorDescriptor> monitors = NativeMethods.DiscoverAndOrderMonitors();
                result.MonitorCount = monitors.Count;
                if (monitors.Count == 0)
                {
                    return FinishFailure(result, timer, "Windows did not report any active monitors.");
                }

                using (StateFileStore.AcquireExclusiveLock(stateFilePath, StateLockTimeoutMilliseconds))
                {
                    StateFileStore.WriteAtomic(stateFilePath, new PlacementState(monitors.Count, -1));
                }

                result.Success = true;
                result.Message = "State initialized with LastUsedIndex=-1.";
                return Finish(result, timer);
            }
            catch (Exception exception)
            {
                return FinishFailure(result, timer, exception.Message);
            }
        }

        public static PlacementResult PlaceNext(IntPtr windowHandle, string applicationName, string stateFilePath, bool maximize, int stabilizationDelayMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            PlacementResult result = NewResult(applicationName);

            try
            {
                string validationError = ValidatePlacementArguments(windowHandle, stateFilePath, stabilizationDelayMilliseconds);
                if (validationError != null)
                {
                    return FinishFailure(result, timer, validationError);
                }

                using (StateFileStore.AcquireExclusiveLock(stateFilePath, StateLockTimeoutMilliseconds))
                {
                    List<MonitorDescriptor> monitors = NativeMethods.DiscoverAndOrderMonitors();
                    result.MonitorCount = monitors.Count;
                    if (monitors.Count == 0)
                    {
                        return FinishFailure(result, timer, "Windows did not report any active monitors.");
                    }

                    StateLoadResult stateResult = StateFileStore.LoadAndRepair(stateFilePath, monitors.Count);
                    int targetIndex = RoundRobinLogic.GetNextIndex(stateResult.State.LastUsedIndex, monitors.Count);
                    PlaceWindow(windowHandle, monitors, targetIndex, maximize, stabilizationDelayMilliseconds, result);

                    if (result.Success)
                    {
                        StateFileStore.WriteAtomic(stateFilePath, new PlacementState(monitors.Count, targetIndex));
                        result.StateAdvanced = true;
                        result.Message = stateResult.WasReset
                            ? stateResult.Message + " Placement verified and state advanced."
                            : "Placement verified and state advanced.";
                    }
                }

                return Finish(result, timer);
            }
            catch (Exception exception)
            {
                return FinishFailure(result, timer, exception.Message);
            }
        }

        public static PlacementResult PlaceOnMonitor(IntPtr windowHandle, string applicationName, string stateFilePath, int targetMonitorIndex, bool maximize, int stabilizationDelayMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            PlacementResult result = NewResult(applicationName);

            try
            {
                string validationError = ValidatePlacementArguments(windowHandle, stateFilePath, stabilizationDelayMilliseconds);
                if (validationError != null)
                {
                    return FinishFailure(result, timer, validationError);
                }

                using (StateFileStore.AcquireExclusiveLock(stateFilePath, StateLockTimeoutMilliseconds))
                {
                    List<MonitorDescriptor> monitors = NativeMethods.DiscoverAndOrderMonitors();
                    result.MonitorCount = monitors.Count;
                    if (targetMonitorIndex < 0 || targetMonitorIndex >= monitors.Count)
                    {
                        return FinishFailure(result, timer, "Target monitor index is outside the current monitor range.");
                    }

                    StateLoadResult stateResult = StateFileStore.LoadAndRepair(stateFilePath, monitors.Count);
                    if (stateResult.WasReset && stateResult.Status == StateLoadStatus.MonitorCountChanged)
                    {
                        return FinishFailure(result, timer, "Monitor count changed; maintenance placement was not attempted and state was reset.");
                    }

                    PlaceWindow(windowHandle, monitors, targetMonitorIndex, maximize, stabilizationDelayMilliseconds, result);
                    if (result.Success)
                    {
                        result.Message = "Placement verified without advancing round-robin state.";
                    }
                }

                return Finish(result, timer);
            }
            catch (Exception exception)
            {
                return FinishFailure(result, timer, exception.Message);
            }
        }

        public static PlacementResult PlaceLastUsed(IntPtr windowHandle, string applicationName, string stateFilePath, bool maximize, int stabilizationDelayMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            PlacementResult result = NewResult(applicationName);

            try
            {
                string validationError = ValidatePlacementArguments(windowHandle, stateFilePath, stabilizationDelayMilliseconds);
                if (validationError != null)
                {
                    return FinishFailure(result, timer, validationError);
                }

                using (StateFileStore.AcquireExclusiveLock(stateFilePath, StateLockTimeoutMilliseconds))
                {
                    List<MonitorDescriptor> monitors = NativeMethods.DiscoverAndOrderMonitors();
                    result.MonitorCount = monitors.Count;
                    if (monitors.Count == 0)
                    {
                        return FinishFailure(result, timer, "Windows did not report any active monitors.");
                    }

                    StateLoadResult stateResult = StateFileStore.LoadAndRepair(stateFilePath, monitors.Count);
                    if (stateResult.WasReset || stateResult.State.LastUsedIndex < 0)
                    {
                        return FinishFailure(result, timer, stateResult.Message + " No previous successful target is available.");
                    }

                    PlaceWindow(windowHandle, monitors, stateResult.State.LastUsedIndex, maximize, stabilizationDelayMilliseconds, result);
                    if (result.Success)
                    {
                        result.Message = "Last-used monitor placement verified without advancing round-robin state.";
                    }
                }

                return Finish(result, timer);
            }
            catch (Exception exception)
            {
                return FinishFailure(result, timer, exception.Message);
            }
        }

        private static void PlaceWindow(IntPtr windowHandle, IList<MonitorDescriptor> monitors, int targetIndex, bool maximize, int delayMilliseconds, PlacementResult result)
        {
            MonitorDescriptor target = monitors[targetIndex];
            result.TargetMonitorIndex = targetIndex;
            result.InitialMonitorIndex = RoundRobinLogic.FindMonitorIndex(monitors, NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MonitorDefaultToNearest));

            NativeMethods.ShowWindow(windowHandle, NativeMethods.ShowRestore);
            Stabilize(delayMilliseconds);

            bool moved = NativeMethods.SetWindowPos(
                windowHandle,
                IntPtr.Zero,
                target.Left,
                target.Top,
                target.Width,
                target.Height,
                NativeMethods.SetWindowNoZOrder | NativeMethods.SetWindowShow);

            if (!moved)
            {
                result.Win32ErrorCode = Marshal.GetLastWin32Error();
                result.Message = new Win32Exception(result.Win32ErrorCode).Message;
                return;
            }

            Stabilize(delayMilliseconds);
            if (maximize)
            {
                NativeMethods.ShowWindow(windowHandle, NativeMethods.ShowMaximize);
            }

            Stabilize(delayMilliseconds);
            IntPtr verifiedHandle = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MonitorDefaultToNearest);
            result.VerifiedMonitorIndex = RoundRobinLogic.FindMonitorIndex(monitors, verifiedHandle);
            result.Success = verifiedHandle == target.Handle;
            if (!result.Success)
            {
                result.Message = "Window placement did not verify on the requested monitor; state was not advanced.";
            }
        }

        private static string ValidatePlacementArguments(IntPtr windowHandle, string stateFilePath, int delayMilliseconds)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return "Window handle was zero.";
            }

            if (!NativeMethods.IsWindow(windowHandle))
            {
                return "Window handle did not identify a current Windows window.";
            }

            if (string.IsNullOrWhiteSpace(stateFilePath))
            {
                return "A state-file path is required.";
            }

            if (delayMilliseconds < 0)
            {
                return "Stabilization delay cannot be negative.";
            }

            return null;
        }

        private static PlacementResult NewResult(string applicationName)
        {
            PlacementResult result = new PlacementResult();
            result.ApplicationName = string.IsNullOrEmpty(applicationName) ? "Application" : applicationName;
            return result;
        }

        private static PlacementResult FinishFailure(PlacementResult result, Stopwatch timer, string message)
        {
            result.Success = false;
            result.Message = message;
            return Finish(result, timer);
        }

        private static PlacementResult Finish(PlacementResult result, Stopwatch timer)
        {
            timer.Stop();
            result.ElapsedMilliseconds = timer.ElapsedMilliseconds;
            return result;
        }

        private static void Stabilize(int milliseconds)
        {
            if (milliseconds > 0)
            {
                Thread.Sleep(milliseconds);
            }
        }
    }
}

using System;

namespace LoginVSI.MultiMonitor
{
    public sealed class MonitorDescriptor
    {
        public MonitorDescriptor(IntPtr handle, bool isPrimary, int left, int top, int right, int bottom)
        {
            Handle = handle;
            IsPrimary = isPrimary;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public IntPtr Handle { get; private set; }
        public bool IsPrimary { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }
        public int Width { get { return Right - Left; } }
        public int Height { get { return Bottom - Top; } }
    }

    public sealed class PlacementState
    {
        public PlacementState(int monitorCount, int lastUsedIndex)
        {
            MonitorCount = monitorCount;
            LastUsedIndex = lastUsedIndex;
        }

        public int MonitorCount { get; set; }
        public int LastUsedIndex { get; set; }
    }

    public enum StateLoadStatus
    {
        Valid,
        Missing,
        Invalid,
        MonitorCountChanged
    }

    public sealed class StateLoadResult
    {
        public StateLoadResult(PlacementState state, StateLoadStatus status, string message)
        {
            State = state;
            Status = status;
            Message = message;
        }

        public PlacementState State { get; private set; }
        public StateLoadStatus Status { get; private set; }
        public string Message { get; private set; }
        public bool WasReset { get { return Status != StateLoadStatus.Valid; } }
    }

    public sealed class PlacementResult
    {
        public PlacementResult()
        {
            ApplicationName = string.Empty;
            InitialMonitorIndex = -1;
            TargetMonitorIndex = -1;
            VerifiedMonitorIndex = -1;
            Message = string.Empty;
        }

        public bool Success { get; set; }
        public string ApplicationName { get; set; }
        public int MonitorCount { get; set; }
        public int InitialMonitorIndex { get; set; }
        public int TargetMonitorIndex { get; set; }
        public int VerifiedMonitorIndex { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public bool StateAdvanced { get; set; }
        public int Win32ErrorCode { get; set; }
        public string Message { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LoginVSI.MultiMonitor
{
    internal static class NativeMethods
    {
        internal const uint MonitorInfoPrimary = 0x00000001;
        internal const uint MonitorDefaultToNearest = 0x00000002;
        internal const int ShowRestore = 9;
        internal const int ShowMaximize = 3;
        internal const uint SetWindowNoZOrder = 0x0004;
        internal const uint SetWindowShow = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MonitorInfo
        {
            public int Size;
            public Rect Monitor;
            public Rect WorkArea;
            public uint Flags;
        }

        internal delegate bool MonitorEnumCallback(IntPtr monitorHandle, IntPtr deviceContext, ref Rect monitorBounds, IntPtr userData);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumDisplayMonitors(IntPtr deviceContext, IntPtr clippingRectangle, MonitorEnumCallback callback, IntPtr userData);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfo monitorInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr windowHandle, int command);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr windowHandle);

        internal static List<MonitorDescriptor> DiscoverAndOrderMonitors()
        {
            List<MonitorDescriptor> monitors = new List<MonitorDescriptor>();
            MonitorEnumCallback callback = delegate(IntPtr handle, IntPtr deviceContext, ref Rect bounds, IntPtr userData)
            {
                MonitorInfo info = new MonitorInfo();
                info.Size = Marshal.SizeOf(typeof(MonitorInfo));
                if (GetMonitorInfo(handle, ref info))
                {
                    monitors.Add(new MonitorDescriptor(
                        handle,
                        (info.Flags & MonitorInfoPrimary) != 0,
                        info.Monitor.Left,
                        info.Monitor.Top,
                        info.Monitor.Right,
                        info.Monitor.Bottom));
                }

                return true;
            };

            if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Monitor enumeration failed.");
            }

            return RoundRobinLogic.OrderPrimaryFirst(monitors);
        }
    }
}

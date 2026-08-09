using System;
using System.Collections.Generic;

namespace LoginVSI.MultiMonitor
{
    public static class RoundRobinLogic
    {
        public static int GetNextIndex(int lastUsedIndex, int monitorCount)
        {
            if (monitorCount <= 0)
            {
                throw new ArgumentOutOfRangeException("monitorCount", "Monitor count must be positive.");
            }

            if (lastUsedIndex < -1 || lastUsedIndex >= monitorCount)
            {
                throw new ArgumentOutOfRangeException("lastUsedIndex", "Last-used index is outside the valid state range.");
            }

            return (lastUsedIndex + 1) % monitorCount;
        }

        public static List<MonitorDescriptor> OrderPrimaryFirst(IEnumerable<MonitorDescriptor> monitors)
        {
            if (monitors == null)
            {
                throw new ArgumentNullException("monitors");
            }

            List<MonitorDescriptor> ordered = new List<MonitorDescriptor>(monitors);
            ordered.Sort(delegate(MonitorDescriptor first, MonitorDescriptor second)
            {
                if (first.IsPrimary && !second.IsPrimary)
                {
                    return -1;
                }

                if (!first.IsPrimary && second.IsPrimary)
                {
                    return 1;
                }

                int xComparison = first.Left.CompareTo(second.Left);
                if (xComparison != 0)
                {
                    return xComparison;
                }

                int yComparison = first.Top.CompareTo(second.Top);
                if (yComparison != 0)
                {
                    return yComparison;
                }

                int rightComparison = first.Right.CompareTo(second.Right);
                if (rightComparison != 0)
                {
                    return rightComparison;
                }

                int bottomComparison = first.Bottom.CompareTo(second.Bottom);
                if (bottomComparison != 0)
                {
                    return bottomComparison;
                }

                return first.Handle.ToInt64().CompareTo(second.Handle.ToInt64());
            });

            return ordered;
        }

        public static int FindMonitorIndex(IList<MonitorDescriptor> monitors, IntPtr handle)
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
    }
}

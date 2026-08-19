using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace LoginVSI.MultiMonitor
{
    public static class StateFileStore
    {
        public static StateLoadResult Load(string stateFilePath, int currentMonitorCount)
        {
            ValidateInputs(stateFilePath, currentMonitorCount);

            if (!File.Exists(stateFilePath))
            {
                return NewResetResult(currentMonitorCount, StateLoadStatus.Missing, "State file was missing; round-robin state was reset.");
            }

            // Parsing failures are represented by StateLoadStatus.Invalid. File-system
            // failures are operational errors and must remain distinguishable instead
            // of being mislabeled as corrupt state and silently overwritten.
            return Parse(File.ReadAllLines(stateFilePath), currentMonitorCount);
        }

        public static StateLoadResult LoadAndRepair(string stateFilePath, int currentMonitorCount)
        {
            StateLoadResult result = Load(stateFilePath, currentMonitorCount);
            if (result.WasReset)
            {
                WriteAtomic(stateFilePath, result.State);
            }

            return result;
        }

        public static StateLoadResult Parse(IEnumerable<string> lines, int currentMonitorCount)
        {
            if (lines == null)
            {
                throw new ArgumentNullException("lines");
            }

            if (currentMonitorCount <= 0)
            {
                throw new ArgumentOutOfRangeException("currentMonitorCount");
            }

            int parsedMonitorCount = 0;
            int parsedLastUsedIndex = -1;
            bool foundMonitorCount = false;
            bool foundLastUsedIndex = false;

            foreach (string untrimmedLine in lines)
            {
                string line = untrimmedLine == null ? string.Empty : untrimmedLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("MonitorCount=", StringComparison.Ordinal))
                {
                    if (foundMonitorCount || !int.TryParse(line.Substring("MonitorCount=".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedMonitorCount))
                    {
                        return NewResetResult(currentMonitorCount, StateLoadStatus.Invalid, "State file had an invalid MonitorCount value; round-robin state was reset.");
                    }

                    foundMonitorCount = true;
                }
                else if (line.StartsWith("LastUsedIndex=", StringComparison.Ordinal))
                {
                    if (foundLastUsedIndex || !int.TryParse(line.Substring("LastUsedIndex=".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedLastUsedIndex))
                    {
                        return NewResetResult(currentMonitorCount, StateLoadStatus.Invalid, "State file had an invalid LastUsedIndex value; round-robin state was reset.");
                    }

                    foundLastUsedIndex = true;
                }
                else
                {
                    return NewResetResult(currentMonitorCount, StateLoadStatus.Invalid, "State file contained an unknown entry; round-robin state was reset.");
                }
            }

            if (!foundMonitorCount || !foundLastUsedIndex || parsedMonitorCount <= 0 || parsedLastUsedIndex < -1 || parsedLastUsedIndex >= parsedMonitorCount)
            {
                return NewResetResult(currentMonitorCount, StateLoadStatus.Invalid, "State file was incomplete or outside the valid range; round-robin state was reset.");
            }

            if (parsedMonitorCount != currentMonitorCount)
            {
                return NewResetResult(currentMonitorCount, StateLoadStatus.MonitorCountChanged, "Monitor count changed; round-robin state was reset.");
            }

            return new StateLoadResult(new PlacementState(parsedMonitorCount, parsedLastUsedIndex), StateLoadStatus.Valid, "State file was valid.");
        }

        public static string[] Serialize(PlacementState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            return new string[]
            {
                "MonitorCount=" + state.MonitorCount.ToString(CultureInfo.InvariantCulture),
                "LastUsedIndex=" + state.LastUsedIndex.ToString(CultureInfo.InvariantCulture)
            };
        }

        public static void WriteAtomic(string stateFilePath, PlacementState state)
        {
            if (string.IsNullOrWhiteSpace(stateFilePath))
            {
                throw new ArgumentException("A state-file path is required.", "stateFilePath");
            }

            string fullPath = Path.GetFullPath(stateFilePath);
            string directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The state-file path must include a directory.", "stateFilePath");
            }

            Directory.CreateDirectory(directory);
            string temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                File.WriteAllLines(temporaryPath, Serialize(state), new UTF8Encoding(false));
                if (File.Exists(fullPath))
                {
                    File.Replace(temporaryPath, fullPath, null);
                }
                else
                {
                    File.Move(temporaryPath, fullPath);
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

        internal static IDisposable AcquireExclusiveLock(string stateFilePath, int timeoutMilliseconds)
        {
            if (string.IsNullOrWhiteSpace(stateFilePath))
            {
                throw new ArgumentException("A state-file path is required.", "stateFilePath");
            }

            if (timeoutMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds");
            }

            string fullPath = Path.GetFullPath(stateFilePath);
            string directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);
            string lockPath = fullPath + ".lock";
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);

            while (true)
            {
                try
                {
                    return new StateLock(lockPath, new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None));
                }
                catch (IOException)
                {
                    if (DateTime.UtcNow >= deadline)
                    {
                        throw new TimeoutException("Timed out waiting for exclusive access to the multi-monitor state file.");
                    }

                    Thread.Sleep(50);
                }
            }
        }

        private static StateLoadResult NewResetResult(int currentMonitorCount, StateLoadStatus status, string message)
        {
            return new StateLoadResult(new PlacementState(currentMonitorCount, -1), status, message);
        }

        private static void ValidateInputs(string stateFilePath, int currentMonitorCount)
        {
            if (string.IsNullOrWhiteSpace(stateFilePath))
            {
                throw new ArgumentException("A state-file path is required.", "stateFilePath");
            }

            if (currentMonitorCount <= 0)
            {
                throw new ArgumentOutOfRangeException("currentMonitorCount");
            }
        }

        private sealed class StateLock : IDisposable
        {
            private readonly string _path;
            private FileStream _stream;

            public StateLock(string path, FileStream stream)
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
                    // The zero-byte lock marker is harmless if cleanup is delayed.
                }
            }
        }
    }
}

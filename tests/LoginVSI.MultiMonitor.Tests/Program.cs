using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using LoginVSI.MultiMonitor;

internal static class Program
{
    private static int _passed;

    private static int Main()
    {
        try
        {
            Run("Next index - one display", delegate { AssertSequence(1, new int[] { 0, 0, 0, 0 }); });
            Run("Next index - two displays", delegate { AssertSequence(2, new int[] { 0, 1, 0, 1 }); });
            Run("Next index - three displays", delegate { AssertSequence(3, new int[] { 0, 1, 2, 0 }); });
            Run("Next index - four displays", delegate { AssertSequence(4, new int[] { 0, 1, 2, 3 }); });
            Run("Next index rejects non-positive monitor count", TestNonPositiveMonitorCount);
            Run("Next index rejects invalid last-used index", TestInvalidLastUsedIndex);
            Run("Next index wraps at upper boundary", delegate { Equal(0, RoundRobinLogic.GetNextIndex(3, 4), "Upper-boundary state did not wrap."); });
            Run("Primary-first synthetic ordering", TestPrimaryFirstOrdering);
            Run("Negative-coordinate monitor data", TestNegativeCoordinates);
            Run("Valid state parsing", TestValidState);
            Run("Invalid state recovery", TestInvalidState);
            Run("Duplicate state keys are rejected", TestDuplicateStateKeys);
            Run("Out-of-range state is rejected", TestOutOfRangeState);
            Run("Missing state handling", TestMissingState);
            Run("Monitor-count-change reset", TestMonitorCountChange);
            Run("Missing state repair", TestMissingStateRepair);
            Run("Invalid state repair", TestInvalidStateRepair);
            Run("Monitor-count-change repair", TestMonitorCountChangeRepair);
            Run("Maintenance repair exposes no previous allocation", TestMaintenanceRepairHasNoPreviousAllocation);
            Run("State serialization", TestSerialization);
            Run("State round trip", TestRoundTrip);
            Run("Atomic replacement write", TestAtomicReplacement);
            Run("State read IO failures remain operational errors", TestStateReadIoFailure);
            Run("State lock serializes contenders", TestStateLockSerialization);
            Run("State lock times out", TestStateLockTimeout);
            Run("Invalid HWND result", TestInvalidWindowHandle);
            Run("Canonical Open/Place workload contract", TestCanonicalOpenPlaceWorkload);
            Run("Canonical Close workload is state-neutral", TestCanonicalCloseWorkload);
            Run("Workload source API casing and harness disposition", TestWorkloadSourceContracts);
            Run("Reusable DLL assembly contract", TestReusableDllAssemblyContract);

            Console.WriteLine("PASS: " + _passed + " tests completed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL: " + exception.Message);
            return 1;
        }
    }

    private static void Run(string name, Action test)
    {
        test();
        _passed++;
        Console.WriteLine("PASS: " + name);
    }

    private static void AssertSequence(int monitorCount, int[] expected)
    {
        int lastUsedIndex = -1;
        for (int index = 0; index < expected.Length; index++)
        {
            int actual = RoundRobinLogic.GetNextIndex(lastUsedIndex, monitorCount);
            Equal(expected[index], actual, "Unexpected round-robin index at position " + index + ".");
            lastUsedIndex = actual;
        }
    }

    private static void TestNonPositiveMonitorCount()
    {
        Throws<ArgumentOutOfRangeException>(delegate { RoundRobinLogic.GetNextIndex(-1, 0); }, "Zero monitor count was accepted.");
        Throws<ArgumentOutOfRangeException>(delegate { StateFileStore.Parse(new string[0], 0); }, "State parser accepted zero monitors.");
    }

    private static void TestInvalidLastUsedIndex()
    {
        Throws<ArgumentOutOfRangeException>(delegate { RoundRobinLogic.GetNextIndex(-2, 2); }, "Index below -1 was accepted.");
        Throws<ArgumentOutOfRangeException>(delegate { RoundRobinLogic.GetNextIndex(2, 2); }, "Index equal to monitor count was accepted.");
    }

    private static void TestPrimaryFirstOrdering()
    {
        List<MonitorDescriptor> monitors = new List<MonitorDescriptor>
        {
            new MonitorDescriptor(new IntPtr(1), false, -1920, 0, 0, 1080),
            new MonitorDescriptor(new IntPtr(2), false, 1920, 0, 3840, 1080),
            new MonitorDescriptor(new IntPtr(3), true, 0, 0, 1920, 1080)
        };

        List<MonitorDescriptor> ordered = RoundRobinLogic.OrderPrimaryFirst(monitors);
        Equal(new IntPtr(3), ordered[0].Handle, "Primary monitor was not first.");
        Equal(new IntPtr(1), ordered[1].Handle, "Secondary monitors were not ordered by X coordinate.");
        Equal(new IntPtr(2), ordered[2].Handle, "Secondary monitors were not ordered by X coordinate.");
    }

    private static void TestNegativeCoordinates()
    {
        MonitorDescriptor negative = new MonitorDescriptor(new IntPtr(1), false, -2560, -200, 0, 1240);
        Equal(-2560, negative.Left, "Negative X coordinate was not preserved.");
        Equal(-200, negative.Top, "Negative Y coordinate was not preserved.");
        Equal(2560, negative.Width, "Width calculation failed for a negative-coordinate monitor.");
        Equal(1440, negative.Height, "Height calculation failed for a negative-coordinate monitor.");
    }

    private static void TestValidState()
    {
        StateLoadResult result = StateFileStore.Parse(new string[] { "MonitorCount=3", "LastUsedIndex=1" }, 3);
        Equal(StateLoadStatus.Valid, result.Status, "Valid state was rejected.");
        Equal(1, result.State.LastUsedIndex, "Last-used index was not parsed.");
    }

    private static void TestInvalidState()
    {
        StateLoadResult result = StateFileStore.Parse(new string[] { "MonitorCount=three", "LastUsedIndex=1" }, 3);
        Equal(StateLoadStatus.Invalid, result.Status, "Invalid state was not detected.");
        Equal(-1, result.State.LastUsedIndex, "Invalid state did not reset the index.");
    }

    private static void TestDuplicateStateKeys()
    {
        StateLoadResult duplicateMonitor = StateFileStore.Parse(new string[] { "MonitorCount=2", "MonitorCount=2", "LastUsedIndex=0" }, 2);
        StateLoadResult duplicateIndex = StateFileStore.Parse(new string[] { "MonitorCount=2", "LastUsedIndex=0", "LastUsedIndex=1" }, 2);
        Equal(StateLoadStatus.Invalid, duplicateMonitor.Status, "Duplicate MonitorCount was accepted.");
        Equal(StateLoadStatus.Invalid, duplicateIndex.Status, "Duplicate LastUsedIndex was accepted.");
    }

    private static void TestOutOfRangeState()
    {
        Equal(StateLoadStatus.Invalid, StateFileStore.Parse(new string[] { "MonitorCount=2", "LastUsedIndex=-2" }, 2).Status, "Index below -1 was accepted by the state parser.");
        Equal(StateLoadStatus.Invalid, StateFileStore.Parse(new string[] { "MonitorCount=2", "LastUsedIndex=2" }, 2).Status, "Out-of-range index was accepted by the state parser.");
        Equal(StateLoadStatus.Invalid, StateFileStore.Parse(new string[] { "MonitorCount=0", "LastUsedIndex=-1" }, 2).Status, "Non-positive stored monitor count was accepted.");
    }

    private static void TestMissingState()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "missing.txt");
            StateLoadResult result = StateFileStore.Load(path, 2);
            Equal(StateLoadStatus.Missing, result.Status, "Missing state was not reported.");
            Equal(-1, result.State.LastUsedIndex, "Missing state did not return the initial index.");
        });
    }

    private static void TestMonitorCountChange()
    {
        StateLoadResult result = StateFileStore.Parse(new string[] { "MonitorCount=2", "LastUsedIndex=1" }, 3);
        Equal(StateLoadStatus.MonitorCountChanged, result.Status, "Monitor-count change was not detected.");
        Equal(3, result.State.MonitorCount, "Reset state did not use the current monitor count.");
        Equal(-1, result.State.LastUsedIndex, "Monitor-count reset did not use the initial index.");
    }

    private static void TestMissingStateRepair()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            StateLoadResult result = StateFileStore.LoadAndRepair(path, 2);
            Equal(StateLoadStatus.Missing, result.Status, "Missing state was not reported during repair.");
            Equal(StateLoadStatus.Valid, StateFileStore.Load(path, 2).Status, "Missing state was not repaired on disk.");
        });
    }

    private static void TestInvalidStateRepair()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            File.WriteAllLines(path, new string[] { "MonitorCount=invalid", "LastUsedIndex=1" });
            StateLoadResult result = StateFileStore.LoadAndRepair(path, 2);
            Equal(StateLoadStatus.Invalid, result.Status, "Invalid state was not reported during repair.");
            StateLoadResult repaired = StateFileStore.Load(path, 2);
            Equal(StateLoadStatus.Valid, repaired.Status, "Invalid state was not repaired on disk.");
            Equal(-1, repaired.State.LastUsedIndex, "Invalid state repair did not persist the initial index.");
        });
    }

    private static void TestMonitorCountChangeRepair()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            StateFileStore.WriteAtomic(path, new PlacementState(2, 1));
            StateLoadResult result = StateFileStore.LoadAndRepair(path, 3);
            Equal(StateLoadStatus.MonitorCountChanged, result.Status, "Count change was not reported during repair.");
            StateLoadResult repaired = StateFileStore.Load(path, 3);
            Equal(StateLoadStatus.Valid, repaired.Status, "Count-changed state was not repaired on disk.");
            Equal(-1, repaired.State.LastUsedIndex, "Count-change repair did not persist the initial index.");
        });
    }

    private static void TestMaintenanceRepairHasNoPreviousAllocation()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string missingPath = Path.Combine(directory, "missing-state.txt");
            StateLoadResult missing = StateFileStore.LoadAndRepair(missingPath, 2);
            Equal(-1, missing.State.LastUsedIndex, "Missing-state repair invented a maintenance target.");

            string corruptPath = Path.Combine(directory, "corrupt-state.txt");
            File.WriteAllLines(corruptPath, new string[] { "MonitorCount=2", "LastUsedIndex=99" });
            StateLoadResult corrupt = StateFileStore.LoadAndRepair(corruptPath, 2);
            Equal(-1, corrupt.State.LastUsedIndex, "Corrupt-state repair advanced or invented a maintenance target.");
        });
    }

    private static void TestSerialization()
    {
        string[] lines = StateFileStore.Serialize(new PlacementState(4, 2));
        Equal(2, lines.Length, "State schema line count changed.");
        Equal("MonitorCount=4", lines[0], "MonitorCount serialization changed.");
        Equal("LastUsedIndex=2", lines[1], "LastUsedIndex serialization changed.");
    }

    private static void TestRoundTrip()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            StateFileStore.WriteAtomic(path, new PlacementState(3, 2));
            StateLoadResult result = StateFileStore.Load(path, 3);
            Equal(StateLoadStatus.Valid, result.Status, "Written state did not parse as valid.");
            Equal(2, result.State.LastUsedIndex, "State did not round-trip.");
        });
    }

    private static void TestAtomicReplacement()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            StateFileStore.WriteAtomic(path, new PlacementState(2, 0));
            StateFileStore.WriteAtomic(path, new PlacementState(2, 1));
            StateLoadResult result = StateFileStore.Load(path, 2);
            Equal(1, result.State.LastUsedIndex, "Replacement write did not publish the new state.");
            Equal(0, Directory.GetFiles(directory, "*.tmp").Length, "Temporary state file was left behind.");
        });
    }

    private static void TestStateReadIoFailure()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            File.WriteAllLines(path, new string[] { "MonitorCount=2", "LastUsedIndex=0" });
            using (FileStream held = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Throws<IOException>(delegate { StateFileStore.Load(path, 2); }, "An unreadable state file was mislabeled as corrupt state.");
            }
        });
    }

    private static void TestStateLockSerialization()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            IDisposable first = StateFileStore.AcquireExclusiveLock(path, 1000);
            bool secondAcquired = false;
            Task contender = Task.Run(delegate
            {
                using (StateFileStore.AcquireExclusiveLock(path, 2000))
                {
                    secondAcquired = true;
                }
            });

            Thread.Sleep(150);
            Equal(false, secondAcquired, "Contending state access bypassed the exclusive lock.");
            first.Dispose();
            Equal(true, contender.Wait(2000), "Contending state access did not resume after lock release.");
            Equal(true, secondAcquired, "Contender did not acquire the released state lock.");
        });
    }

    private static void TestStateLockTimeout()
    {
        WithTemporaryDirectory(delegate(string directory)
        {
            string path = Path.Combine(directory, "state.txt");
            using (StateFileStore.AcquireExclusiveLock(path, 1000))
            {
                Throws<TimeoutException>(delegate
                {
                    using (StateFileStore.AcquireExclusiveLock(path, 100)) { }
                }, "Lock contention did not honor the timeout.");
            }
        });
    }

    private static void TestInvalidWindowHandle()
    {
        PlacementResult result = MultiMonitorPlacer.PlaceNext(IntPtr.Zero, "Invalid window test", "unused-state-path", true, 0);
        Equal(false, result.Success, "Zero HWND unexpectedly succeeded.");
        Equal(false, result.StateAdvanced, "Failed placement advanced state.");
        Equal(-1, result.TargetMonitorIndex, "Failed placement selected a target.");
    }

    private static void TestCanonicalOpenPlaceWorkload()
    {
        string source = ReadRepositoryFile("workloads", "dll-backed", "01-Open-Place-Applications.cs");
        Contains(source, "LoginVSI.MultiMonitor.dll", "Open/Place does not reference the staged Preview DLL.");
        Contains(source, "state.txt", "Open/Place does not reference the round-robin state path.");
        Contains(source, "Assembly.LoadFrom", "Open/Place does not load the staged Preview DLL.");
        Contains(source, "PlaceNext", "Open/Place does not allocate through the Preview DLL.");
        Equal(3, CountOccurrences(source, "placement.PlaceNext("), "Open/Place must call the allocating API once per durable demonstration window.");
        Contains(source, "ResetStateForFreshPreviewRun", "Fresh-run reset intent is not explicit.");
        Contains(source, "className:", "Open/Place does not use compiler-proven className casing.");
        Contains(source, "processName:", "Open/Place does not use compiler-proven processName casing.");
    }

    private static void TestCanonicalCloseWorkload()
    {
        string source = ReadRepositoryFile("workloads", "dll-backed", "02-Close-Applications.cs");
        Contains(source, "FindWindows", "Close does not resolve base windows.");
        Contains(source, ".Close()", "Close does not request bounded window cleanup.");
        DoesNotContain(source, "PlaceNext", "Close must not allocate a monitor destination.");
        DoesNotContain(source, "ResetState", "Close must not reset round-robin state.");
        DoesNotContain(source, "state.txt", "Close must not read or write round-robin state.");
        DoesNotContain(source, "NativeWindowHandle", "Close must not persist or use native handles.");
        DoesNotContain(source, "LoginVSI.MultiMonitor.dll", "Close must not load the placement DLL.");
    }

    private static void TestWorkloadSourceContracts()
    {
        string repositoryRoot = FindRepositoryRoot();
        string regressionDirectory = Path.Combine(repositoryRoot, "workloads", "dll-backed", "regression");
        Equal(true, File.Exists(Path.Combine(regressionDirectory, "01-Initialize-Notepad-Paint.cs")), "Proven phase-one harness was not retained under regression.");
        Equal(true, File.Exists(Path.Combine(regressionDirectory, "02-Continue-Edge.cs")), "Proven phase-two harness was not retained under regression.");
        Equal(false, File.Exists(Path.Combine(repositoryRoot, "workloads", "dll-backed", "01-Initialize-Notepad-Paint.cs")), "Old harness remains ambiguous with the canonical flow.");

        string[] workloadFiles = Directory.GetFiles(Path.Combine(repositoryRoot, "workloads"), "*.cs", SearchOption.AllDirectories);
        foreach (string workloadFile in workloadFiles)
        {
            string source = File.ReadAllText(workloadFile);
            DoesNotContain(source, "classname:", "Lowercase classname named argument reappeared in " + workloadFile + ".");
            DoesNotContain(source, "processname:", "Lowercase processname named argument reappeared in " + workloadFile + ".");
        }
    }

    private static void TestReusableDllAssemblyContract()
    {
        Assembly assembly = typeof(MultiMonitorPlacer).Assembly;
        TargetFrameworkAttribute target = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
        Equal(true, target != null, "Reusable DLL has no target-framework metadata.");
        Equal(".NETStandard,Version=v2.0", target.FrameworkName, "Reusable DLL target framework changed.");

        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
        {
            Equal(false, string.Equals(reference.Name, "LoginPI.Engine", StringComparison.OrdinalIgnoreCase), "Reusable DLL references LoginPI.Engine.");
        }
    }

    private static string ReadRepositoryFile(params string[] relativePathParts)
    {
        string path = FindRepositoryRoot();
        foreach (string part in relativePathParts)
        {
            path = Path.Combine(path, part);
        }

        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "workloads")) &&
                Directory.Exists(Path.Combine(directory.FullName, "reference")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root from " + AppContext.BaseDirectory + ".");
    }

    private static void Contains(string text, string expected, string message)
    {
        if (text.IndexOf(expected, StringComparison.Ordinal) < 0)
        {
            throw new InvalidOperationException(message + " Missing=" + expected + ".");
        }
    }

    private static void DoesNotContain(string text, string unexpected, string message)
    {
        if (text.IndexOf(unexpected, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(message + " Unexpected=" + unexpected + ".");
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private static void WithTemporaryDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "LoginVSI.MultiMonitor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            action(directory);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(message + " Expected=" + expected + ", Actual=" + actual + ".");
        }
    }

    private static void Throws<TException>(Action action, string message) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
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
            Run("Primary-first synthetic ordering", TestPrimaryFirstOrdering);
            Run("Negative-coordinate monitor data", TestNegativeCoordinates);
            Run("Valid state parsing", TestValidState);
            Run("Invalid state recovery", TestInvalidState);
            Run("Missing state handling", TestMissingState);
            Run("Monitor-count-change reset", TestMonitorCountChange);
            Run("Missing state repair", TestMissingStateRepair);
            Run("Invalid state repair", TestInvalidStateRepair);
            Run("Monitor-count-change repair", TestMonitorCountChangeRepair);
            Run("State serialization", TestSerialization);
            Run("State round trip", TestRoundTrip);
            Run("Atomic replacement write", TestAtomicReplacement);
            Run("Invalid HWND result", TestInvalidWindowHandle);

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

    private static void TestInvalidWindowHandle()
    {
        PlacementResult result = MultiMonitorPlacer.PlaceNext(IntPtr.Zero, "Invalid window test", "unused-state-path", true, 0);
        Equal(false, result.Success, "Zero HWND unexpectedly succeeded.");
        Equal(false, result.StateAdvanced, "Failed placement advanced state.");
        Equal(-1, result.TargetMonitorIndex, "Failed placement selected a target.");
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
}

// TARGET:none
// START_IN:

using LoginPI.Engine.ScriptBase;
using System;
using System.IO;
using System.Reflection;

public class OfficePreviewResetPlacementState : ScriptBase
{
    private void Execute()
    {
        string previewDirectory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string assemblyPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        string statePath = Path.Combine(previewDirectory, "state.txt");
        if (!FileExists(assemblyPath))
        {
            ABORT("Multi-monitor Preview DLL is missing. Run 00-Prepare-MultiMonitor first.");
            return;
        }

        Type placer = Assembly.LoadFrom(assemblyPath).GetType("LoginVSI.MultiMonitor.MultiMonitorPlacer", true);
        object result = placer.GetMethod("ResetState", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { statePath });
        Log(OfficePreviewResult.Format(result));
        if (!(bool)OfficePreviewResult.Get(result, "Success"))
        {
            ABORT("Office Preview state reset failed: " + OfficePreviewResult.Get(result, "Message"));
        }
    }
}

internal static class OfficePreviewResult
{
    internal static object Get(object result, string name)
    {
        return result.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance).GetValue(result, null);
    }

    internal static string Format(object result)
    {
        return Get(result, "ApplicationName") + ": Success=" + Get(result, "Success") +
            ", MonitorCount=" + Get(result, "MonitorCount") + ", Target=" + Get(result, "TargetMonitorIndex") +
            ", Verified=" + Get(result, "VerifiedMonitorIndex") + ", StateAdvanced=" + Get(result, "StateAdvanced") +
            ", ElapsedMs=" + Get(result, "ElapsedMilliseconds") + ", Message=" + Get(result, "Message");
    }
}

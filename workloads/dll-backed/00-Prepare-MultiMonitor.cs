// TARGET:none
// START_IN:

/////////////
// Multi-Monitor Preview DLL preparation
// Runtime-proven in Login Enterprise 6.8.6 Script Editor/Standalone Engine
/////////////

using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Constants;
using System.IO;

public class PrepareMultiMonitorPreview : ScriptBase
{
    // Leave false for normal runs. Set true only when deliberately replacing a
    // previously staged target-local DLL with the appliance ScriptContent copy.
    private bool ForceRefreshMultiMonitorDll = false;

    private void Execute()
    {
        string previewDirectory = Path.Combine(GetEnvironmentVariable("TEMP"), "LoginPI", "MultiMonitor");
        string destinationPath = Path.Combine(previewDirectory, "LoginVSI.MultiMonitor.dll");
        string sourcePath = UrnBaseForFiles.UrnBase + "LoginVSI.MultiMonitor.dll";

        if (!Directory.Exists(previewDirectory))
        {
            Directory.CreateDirectory(previewDirectory);
            Log("Created multi-monitor Preview staging directory: " + previewDirectory);
        }

        bool localDllExists = FileExists(destinationPath);
        if (localDllExists && !ForceRefreshMultiMonitorDll)
        {
            Log("Retaining existing staged multi-monitor Preview DLL: " + destinationPath);
            return;
        }

        if (localDllExists)
        {
            Log("Force refresh enabled; removing existing staged multi-monitor Preview DLL: " + destinationPath);
            RemoveFile(path: destinationPath);
            if (FileExists(destinationPath))
            {
                ABORT("Multi-monitor Preview DLL refresh could not remove the existing staged DLL: " + destinationPath);
                return;
            }
        }

        Log("Staging multi-monitor Preview DLL from ScriptContent to: " + destinationPath);
        CopyFile(sourcePath: sourcePath, destinationPath: destinationPath);

        if (!FileExists(destinationPath))
        {
            ABORT("Multi-monitor Preview DLL staging did not create the expected target-local file: " + destinationPath);
            return;
        }

        if (localDllExists)
        {
            Log("Forced refresh completed for staged multi-monitor Preview DLL: " + destinationPath);
        }
        else
        {
            Log("Initial staging completed for multi-monitor Preview DLL: " + destinationPath);
        }
    }
}

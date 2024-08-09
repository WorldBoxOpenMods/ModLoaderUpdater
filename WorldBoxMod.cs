using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

public class WorldBoxMod : MonoBehaviour
{
    public static Version CurrentVersion { get; private set; }

    public void Awake()
    {
        Debug.Log($"Begin to check update for NML. Current version: {CurrentVersion}");
        var path1 = Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader.dll");
        var path2 = Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader_memload.dll");
        var both_existed = File.Exists(path1) && File.Exists(path2);
        if (both_existed)
        {
            try
            {
                File.Delete(Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader_memload.dll"));
            }
            catch (Exception e)
            {
                return;
            }
        }

        if (File.Exists(path2))
            Paths.NMLPath = path2;
        else
            Paths.NMLPath = path1;

        UpdateVersion();
/*
        if (new WorkshopUpdater().Update())
        {
            UpdateVersion();
            Debug.Log($"Updated to latest version: {CurrentVersion} from Workshop");
            return;
        }
*/
        if (new GithubUpdater().Update())
        {
            UpdateVersion();
            Debug.Log($"Updated to latest version: {CurrentVersion} from Github");
            return;
        }

        if (new GiteeUpdater().Update())
        {
            UpdateVersion();
            Debug.Log($"Updated to latest version: {CurrentVersion} from Gitee");
            return;
        }

        Debug.Log($"No update available. Current version: {CurrentVersion}");
    }

    internal void UpdateVersion()
    {
        if (File.Exists(Paths.NMLPath))
            CurrentVersion = AssemblyName.GetAssemblyName(Paths.NMLPath).Version;
        else
            CurrentVersion = new Version(0, 0, 0, 0);
    }
}
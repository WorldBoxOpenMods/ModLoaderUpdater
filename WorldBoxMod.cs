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
        Debug.Log("Begin to check update for NML");
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

        CurrentVersion = AssemblyName.GetAssemblyName(Paths.NMLPath).Version;

        if (new WorkshopUpdater().Update())
        {
            UpdateVersion();
            return;
        }

        if (new GithubUpdater().Update())
        {
            UpdateVersion();
            return;
        }

        if (new GiteeUpdater().Update())
        {
            UpdateVersion();
        }
    }

    internal void UpdateVersion()
    {
        CurrentVersion = AssemblyName.GetAssemblyName(Paths.NMLPath).Version;
    }
}
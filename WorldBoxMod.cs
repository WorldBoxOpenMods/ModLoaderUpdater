using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

public class WorldBoxMod : MonoBehaviour
{
    public static WorldBoxMod I              { get; private set; }
    public static Version     CurrentVersion { get; private set; }

    public async void Awake()
    {
        I = this;
        Debug.Log($"Begin to check update for NML. Current version: {CurrentVersion}");
        var path1 = Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader.dll");
        var path2 = Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader_memload.dll");
        var both_existed = File.Exists(path1) && File.Exists(path2);
        if (both_existed)
        {
            try
            {
                File.Delete(path2);
            }
            catch (Exception e)
            {
                File.Delete(path1);
            }
        }

        if (File.Exists(path2))
            Paths.NMLPath = path2;
        else
            Paths.NMLPath = path1;

        UpdateVersion();

        var updaters = new List<AUpdater>
        {
            new WorkshopUpdater(),
            new GithubUpdater(), new GiteeUpdater()
        };
        foreach (AUpdater updater in updaters)
        {
            var res = await updater.Update();
            if (res)
            {
                UpdateVersion();
                Debug.Log($"Updated to latest version: {CurrentVersion} from {updater.GetType().Name}");
                if (!ModLoader.getModsLoaded().Contains("NeoModLoader")) UpdateHelper.LoadNMLManually();

                return;
            }
        }

        if (!ModLoader.getModsLoaded().Contains("NeoModLoader")) UpdateHelper.LoadNMLManually();

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
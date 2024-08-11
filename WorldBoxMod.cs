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
        var any_existed = File.Exists(path1) || File.Exists(path2);
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
            //new WorkshopUpdater(),
            //new GithubUpdater(),
            new GiteeUpdater()
        };
        var async = false;
        foreach (AUpdater updater in updaters)
        {
            var awaiter_res = updater.Update();
            var no_async = awaiter_res.IsCompleted;
            async |= !no_async;
            /* Several situations:
             * 1. Mod loaded: NeoModLoader->AutoUpdate, no need to load NML manually
             * 2. Mod loaded: AutoUpdate->NeoModLoader, use cached download file: old file is deleted and restored in a single frame(no async). no need to load NML manually
             * 3. Mod loaded: AutoUpdate->NeoModLoader, download file: old file is deleted, start downloading new file, replace successfully(async). NML is loaded already.
             * 4. Mod loaded: AutoUpdate->NeoModLoader, download file: old file is deleted, start downloading new file, replace failed and restore old file(async). NML is not existed so that it should be loaded manually.
             * 5. Mod loaded: AutoUpdate->NeoModLoader, NML is not existed so that it should be loaded manually.
             */
            if (await awaiter_res)
            {
                UpdateVersion();
                Debug.Log($"Updated to latest version: {CurrentVersion} from {updater.GetType().Name}");
                if ((!no_async && !ModLoader.getModsLoaded().Contains("NeoModLoader")) || !any_existed)
                    UpdateHelper.LoadNMLManually();

                return;
            }
        }

        if (async && !ModLoader.getModsLoaded().Contains("NeoModLoader")) UpdateHelper.LoadNMLManually();

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
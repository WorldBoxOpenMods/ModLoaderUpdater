using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

public enum UpdateResult
{
    IsTaken,
    Success,
    NoNeedUpdate,
    Fail
}

public static class UpdateHelper
{
    private static readonly Dictionary<string, byte[]> file_bak = new();

    public static bool BackupFile(string file_path)
    {
        if (!File.Exists(file_path)) return true;
        file_bak[file_path] = File.ReadAllBytes(file_path);
        try
        {
            File.Delete(file_path);
            return true;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }

    public static void RestoreFile(string file_path)
    {
        File.WriteAllBytes(file_path, file_bak[file_path]);
        file_bak.Remove(file_path);
    }

    public static void LoadNMLManually()
    {
        var nml_bytes = File.ReadAllBytes(Paths.NMLPath);
        Assembly assembly;

        if (File.Exists(Paths.NMLPdbPath))
            assembly = Assembly.Load(nml_bytes, File.ReadAllBytes(Paths.NMLPdbPath));
        else
            assembly = Assembly.Load(nml_bytes);

        Type type = assembly.GetType("NeoModLoader.WorldBoxMod");
        new GameObject("NeoModLoader")
        {
            transform =
            {
                parent = WorldBoxMod.I.transform.parent
            }
        }.AddComponent(type);
        ModLoader.modsLoaded.Add("NeoModLoader");
        Debug.Log("[NeoModLoader] Was added manually");
    }

    public static UpdateResult TryReplaceFile(string pOldFile, string pNewFile)
    {
        FileInfo old_file = new(pOldFile);
        FileInfo new_file = new(pNewFile);
        if (old_file.Exists && old_file.LastWriteTime >= new_file.LastWriteTime) return UpdateResult.NoNeedUpdate;
        try
        {
            if (old_file.Exists)
                old_file.Delete();
        }
        catch (Exception e)
        {
            return UpdateResult.IsTaken;
        }

        File.Copy(pNewFile, pOldFile, true);
        return UpdateResult.Success;
    }

    public static bool CheckValid(string file_path)
    {
        try
        {
            AssemblyName.GetAssemblyName(file_path);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task<string> DownloadFile(string download_url, string postfix, string filename)
    {
        var components = filename.Split('.');
        var download_path = Path.Combine(Path.GetTempPath(), $"{components[0]}_{postfix}.{components[1]}");
        var download_complete_path =
            Path.Combine(Path.GetTempPath(), $"{components[0]}_{postfix}_completed.{components[1]}");
        if (!File.Exists(download_complete_path))
        {
            using var client = new WebClient();
            try
            {
                await HttpUtils.DownloadFile(download_url, download_path);
            }
            catch (Exception)
            {
                return "";
            }

            File.Move(download_path, download_complete_path);
        }

        return download_complete_path;
    }
}
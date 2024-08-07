using System;
using System.IO;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;
public class WorldBoxMod : MonoBehaviour
{
    private bool _nml_is_taken;
    private bool _nml_is_updated;
    public void Awake()
    {
        Debug.Log("Begin to check update for NML");
        bool both_existed = File.Exists(Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader.dll")) &&
                            File.Exists(Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader_memload.dll"));
        if (both_existed)
        {
            try
            {
                File.Delete(Path.Combine(Application.streamingAssetsPath, "Mods", "NeoModLoader.dll"));
            }
            catch (Exception e)
            {
                return;
            }
        }
        if (TryUpdateNMLFromWorkshop()) return;
        TryUpdateNMLFromGithub();
    }

    class ReleaseInfo
    {
        public ReleaseAsset[] assets;
    }

    class ReleaseAsset
    {
        public string name;
        public string browser_download_url;
    }
    class RefInfo
    {
        public string sha;
    }

    private string GetDownloadMirrorUrl(string url)
    {
        return url.Replace("github.com", "github.ink");
    }

    private bool CheckValid(string file_path)
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

    private bool TryUpdateNMLFromGithub()
    {
        if (_nml_is_taken) return false;
        
        string release_url = $"https://api.github.com/repos/WorldBoxOpenMods/ModLoader/releases/latest";
        string commit_url =
            $"https://api.github.com/repos/WorldBoxOpenMods/ModLoader/commits/latest";
        
        string nml_commit = File.Exists(Paths.NMLCommitPath) ? File.ReadAllText(Paths.NMLCommitPath) : "";
        
        string ref_info_str = HttpUtils.Request(commit_url);
        
        RefInfo ref_info = JsonConvert.DeserializeObject<RefInfo>(ref_info_str);
        if (ref_info.sha == nml_commit && File.Exists(Paths.NMLPath)) return false;
        
        string release_info_str = HttpUtils.Request(release_url);
        
        ReleaseInfo release_info = JsonConvert.DeserializeObject<ReleaseInfo>(release_info_str);
        
        if (release_info.assets.Length == 0) return false;
        using WebClient client = new();
        foreach (ReleaseAsset asset in release_info.assets)
        {
            if (!asset.name.StartsWith("NeoModLoader")) continue;
            if (asset.name.EndsWith(".pdb"))
            {
                string download_path = Path.Combine(Path.GetTempPath(), $"NML_{nml_commit}.pdb");
                string download_completed_path = Path.Combine(Path.GetTempPath(), $"NML_{nml_commit}_completed.pdb");
                if (!File.Exists(download_completed_path))
                {
                    Debug.Log("Downloading NML pdb from github");
                    try
                    {
                        client.DownloadFile(GetDownloadMirrorUrl(asset.browser_download_url), download_path);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    File.Move(download_path, download_completed_path);
                }
                TryReplaceFile(Paths.NMLPdbPath, download_completed_path);
                continue;
            }

            if (asset.name.EndsWith(".dll"))
            {
                string download_path = Path.Combine(Path.GetTempPath(), $"NML_{nml_commit}.dll");
                string download_completed_path = Path.Combine(Path.GetTempPath(), $"NML_{nml_commit}_completed.dll");
                if (!File.Exists(download_completed_path))
                {
                    Debug.Log("Downloading NML dll from github");
                    try
                    {
                        client.DownloadFile(GetDownloadMirrorUrl(asset.browser_download_url), download_path);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (CheckValid(download_path))
                    {
                        continue;
                    }
                    File.Move(download_path, download_completed_path);
                }

                if (File.Exists(Paths.NMLPath))
                {
                    try
                    {
                        File.Delete(Paths.NMLPath);
                    }
                    catch (Exception)
                    {
                        // ignored
                        _nml_is_taken = true;
                        continue;
                    }
                }
                int result = TryReplaceFile(Paths.NMLInstallPath, download_completed_path);
                switch (result)
                {
                    case -1:
                        _nml_is_taken = true;
                        continue;
                    case 0:
                        continue;
                    case 1:
                        _nml_is_updated = true;
                        try
                        {
                            File.Delete(download_path);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        continue;
                }

                continue;
            }
        }
        
        return true;
    }

    private bool TryUpdateNMLFromWorkshop()
    {
        if (_nml_is_taken) return false;
        
        if (!Directory.Exists(Paths.NMLWorkshopPath)) return false;
        string[] files = Directory.GetFiles(Paths.NMLWorkshopPath);
        if (files.Length == 0) return false;
        foreach (string file in files)
        {
            string file_name = Path.GetFileName(file);
            if (!file_name.StartsWith("NeoModLoader")) continue;

            if (file_name.EndsWith(".pdb"))
            {
                TryReplaceFile(Paths.NMLPdbPath, file);
                continue;
            }

            if (file_name.EndsWith(".dll"))
            {
                int result = TryReplaceFile(Paths.NMLPath, file);
                switch (result)
                {
                    case -1:
                        _nml_is_taken = true;
                        continue;
                    case 0:
                        continue;
                    case 1:
                        _nml_is_updated = true;
                        continue;
                }
                continue;
            }
        }
        return true;
    }

    private int TryReplaceFile(string pOldFile, string pNewFile)
    {
        FileInfo old_file = new(pOldFile);
        FileInfo new_file = new(pNewFile);
        if (old_file.Exists && old_file.LastWriteTime >= new_file.LastWriteTime) return 0;
        try
        {
            old_file.Delete();
        }
        catch (Exception e)
        {
            return -1;
        }
        File.Copy(pNewFile, pOldFile, true);
        return 1;
    }
}
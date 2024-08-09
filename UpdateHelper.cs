using System;
using System.IO;
using System.Net;
using System.Reflection;

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

    public static string DownloadFile(string download_url, string postfix, string filename)
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
                client.DownloadFile(download_url, download_path);
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
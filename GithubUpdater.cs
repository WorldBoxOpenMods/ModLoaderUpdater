using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace NeoModLoader.AutoUpdate;

public class GithubUpdater : AUpdater
{
    private const string release_url = "https://api.github.com/repos/WorldBoxOpenMods/ModLoader/releases/tags/latest";
    private       Version online_version;
    private       ReleaseInfo release_info;

    public override bool CheckUpdate()
    {
        release_info = JsonConvert.DeserializeObject<ReleaseInfo>(HttpUtils.Request(release_url));
        var left_idx = release_info.name.IndexOf('(');
        var right_idx = release_info.name.IndexOf(')');
        online_version = new Version(release_info.name.Substring(left_idx + 1, right_idx - left_idx - 1));
        return online_version > WorldBoxMod.CurrentVersion;
    }

    public override bool IsAvailable()
    {
        return LocalizedTextManager.instance.language != "cz";
    }

    public override UpdateResult DownloadAndReplace()
    {
        if (release_info.assets.Length == 0) return UpdateResult.Fail;

        var download_postfix = online_version.ToString().Replace('.', '-');

        using var client = new WebClient();
        ReleaseAsset pdb_asset = release_info.assets.FirstOrDefault(x => x.name.EndsWith(".pdb"));
        if (!string.IsNullOrEmpty(pdb_asset?.browser_download_url))
        {
            var downloaded =
                UpdateHelper.DownloadFile(pdb_asset.browser_download_url, download_postfix, "NeoModLoader.pdb");
            if (!string.IsNullOrEmpty(downloaded)) UpdateHelper.TryReplaceFile(Paths.NMLPdbPath, downloaded);
        }

        ReleaseAsset dll_asset = release_info.assets.FirstOrDefault(x => x.name.EndsWith(".dll"));
        if (!string.IsNullOrEmpty(dll_asset?.browser_download_url))
        {
            var downloaded =
                UpdateHelper.DownloadFile(dll_asset.browser_download_url, download_postfix, "NeoModLoader.dll");
            if (!string.IsNullOrEmpty(downloaded))
            {
                if (UpdateHelper.CheckValid(downloaded)) return UpdateHelper.TryReplaceFile(Paths.NMLPath, downloaded);

                return UpdateResult.Fail;
            }
        }

        return UpdateResult.Fail;
    }

    private class ReleaseInfo
    {
        public ReleaseAsset[] assets;
        public string         name;
    }

    private class ReleaseAsset
    {
        public string browser_download_url;
        public string name;
    }
}
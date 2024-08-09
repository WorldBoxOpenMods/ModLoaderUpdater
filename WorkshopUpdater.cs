using System.IO;
using System.Linq;

namespace NeoModLoader.AutoUpdate;

public class WorkshopUpdater : AUpdater
{
    public override bool CheckUpdate()
    {
        return false;
    }

    public override bool IsAvailable()
    {
        return Directory.Exists(Paths.NMLWorkshopPath);
    }

    public override UpdateResult DownloadAndReplace()
    {
        var files = Directory.GetFiles(Paths.NMLWorkshopPath);
        if (files.Length == 0) return UpdateResult.NoNeedUpdate;

        var pdb_file = files.FirstOrDefault(x => x.EndsWith(".pdb"));
        if (!string.IsNullOrEmpty(pdb_file)) UpdateHelper.TryReplaceFile(Paths.NMLPdbPath, pdb_file);

        var dll_file = files.FirstOrDefault(x => x.EndsWith(".dll"));
        if (!string.IsNullOrEmpty(dll_file)) return UpdateHelper.TryReplaceFile(Paths.NMLPath, dll_file);

        return UpdateResult.NoNeedUpdate;
    }
}
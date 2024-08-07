using System.IO;
using System.Linq;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

internal class Paths
{
    public static readonly string StreamingAssetsPath = Combine(Application.streamingAssetsPath);
    public static  string GamePath => Application.platform switch
    {
        RuntimePlatform.WindowsPlayer => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.LinuxPlayer   => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.OSXPlayer     => Combine(StreamingAssetsPath, "..", "..", "..", "..", ".."),
        _                             => Combine(StreamingAssetsPath, "..", "..")
    };
    
    public static readonly string NMLWorkshopPath =
        Combine(GamePath, "..", "..", "workshop", "content", "1206560", "3080294469");

    public static string NMLPath => Directory.GetFiles(Combine(StreamingAssetsPath, "Mods")).FirstOrDefault(file =>
                                           Path.GetFileName(file).StartsWith("NeoModLoader") &&
                                           Path.GetFileName(file).EndsWith(".dll") &&
                                           !Path.GetFileName(file).Contains("AutoUpdate")) ??
                                       Combine(StreamingAssetsPath, "Mods", "NeoModLoader_memload.dll");
    
    public static string NMLInstallPath => Combine(StreamingAssetsPath, "Mods", "NeoModLoader_memload.dll");
    public static string NMLPdbPath => Combine(StreamingAssetsPath, "Mods", "NeoModLoader.pdb");
    public static string NMLCommitPath => Combine(StreamingAssetsPath, "Mods", "NML", "commit");
    private static string Combine(params string[] paths) => new FileInfo(paths.Aggregate("", Path.Combine)).FullName;
}
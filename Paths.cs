using System.IO;
using System.Linq;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

internal class Paths
{
    public static readonly string StreamingAssetsPath = Combine(Application.streamingAssetsPath);

    public static readonly string NMLWorkshopPath =
        Combine(GamePath, "..", "..", "workshop", "content", "1206560", "3080294469");

    public static string GamePath => Application.platform switch
    {
        RuntimePlatform.WindowsPlayer => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.LinuxPlayer   => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.OSXPlayer     => Combine(StreamingAssetsPath, "..", "..", "..", "..", ".."),
        _                             => Combine(StreamingAssetsPath, "..", "..")
    };

    public static string NMLPath { get; internal set; }

    public static string NMLPdbPath => Combine(StreamingAssetsPath, "Mods", "NeoModLoader.pdb");
    private static string Combine(params string[] paths) => new FileInfo(paths.Aggregate("", Path.Combine)).FullName;
}
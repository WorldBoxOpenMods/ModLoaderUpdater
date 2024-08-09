using System.Threading.Tasks;
using UnityEngine;

namespace NeoModLoader.AutoUpdate;

public abstract class AUpdater
{
    /// <summary>
    /// </summary>
    /// <returns>Whether update to date now</returns>
    public abstract bool CheckUpdate();

    public abstract bool               IsAvailable();
    public abstract Task<UpdateResult> DownloadAndReplace();

    /// <summary>
    /// </summary>
    /// <returns>Whether update to date now</returns>
    public async Task<bool> Update()
    {
        if (!IsAvailable())
        {
            Debug.Log($"{GetType().Name} is not available");
            return false;
        }

        if (CheckUpdate()) return true;

        UpdateResult res = await DownloadAndReplace();
        switch (res)
        {
            case UpdateResult.IsTaken:
                return true;
            case UpdateResult.Success:
                return true;
            case UpdateResult.NoNeedUpdate:
                return true;
            case UpdateResult.Fail:
                return false;
        }

        return false;
    }
}
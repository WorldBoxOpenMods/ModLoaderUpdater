namespace NeoModLoader.AutoUpdate;

public abstract class AUpdater
{
    /// <summary>
    /// </summary>
    /// <returns>Whether update to date now</returns>
    public abstract bool CheckUpdate();

    public abstract bool         IsAvailable();
    public abstract UpdateResult DownloadAndReplace();

    /// <summary>
    /// </summary>
    /// <returns>Whether update to date now</returns>
    public bool Update()
    {
        if (!IsAvailable()) return false;

        if (CheckUpdate()) return true;

        UpdateResult res = DownloadAndReplace();
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
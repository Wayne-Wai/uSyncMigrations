using System.Diagnostics.CodeAnalysis;

namespace uSync.Migrations.Core.Upgrade;

/// <summary>
///  upgrades files from older versions of uSync to be compatible with the latest version.
/// </summary>
public interface ISyncUpgradeService
{
    string LatestFolder { get; }
    string LatestVersion { get; }

    /// <summary>
    ///  upgrades a folder to the latest version of uSync.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    Task<List<SyncUpgradeMessage>> UpgradeFolderAsync(string folderPath, string targetFolder);

    /// <summary>
    ///  fetches the latest legacy folder from disk. will return false if no legacy folder is found, 
    ///  or if the folder is not compatible with the upgrade process.
    /// </summary>
    bool TryGetLatestLegacyFolder([NotNullWhen(true)] out string? folderPath);


    /// <summary>
    ///  mark a folder to be ignored. 
    /// </summary>
    Task<bool> IgnoreLegacyFolderAsync(string folderPath, string message);
    Task<IEnumerable<SyncUpgradeMessage>> AnalyseFolderAsync(string folderPath);
}

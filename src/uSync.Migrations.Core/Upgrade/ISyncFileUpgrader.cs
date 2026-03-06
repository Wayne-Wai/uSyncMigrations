namespace uSync.Migrations.Core.Upgrade;

/// <summary>
///  handle the upgrading of files. 
/// </summary>
public interface ISyncFileUpgrader
{
    string ItemType { get; }

    Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file);
}

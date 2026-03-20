using uSync.BackOffice.Configuration;

namespace uSync.Migrations.Core.Upgrade;

/// <summary>
///  handle the upgrading of files. 
/// </summary>
public interface ISyncFileUpgrader
{
    string ItemType { get; }

    Task<SyncUpgradeResult> UpgradeFilesAsync(SyncUpgradeFile file);
    Task<IEnumerable<SyncUpgradeMessage>> AnalyseFilesAsync(SyncUpgradeFile file);
}

public record SyncUpgradeResult
{
    public SyncUpgradeResult() { }

    public SyncUpgradeResult(bool success)
    {
        Success = success;
    }

    public bool Success { get; set; }
    public List<SyncUpgradeFile> Files { get; set; } = [];
    public List<SyncUpgradeMessage> Messages { get; set; } = [];

    public void MergeResults(SyncUpgradeResult result)
    {
        Success = result.Success;
        Files.AddRange(result.Files);
        Messages.AddRange(result.Messages);
    }
}

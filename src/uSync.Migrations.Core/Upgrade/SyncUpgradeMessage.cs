namespace uSync.Migrations.Core.Upgrade;

public class SyncUpgradeMessage
{
    public required SyncUpgradeStatus Status { get; set; }
    public required string Upgrader { get; set; }
    public required string FileName { get; set; }
    public string? Message { get; set; }

    public static SyncUpgradeMessage Create(SyncUpgradeStatus status, string upgrader, string fileName, string? message = null)
        => new SyncUpgradeMessage
        {
            Status = status,
            Upgrader = upgrader,
            FileName = fileName,
            Message = message
        };
}

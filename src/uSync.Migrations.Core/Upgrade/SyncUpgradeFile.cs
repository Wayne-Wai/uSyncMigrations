using System.Xml.Linq;

namespace uSync.Migrations.Core.Upgrade;

public class SyncUpgradeFile
{
    public required string Filename { get; set; }
    public required XElement Node { get; set; }
    public string? Content { get; set; }
}
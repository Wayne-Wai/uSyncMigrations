using System.Xml.Linq;

using uSync.Core;

namespace uSync.Migrations.Core.Upgrade.Upgraders;

internal class SyncDataTypeFileUpgrader : SyncItemTypeFileUpgraderBase, ISyncFileUpgrader
{
    public SyncDataTypeFileUpgrader(SyncFileUpgraderCollection fileUpgraders)
        : base(fileUpgraders)
    { }

    public override string ItemType => "DataType";

    protected override string? GetItemKey(XElement node)
        => node.Element("Info")?.Element("EditorAlias").ValueOrDefault<string?>(null);
}
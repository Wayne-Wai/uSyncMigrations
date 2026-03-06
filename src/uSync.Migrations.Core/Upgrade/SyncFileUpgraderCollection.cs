using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace uSync.Migrations.Core.Upgrade;

public class SyncFileUpgraderCollection :
    BuilderCollectionBase<ISyncFileUpgrader>
{
    public SyncFileUpgraderCollection(Func<IEnumerable<ISyncFileUpgrader>> items) : base(items)
    { }

    public ISyncFileUpgrader[] GetUpgraders(string itemType)
        => [.. this
            .Where(x => x.ItemType.Equals(itemType, StringComparison.OrdinalIgnoreCase))
            .WhereNotNull()];
}

public class SyncFileUpgraderCollectionBuilder
    : LazyCollectionBuilderBase<SyncFileUpgraderCollectionBuilder, SyncFileUpgraderCollection, ISyncFileUpgrader>
{
    protected override SyncFileUpgraderCollectionBuilder This => this;
}
using Umbraco.Cms.Core.Models.Blocks;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

internal abstract class SyncBlockMigratorBase
{
    public virtual IEnumerable<BlockItemData> GetPropertyContentBlocks(GridValue.GridControl control)
        => [];

    public virtual IEnumerable<BlockItemData> GetPropertySettingsBlocks(GridValue.GridControl control)
        => [];

    public virtual string? GetContentTypeAlias(GridValue.GridControl control) => null;

}

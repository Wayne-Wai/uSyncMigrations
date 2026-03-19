using System.Xml.Linq;

using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

public interface IGridSettingsViewMigrator
{
    string ViewAlias { get; }
    string? GetDataTypeAlias(string gridAlias, string? configItemLabel);
    Task<XElement?> GetAdditionalDataTypeAsync(string dataTypeAlias, List<GridSettingsConfigurationItemPreValue>? preValues);
    Task<IEnumerable<UmbBlockGridTypeModel>> GetAdditionalGridBlocksAsync(string gridAlias, string blockLabel, Guid? groupKey);
    Task<IEnumerable<Guid>> GetAllowedElementKeysAsync(string elementAlias);

}

public class GridSettingsViewMigratorCollectionBuilder
    : LazyCollectionBuilderBase<GridSettingsViewMigratorCollectionBuilder, GridSettingsViewMigratorCollection, IGridSettingsViewMigrator>
{
    protected override GridSettingsViewMigratorCollectionBuilder This => this;
}

public class GridSettingsViewMigratorCollection
    : BuilderCollectionBase<IGridSettingsViewMigrator>
{
    public GridSettingsViewMigratorCollection(Func<IEnumerable<IGridSettingsViewMigrator>> items) : base(items)
    { }

    public IGridSettingsViewMigrator? GetMigrator(string? alias)
    {
        if (alias == null) return null;
        return this.FirstOrDefault(x => x.ViewAlias.InvariantEquals(alias));
    }

    public IGridSettingsViewMigrator? GetMigratorOrDefault(string? alias)
    {
        if (alias == null) return null;
        return this.FirstOrDefault(x => x.ViewAlias.InvariantEquals(alias))
            ?? this.FirstOrDefault(x => x.ViewAlias.InvariantEquals("__default__"));
    }

    public async Task<IEnumerable<Guid>> GetAllowedElementKeysAsync(string elementAlias)
    {
        var migrator = GetMigrator(elementAlias);
        if (migrator == null) return [];
        return await migrator.GetAllowedElementKeysAsync(elementAlias);
    }
}
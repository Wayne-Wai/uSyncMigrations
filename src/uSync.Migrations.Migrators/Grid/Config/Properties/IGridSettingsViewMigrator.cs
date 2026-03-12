using System.Xml.Linq;

using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Properties;

public interface IGridSettingsViewMigrator
{
    string ViewAlias { get; }
    string GetDataTypeAlias(string gridAlias, string? configItemLabel);
    object ConvertContentString(string value);
    XElement? GetAdditionalDataType(string dataTypeAlias, List<GridSettingsConfigurationItemPreValue>? preValues);
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
}
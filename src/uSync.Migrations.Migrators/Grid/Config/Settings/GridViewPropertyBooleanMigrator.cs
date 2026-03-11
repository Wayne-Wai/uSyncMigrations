using Umbraco.Cms.Core;

namespace uSync.Migrations.Migrators.Grid.Config.Settings;

public class GridViewPropertyBooleanMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "Boolean";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel) =>
        "True/false";
}

using Umbraco.Cms.Core;

namespace uSync.Migrations.Migrators.Grid.Config.Properties;

public class GridViewPropertyNumberMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "Number";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "Numeric";
}

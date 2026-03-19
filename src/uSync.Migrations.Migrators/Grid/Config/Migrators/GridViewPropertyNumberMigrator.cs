namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

public class GridViewPropertyNumberMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "Number";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "Numeric";
}

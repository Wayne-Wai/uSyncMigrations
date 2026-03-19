namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

internal class GridConfigRteMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "rte";

    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "Richtext editor";
}

internal class GridConfigMediaMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "media";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "Media Picker";

}

internal class GridConfigTextStringMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "textstring";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "textstring";
}

internal class GridConfigDefaultMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "__default__";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => "Label (string)";
}
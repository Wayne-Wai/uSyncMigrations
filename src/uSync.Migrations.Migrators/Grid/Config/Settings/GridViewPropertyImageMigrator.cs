using Umbraco.Cms.Core;

namespace uSync.Migrations.Migrators.Grid.Config.Settings;

public class GridViewPropertyImageMigrator: GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "ImagePicker";
    public string GetDataTypeAlias(string gridAlias, string? configItemLabel) =>
        "Image Media Picker";
}

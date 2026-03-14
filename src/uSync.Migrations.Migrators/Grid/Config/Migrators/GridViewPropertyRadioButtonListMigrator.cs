using System.Xml.Linq;

using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Core.Extensions;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

public class GridViewPropertyRadioButtonListMigrator : GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    public string ViewAlias => "RadioButtonList";

    public string GetDataTypeAlias(string gridAlias, string? configItemLabel)
    {
        var newAlias = gridAlias;
        if (string.IsNullOrWhiteSpace(configItemLabel) is false)
        {
            newAlias += $"-{configItemLabel}";
        }
        newAlias += "-RadioButtonList";
        return newAlias;
    }

    public override XElement? GetAdditionalDataType(string dataTypeAlias, List<GridSettingsConfigurationItemPreValue>? preValues)
    {
        if (preValues is null) return null;

        var radioButtonConfig = new RadioButtonListConfig
        {
            Items = preValues.Select(x => x.Value).WhereNotNull().ToList() ?? []
        };

        return SyncMigrationDataTypeHelper.CreateDataType(
                dataTypeAlias,
                Constants.PropertyEditors.Aliases.RadioButtonList,
                SyncGridMigrations.ElementContainerName,
                radioButtonConfig.SerializeJsonString());
    }

    class RadioButtonListConfig
    {
        public required List<string> Items { get; set; }
    }
}
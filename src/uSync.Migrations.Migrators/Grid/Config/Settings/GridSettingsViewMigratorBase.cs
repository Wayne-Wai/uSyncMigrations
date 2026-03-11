using System.Xml.Linq;

using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Settings;

public abstract class GridSettingsViewMigratorBase
{
    public virtual object ConvertContentString(string value) => value;
    public virtual XElement? GetAdditionalDataType(string dataTypeAlias, List<GridSettingsConfigurationItemPreValue>? preValues) => null;

}

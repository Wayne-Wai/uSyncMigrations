using System.Text.Json;

using uSync.Core.Extensions;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

internal class DefaultBlockMigrator : ISyncBlockMigrator
{
    public string[] Aliases => [SyncGridMigrations.DefaultMigratorType];

    public Dictionary<string, object> GetPropertyValues(GridValue.GridControl control)
    {
        var value = control.Value?.GetValueKind() == JsonValueKind.String    
            ? control.Value.ToString()
            : control.Value?.SerializeJsonString(true);

        return new Dictionary<string, object>
        {
            { control.Editor.Alias, value ?? "(Blank)" }
        };
    }
}

using Umbraco.Cms.Core;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.Switcher;

public class SwitcherConfigurationMigrator : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(SwitcherConfigurationMigrator);

    public string[] Editors => ["Our.Umbraco.Switcher"];
    public override string? TargetEditor => Constants.PropertyEditors.Aliases.Boolean;

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        return Task.FromResult(MigratePropertyNames(configuration, new Dictionary<string, string>
        {
            { "hideLabel", "showLabels" },
            { "onLabelText", "onLabel" },
            { "offLabelText", "offLabel" },
            { "switchOn", "default" }
        }));
    }
}

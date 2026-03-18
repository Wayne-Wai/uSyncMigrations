using Umbraco.Cms.Core;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.Codery;

internal class TextCountToTextboxConfigurationMigrator : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(TextCountToTextboxConfigurationMigrator);
    public string[] Editors => ["Codery.TextCount"];

    public override string? TargetEditor => Constants.PropertyEditors.Aliases.TextBox;

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        return Task.FromResult(MigratePropertyNames(configuration, new Dictionary<string, string>
        {
            { "limit", "maxChars" },
        }));
    }
}

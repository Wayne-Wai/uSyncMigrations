using Umbraco.Cms.Core;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.CrumpledCharLimitEditor;

internal class CrumpledCharLimitEditorConfigurationMigrator : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(CrumpledCharLimitEditorConfigurationMigrator);
    public string[] Editors => ["Crumpled.CharLimitEditor"];
    public override string? TargetEditor => Constants.PropertyEditors.Aliases.TextBox;

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        return Task.FromResult(MigratePropertyNames(configuration, new Dictionary<string, string>
        {
            { "limit", "maxChars"},
        }));
    }
}

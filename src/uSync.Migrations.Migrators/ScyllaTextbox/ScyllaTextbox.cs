using Umbraco.Cms.Core;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.ScyllaTextbox;

internal class ScyllaTextbox : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(ScyllaTextbox);
    public string[] Editors => ["Scylla.TextboxWithCharacterCount"];
    public override string? TargetEditor => Constants.PropertyEditors.Aliases.TextBox;

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
        => Task.FromResult(configuration);
}

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.Seven.MegaNav;

internal class MegaNavConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(MegaNavConfigSerializer);

    public string[] Editors => ["Cogworks.Meganav", "Meganav"];

    public override string? TargetEditor => "Our.Umbraco.Meganav";

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
        => Task.FromResult(configuration);

}

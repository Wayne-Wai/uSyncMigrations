using Umbraco.Cms.Core;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.GibeLinkPicker;

internal class GibeLinkPickerConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(GibeLinkPickerConfigSerializer);
    public string[] Editors => ["Gibe.LinkPicker"];
    public override string? TargetEditor => Constants.PropertyEditors.Aliases.MultiUrlPicker;
    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>
        {
            { "maxNumber", 1 }
        });
    }
}


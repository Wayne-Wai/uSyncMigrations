using Microsoft.Extensions.Options;

using uSync.Core.DataTypes;
using uSync.Core.Mapping;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Migrators.NuPickers.Config.Models;
using uSync.Migrations.Migrators.NuPickers.Configuration;

namespace uSync.Migrations.Migrators.NuPickers.Config;

[RequiresPropertyEditor("Umbraco.Community.Contentment.DataList")]
internal class NuPickersEnumDropDownPickerConfigSerializer : NuPickersConfigurationDataListBase, IConfigurationSerializer
{
    public NuPickersEnumDropDownPickerConfigSerializer(IOptions<NuPickerMigrationOptions> options) : base(options)
    {
    }

    public string Name => nameof(NuPickersEnumDropDownPickerConfigSerializer);

    public string[] Editors => ["nuPickers.EnumDropDownPicker"];

    public override string? TargetEditor => "Umbraco.Community.Contentment.DataList";
    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        if (configuration.TryGetValueAsObject<NuPickersEnumConfig>("dataSource", out var nuPickersConfig) is false)
            return Task.FromResult(configuration);

        //Using an anonymous object for now, but this should be replaced with Contentment objects (when they're created).
        var dataSource = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.EnumDataListSource, Umbraco.Community.Contentment",
                value = new
                {
                    enumType = new[] { MapAssembly(nuPickersConfig?.AssemblyName) , MapNamespace(nuPickersConfig?.EnumName) }
                }
            }
        }.ToList();

        var listEditor = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.DropdownListDataListEditor, Umbraco.Community.Contentment",
                value = new
                {
                    allowEmpty = "false"
                }
            }
        }.ToList();

        var config = new Dictionary<string, object>()
        {
            { "dataSource", dataSource },
            { "listEditor", listEditor }
        };

        return Task.FromResult<IDictionary<string, object>>(config);
    }
}

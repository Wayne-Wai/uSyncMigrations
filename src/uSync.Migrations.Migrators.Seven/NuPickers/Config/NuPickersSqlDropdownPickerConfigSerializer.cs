using Microsoft.Extensions.Options;

using uSync.Core.DataTypes;
using uSync.Core.Mapping;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Migrators.NuPickers.Config.Models;
using uSync.Migrations.Migrators.NuPickers.Configuration;

namespace uSync.Migrations.Migrators.NuPickers.Config;

[RequiresPropertyEditor("Umbraco.Community.Contentment.DataList")]
internal class NuPickersSqlDropdownPickerConfigSerializer : NuPickersConfigurationDataListBase, IConfigurationSerializer
{
    public NuPickersSqlDropdownPickerConfigSerializer(IOptions<NuPickerMigrationOptions> options) : base(options)
    {
    }

    public string Name => nameof(NuPickersSqlDropdownPickerConfigSerializer);

    public string[] Editors => ["nuPickers.SqlDropdownPicker"];

    public override string? TargetEditor => "Umbraco.Community.Contentment.DataList";

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        if (configuration.TryGetValueAsObject<NuPickersSqlConfig>("dataSource", out var nuPickersConfig) is false)
            return Task.FromResult(configuration);

        //Using an anonymous object for now, but this should be replaced with Contentment objects (when they're created).
        var dataSource = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.SqlDataListSource, Umbraco.Community.Contentment",
                value = new
                {
                    Query = new [] {
                        nuPickersConfig?.Query,
                        nuPickersConfig?.ConnectionString
                    }
                }
            }
        }.ToList();

        var listEditor = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.DropdownListDataListEditor, Umbraco.Community.Contentment",
                value = new
                {
                    allowEmpty = "1",
                    htmlAttributes = Array.Empty<object>()
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

using uSync.Core.DataTypes;
using uSync.Core.Mapping;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Migrators.NuPickers.Config.Models;

namespace uSync.Migrations.Migrators.NuPickers.Config;

[RequiresPropertyEditor("Umbraco.Community.Contentment.DataList")]
internal class NuPickersXPathPickersConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(NuPickersXPathPickersConfigSerializer);
    public string[] Editors => ["nuPickers.XmlCheckBoxPicker", "nuPickers.XmlDropdownPicker"];
    public override string? TargetEditor => "Umbraco.Community.Contentment.DataList";

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        if (configuration.TryGetValueAsObject<NuPickersXmlConfig>("dataSource", out var nuPickersConfig) is false)
            return Task.FromResult(configuration);

        // replace non-standard token '$ancestorOrSelf' parsed by nuPickers, into a Contentment '$current' placeholder token
        nuPickersConfig.XPath = nuPickersConfig.XPath?.Replace("$ancestorOrSelf", "$current") ?? null;

        //Using an anonymous object - we don't have access to contment objects as contentment might not be installed. 
        var dataSource = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.UmbracoContentXPathDataListSource, Umbraco.Community.Contentment",
                value = new
                {
                    xpath = nuPickersConfig?.XPath
                }
            }
        }.ToList();

        var listEditor = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.CheckboxListDataListEditor, Umbraco.Community.Contentment",
                value = new
                {
                    checkAll = "false"
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

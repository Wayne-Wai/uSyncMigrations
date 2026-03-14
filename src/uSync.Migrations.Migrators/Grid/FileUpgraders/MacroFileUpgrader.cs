using Examine;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Upgrade;

namespace uSync.Migrations.Migrators.Grid.FileUpgraders;

/// <summary>
///  turns macros into content types. 
/// </summary>
internal class MacroFileUpgrader : ISyncFileUpgrader
{
    private const string MacroContainer = "Macros";
    
    private readonly IShortStringHelper _shortStringHelper;

    public MacroFileUpgrader(IShortStringHelper shortStringHelper)
    {
        _shortStringHelper = shortStringHelper;
    }

    public string ItemType => "Macro";

    public async Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var alias = file.Node.GetAlias();
        var key = file.Node.GetKey();
        var name = file.Node.Element("Name").ValueOrDefault<string?>(alias);

        List<SyncUpgradeFile> files = [];
        List<SyncDataTypeInfo> dataTypes = [];

        foreach (var propertyNode in file.Node.Element("Properties")?.Elements("Property") ?? [])
        {
            var propertyAlias = propertyNode.Element("Alias").ValueOrDefault<string?>(null) ;
            var editorAlias = propertyNode.Element("EditorAlias").ValueOrDefault<string?>(null);
            if (propertyAlias is null || editorAlias is null) continue;

            var propertyName = propertyNode.Element("Name").ValueOrDefault<string>(alias);

            var dataTypeFile = CreateDataTypeForProperty(propertyName, editorAlias);
            files.Add(dataTypeFile);

            dataTypes.Add(new SyncDataTypeInfo(
                Name: propertyName,
                Alias: editorAlias,
                Definition: dataTypeFile.Node.GetKey(),
                PropertyType: editorAlias,
                PropertyAlias: propertyAlias));
        }


        files.Add(new SyncUpgradeFile
        {
            Filename = Path.Combine("ContentTypes\\Macros", $"{alias}_macro_contenttype.config"),
            Node = SyncMigrationContentTypeHelper.CreateContentType(
                name: $"{name} - Macro",
                key: key,
                alias: alias,
                folder: MacroContainer,
                icon: "icon-settings color-red",
                description: "Migrated: Converted from macro",
                compositions: [],
                dataTypes: [.. dataTypes])
        });

        return files;
    }


    private SyncUpgradeFile CreateDataTypeForProperty(string name, string editorAlias)
    {
        var safeName = $"{name}_macro".ToSafeAlias(_shortStringHelper);

        return new SyncUpgradeFile
        {
            Filename = Path.Combine("DataTypes\\Macros", $"{safeName}.config"),
            Node = SyncMigrationDataTypeHelper.CreateDataType(
                name: name,
                propertyType: editorAlias,
                folder: MacroContainer,
                configuration: "{}"
            )
        };
    }
}

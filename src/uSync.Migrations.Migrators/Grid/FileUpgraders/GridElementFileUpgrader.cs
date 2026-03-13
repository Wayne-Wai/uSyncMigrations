using System.Diagnostics;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Config.Properties;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.FileUpgraders;

/// <summary>
///  the grid upgrader
/// </summary>
/// <remarks>
///  Upgraders run as part of the file copy process - so they only run when the user 
///  clicks 'upgrade' in the migrations tab. 
///  
///  upgraders have access to the file, and they go and tell uSync that there needs 
///  to be additional files created as part of the upgrade. 
///  
///  so for example, the grid upgrader will create new content types for the grid editors,
///  and then add those to the list of files to be saved as part of the upgrade process.
///  
///  then when the imports happen the extra content types will exist and the other parts 
///  of the upgrade process can handle it. 
/// </remarks>  
internal class GridElementFileUpgrader : GridFileUpgraderBase, ISyncFileUpgrader
{
    private readonly GridSettingsViewMigratorCollection _settingsMigrators;
    private readonly ISyncGridNameService _gridNameHelper;

    public GridElementFileUpgrader(GridSettingsViewMigratorCollection settingsMigrators, ISyncGridNameService gridNameHelper, IDataTypeService dataTypeService)
        : base(dataTypeService)
    { 
        _settingsMigrators = settingsMigrators;
        _gridNameHelper = gridNameHelper;
    }

    public string ItemType => "DataType:Umbraco.Grid";

    public async Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var config = file.Node.Element("Config").ValueOrDefault<string?>(null);
        if (string.IsNullOrWhiteSpace(config))
            return [file];

        if (config.TryDeserialize<GridConfiguration>(out var gridConfiguration) is false || gridConfiguration is null)
            return [file];


        var alias = file.Node.GetAlias();

        // here we have the grid config, so if there is anything we want to do - pre import, 
        // we can do it here. 
        var results = new List<SyncUpgradeFile>();

        results.AddRange(CreateTemplateElements(alias, gridConfiguration));
        results.AddRange(CreateLayoutElements(alias, gridConfiguration));
        results.AddRange(CreateSettingsElements(alias, gridConfiguration));

        return results;
    }

    /// <summary>
    ///  creates a document type for each 'template' in the grid config. 
    /// </summary>
    /// <remarks>
    ///  at this point, we don't care about structure or layout of the template, only 
    ///  if it contains sections, because if it does we will need to create areas for it
    ///  during the import. 
    /// </remarks>
    private IEnumerable<SyncUpgradeFile> CreateTemplateElements(string alias, GridConfiguration gridConfiguration)
    {
        foreach(var template in gridConfiguration.Items?.Templates ?? [])
        {
            if (template.Sections is null) continue;

            var contentTypeAlias = _gridNameHelper.GetTemplateContentTypeAlias(alias, template.Name);

            yield return new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Template_{alias}_{template.Name}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{template.Name} - {alias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.LayoutContainerName,
                    icon: "icon-layout color-green",
                    description: "Migrated: Grid Template",
                    dataTypes: [])
            };
        }
    }

    /// <summary>
    ///  Add content types for the grid layouts - again only if the layout has areas. 
    /// </summary>
    private IEnumerable<SyncUpgradeFile> CreateLayoutElements(string alias, GridConfiguration gridConfiguration)
    {
        foreach(var layout in gridConfiguration.Items?.Layouts ?? [])
        {
            if (layout.Areas is null || layout.Areas.Count == 0)
                continue;

            var contentTypeAlias = _gridNameHelper.GetLayoutContentTypeAlias(alias, layout.Name);

            yield return new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Layout_{alias}_{layout.Name}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{layout.Name} - {alias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.LayoutContainerName,
                    icon: "icon-layout color-blue",
                    description: "Migrated : Grid Layout ",
                    dataTypes: [])
            };            
        }
    }

    /// <summary>
    ///  Add any datatypes that might need to exist for the grid settings to be implimented in the grid.
    /// </summary>
    private IEnumerable<SyncUpgradeFile> CreateSettingsElements(string gridAlias, GridConfiguration gridConfiguration)
    {
        var groups = gridConfiguration.Items?.Config?.GroupBy(x => x.GetAppliesToValue());
        if (groups is null) return [];

        List<SyncUpgradeFile> results = [];

        foreach (var configGroup in groups)
        {
            var dataTypes = new List<SyncDataTypeInfo>(); 

            foreach (var config in configGroup)
            {
                var migrator = _settingsMigrators.GetMigrator(config.View);
                if (migrator is null) continue;
                
                var dataTypeAlias = migrator.GetDataTypeAlias(gridAlias, config.Label);
                var node = migrator.GetAdditionalDataType(dataTypeAlias, config.PreValues);
                if (node is null)
                {
                    var dataType = GetDataType(dataTypeAlias).Result;
                    dataTypes.Add(new SyncDataTypeInfo(
                        Name: $"{config.Label}",
                        Alias: dataTypeAlias,
                        Definition: dataType?.Key ?? dataTypeAlias.ToGuid(),
                        PropertyType: dataType?.EditorAlias ?? Constants.PropertyEditors.Aliases.Label,
                        propertyAlias: $"{config.Key ?? config.Label}"));

                    continue;
                }
                
                results.Add(new SyncUpgradeFile
                {
                    Filename = Path.Combine(SyncGridMigrations.DataTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Settings_{gridAlias}_{dataTypeAlias}")),
                    Node = node
                });

                dataTypes.Add(new SyncDataTypeInfo(
                    Name: $"{config.Label}",
                    Alias: dataTypeAlias,
                    Definition: node.GetKey(),
                    PropertyType: dataTypeAlias,
                    propertyAlias: $"{config.Key ?? config.Label}"
                ));
            }

            var contentTypeAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, configGroup.Key);

            results.Add(new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Settings_{gridAlias}_{configGroup.Key}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{configGroup.Key} Settings {gridAlias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.SettingsContainerName,
                    icon: "icon-settings color-orange",
                    description: $"Migrated : Grid Settings from {gridAlias}",
                    dataTypes: [.. dataTypes])
            });
        }

        return results;
    }


}

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Config.Migrators;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.FileUpgraders;

/// <summary>
///  a special upgrader that handles the grid.editors.config.js file. 
/// </summary>
/// <remarks>
///  the grid.editors.config.js file contains grid editors that will not 
///  exist inside a modern version of umbraco, so this upgrader will
///  need to create the content types for these grid editors so 
///  when the upgrade runs we can refernece them 
/// </remarks>

internal class GridEditorsConfigFileUpgrader : GridFileUpgraderBase, ISyncFileUpgrader
{
    public string ItemType => "grid.editors.config.js";

    private readonly ISyncGridNameService _gridNameHelper;
    private readonly GridSettingsViewMigratorCollection _settingsMigrators;

    public GridEditorsConfigFileUpgrader(ISyncGridNameService gridNameHelper, GridSettingsViewMigratorCollection settingsMigrators, IDataTypeService dataTypeService)
        : base(dataTypeService)
    {
        _gridNameHelper = gridNameHelper;
        _settingsMigrators = settingsMigrators;
    }

    public async Task<SyncUpgradeResult> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var result = new SyncUpgradeResult(true);

        if (file.Content.TryDeserialize<GridEditor[]>(out var config) is false || config is null)
            return result;

        foreach (var editor in config)
        {
            if (editor is null) continue;

            // we get the migrator or fallback (to a label migrator) based on the 'view' of the editor,
            var migrator = _settingsMigrators.GetMigratorOrDefault(editor.View);
            if (migrator is null) continue;

            var dataTypeAlias = migrator.GetDataTypeAlias("config", editor.View);
            if (dataTypeAlias is null) continue;

            var definition = Guid.NewGuid();
            var propertyType = Constants.PropertyEditors.Aliases.Label;

            var node = await migrator.GetAdditionalDataTypeAsync(dataTypeAlias, null);
            if (node is null)
            {
                // if the migrator is using a pre-existing datatype alias, we will try to find it and use it.
                var dataType = GetDataType(dataTypeAlias).Result;
                if (dataType != null)
                {
                    definition = dataType.Key;
                    propertyType = dataType.EditorAlias;
                }
            }
            else
            {
                result.Messages.Add(new SyncUpgradeMessage
                {
                    FileName = file.Filename,
                    Upgrader = nameof(GridEditorsConfigFileUpgrader),
                    Status = SyncUpgradeStatus.Info,
                    Message = $"Creating data type for grid editor '{editor.Name}' with alias '{dataTypeAlias}' and editor view '{editor.View}'"
                });

                // create a new datatype for the value we will use in the 'grid element' content type.
                var upgradeFile = new SyncUpgradeFile
                {
                    Filename = Path.Combine(SyncGridMigrations.DataTypeFolder, _gridNameHelper.MakeSafeConfig($"grid_config_{dataTypeAlias}_datatype")),
                    Node = node
                };
                result.Files.Add(upgradeFile);

                definition = node.GetKey();
                propertyType = node.Element("Info")?.Element("EditorAlias")?.Value ?? Constants.PropertyEditors.Aliases.Label;
            }

            var dataTypeInfo = new SyncDataTypeInfo(
                Name: editor.Name,
                Alias: editor.Alias,
                Definition: definition,
                PropertyType: propertyType,
                PropertyAlias: editor.Alias);

            var contentTypeAlias = _gridNameHelper.GetElementContentTypeAlias(editor.Alias);

            result.Messages.Add(new SyncUpgradeMessage
            {
                FileName = file.Filename,
                Upgrader = nameof(GridEditorsConfigFileUpgrader),
                Status = SyncUpgradeStatus.Info,
                Message = $"Creating content type for grid editor '{editor.Name}' with alias '{contentTypeAlias}' and data type alias '{dataTypeAlias}' (editor view: '{editor.View}')"
            });

            // create the content type 'element' that will be used in the block grid to represent this editor.
            result.Files.Add(new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"grid_editor_{editor.Name}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                            name: editor.Name,
                            alias: contentTypeAlias,
                            folder: SyncGridMigrations.ElementContainerName,
                            icon: editor.Icon ?? "icon-bug",
                            description: "Migrated from grid config",
                            compositions: [],
                            dataTypes: [dataTypeInfo])
            });
        }

        return result;
    }

    public Task<IEnumerable<SyncUpgradeMessage>> AnalyseFilesAsync(SyncUpgradeFile file)
    {
        var result = new List<SyncUpgradeMessage>()
        {
            new SyncUpgradeMessage
            {
                FileName = file.Filename,
                Upgrader = nameof(GridEditorsConfigFileUpgrader),
                Status = SyncUpgradeStatus.Info,
                Message = $"Grid Editors Config file is present and will be used to create block level content types for grid migration."
            }
        };

        return Task.FromResult(result.AsEnumerable());
    }
}

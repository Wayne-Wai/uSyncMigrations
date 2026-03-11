using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Config.Settings;
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

    private readonly ISyncGridNameHelper _gridNameHelper;
    private readonly GridSettingsViewMigratorCollection _settingsMigrators;

    public GridEditorsConfigFileUpgrader(ISyncGridNameHelper gridNameHelper, GridSettingsViewMigratorCollection settingsMigrators, IDataTypeService dataTypeService)
        : base(dataTypeService)
    {
        _gridNameHelper = gridNameHelper;
        _settingsMigrators = settingsMigrators;
    }

    public async Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        if (file.Content.TryDeserialize<GridEditor[]>(out var config) is false || config is null) return [];

        var newContentTypes = new List<SyncUpgradeFile>();

        foreach (var editor in config)
        {
            if (editor is null) continue;

            // we get the migrator or fallback (to a label migrator) based on the 'view' of the editor,
            var migrator = _settingsMigrators.GetMigratorOrDefault(editor.View);
            if (migrator is null) continue;

            var dataTypeAlias = migrator.GetDataTypeAlias("config", editor.View);

            var definition = Guid.NewGuid();
            var propertyType = Constants.PropertyEditors.Aliases.Label;

            var node = migrator.GetAdditionalDataType(dataTypeAlias, null);
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
                // create a new datatype for the value we will use in the 'grid element' content type.
                var upgradeFile = new SyncUpgradeFile
                {
                    Filename = Path.Combine(SyncGridMigrations.DataTypeFolder, _gridNameHelper.MakeSafeConfig($"grid_config_{dataTypeAlias}_datatype")),
                    Node = node
                };
                newContentTypes.Add(upgradeFile);

                definition = node.GetKey();
                propertyType = node.Element("Info")?.Element("EditorAlias")?.Value ?? Constants.PropertyEditors.Aliases.Label;
            }

            var dataTypeInfo = new SyncDataTypeInfo(
                Name: editor.Name,
                Alias: dataTypeAlias,
                Definition: definition,
                PropertyType: propertyType);

            var contentTypeAlias = _gridNameHelper.GetElementContentTypeAlias(editor.Name);

            // create the content type 'element' that will be used in the block grid to represent this editor.
            newContentTypes.Add(new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"grid_editor_{editor.Name}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                            name: editor.Name,
                            alias: contentTypeAlias,
                            folder: SyncGridMigrations.ElementContainerName,
                            icon: editor.Icon ?? "icon-bug",
                            description: "Migrated from grid config",
                            dataTypes: [dataTypeInfo])
            });
        }

        return newContentTypes;
    }
}

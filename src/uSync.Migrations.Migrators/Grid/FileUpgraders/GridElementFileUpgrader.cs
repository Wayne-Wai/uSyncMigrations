using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Config.Migrators;
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

    public async Task<SyncUpgradeResult> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var result = new SyncUpgradeResult(true);

        var config = file.Node.Element("Config").ValueOrDefault<string?>(null);
        if (string.IsNullOrWhiteSpace(config))
        {
            result.Files = [file];
            return result;
        }

        if (config.TryDeserialize<GridConfiguration>(out var gridConfiguration) is false || gridConfiguration is null)
        {
            result.Files = [file];
            return result;
        }


        var alias = file.Node.GetAlias();

        // here we have the grid config, so if there is anything we want to do - pre import, 
        // we can do it here. 

        result.MergeResults(CreateTemplateElements(alias, gridConfiguration));
        result.MergeResults(CreateLayoutElements(alias, gridConfiguration));
        result.MergeResults(await CreateSettingsElementsAsync(alias, gridConfiguration));

        return result;
    }

    /// <summary>
    ///  creates a document type for each 'template' in the grid config. 
    /// </summary>
    /// <remarks>
    ///  at this point, we don't care about structure or layout of the template, only 
    ///  if it contains sections, because if it does we will need to create areas for it
    ///  during the import. 
    /// </remarks>
    private SyncUpgradeResult CreateTemplateElements(string alias, GridConfiguration gridConfiguration)
    {
        // Todo: flatten - if there is only one template, can we remove it ? 
        var result = new SyncUpgradeResult(true);

        foreach (var template in gridConfiguration.Items?.Templates ?? [])
        {
            if (template.Sections is null) continue;

            var contentTypeAlias = _gridNameHelper.GetTemplateContentTypeAlias(alias, template.Name);

            var filePath = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Template_{alias}_{template.Name}"));

            result.Messages.Add(new SyncUpgradeMessage
            {
                Upgrader = nameof(GridElementFileUpgrader),
                FileName = filePath,
                Status = SyncUpgradeStatus.Info,
                Message = $"Adding content type for template {template.Name} with alias {contentTypeAlias}",
            });

            result.Files.Add(new SyncUpgradeFile
            {
                Filename = filePath,
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{template.Name} - {alias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.LayoutContainerName,
                    icon: "icon-layout color-green",
                    description: "Migrated: Grid Template",
                    compositions: [],
                    dataTypes: [])
            });
        }

        return result;
    }

    /// <summary>
    ///  Add content types for the grid layouts - again only if the layout has areas. 
    /// </summary>
    private SyncUpgradeResult CreateLayoutElements(string alias, GridConfiguration gridConfiguration)
    {
        var result = new SyncUpgradeResult(true);

        foreach (var layout in gridConfiguration.Items?.Layouts ?? [])
        {
            if (layout.Areas is null || layout.Areas.Count == 0)
                continue;

            var contentTypeAlias = _gridNameHelper.GetLayoutContentTypeAlias(alias, layout.Name);

            result.Messages.Add(new SyncUpgradeMessage
            {
                Upgrader = nameof(GridElementFileUpgrader),
                FileName = contentTypeAlias,
                Status = SyncUpgradeStatus.Info,
                Message = $"Adding content type for layout {layout.Name} with alias {contentTypeAlias}",
            });

            result.Files.Add(new SyncUpgradeFile
            {
                Filename = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Layout_{alias}_{layout.Name}")),
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{layout.Name} - {alias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.LayoutContainerName,
                    icon: "icon-layout color-blue",
                    description: "Migrated : Grid Layout ",
                    compositions: [],
                    dataTypes: [])
            });
        }

        return result;
    }

    /// <summary>
    ///  Add any datatypes that might need to exist for the grid settings to be implimented in the grid.
    /// </summary>
    private async Task<SyncUpgradeResult> CreateSettingsElementsAsync(string gridAlias, GridConfiguration gridConfiguration)
    {
        List<GridConfigurationConfig> configAndStyles = [.. gridConfiguration.Items?.Config ?? [], .. gridConfiguration.Items?.Styles ?? []];

        var result = new SyncUpgradeResult(true);

        var groups = configAndStyles.GroupBy(x => x.GetAppliesToValue());
        if (groups is null) return result;

        var configGroupHasAppliesToAll = groups.Any(x => x.Key == SyncGridMigrations.ApplyTo.ApplyToAll);

        foreach (var configGroup in groups)
        {
            var dataTypes = new List<SyncDataTypeInfo>();

            foreach (var config in configGroup)
            {
                var migrator = _settingsMigrators.GetMigrator(config.View);
                if (migrator is null) continue;

                var dataTypeAlias = migrator.GetDataTypeAlias(gridAlias, config.Label);
                if (dataTypeAlias is null) continue;

                var node = await migrator.GetAdditionalDataTypeAsync(dataTypeAlias, config.PreValues);
                if (node is null)
                {
                    var dataType = await GetDataType(dataTypeAlias);
                    dataTypes.Add(new SyncDataTypeInfo(
                        Name: $"{config.Label}",
                        Alias: dataTypeAlias,
                        Definition: dataType?.Key ?? dataTypeAlias.ToGuid(),
                        PropertyType: dataType?.EditorAlias ?? Constants.PropertyEditors.Aliases.Label,
                        PropertyAlias: $"{config.Key ?? config.Label}"));

                    continue;
                }

                var dataTypeFilePath = Path.Combine(SyncGridMigrations.DataTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Settings_{gridAlias}_{dataTypeAlias}"));

                result.Messages.Add(new SyncUpgradeMessage
                {
                    Upgrader = nameof(GridElementFileUpgrader),
                    FileName = dataTypeFilePath,
                    Status = SyncUpgradeStatus.Info,
                    Message = $"Adding data type for grid setting {config.Label} with alias {dataTypeAlias}",
                });

                result.Files.Add(new SyncUpgradeFile
                {
                    Filename = dataTypeFilePath,
                    Node = node
                });

                dataTypes.Add(new SyncDataTypeInfo(
                    Name: $"{config.Label}",
                    Alias: dataTypeAlias,
                    Definition: node.GetKey(),
                    PropertyType: dataTypeAlias,
                    PropertyAlias: $"{config.Key ?? config.Label}"
                ));
            }


            // TODO: This works for 'All' but we need to also compose for row and area settings ? 
            var compositions =
                configGroupHasAppliesToAll ? GetCompositions(gridAlias, configGroup.Key) : [];

            var contentTypeAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, configGroup.Key);

            var contentTypeFilePath = Path.Combine(SyncGridMigrations.ContentTypeFolder, _gridNameHelper.MakeSafeConfig($"Grid_Settings_{gridAlias}_{configGroup.Key}"));

            result.Messages.Add(new SyncUpgradeMessage
            {
                Upgrader = nameof(GridElementFileUpgrader),
                FileName = contentTypeFilePath,
                Status = SyncUpgradeStatus.Info,
                Message = $"Adding content type for grid settings for {gridAlias} with alias {contentTypeAlias}",
            });

            result.Files.Add(new SyncUpgradeFile
            {
                Filename = contentTypeFilePath,
                Node = SyncMigrationContentTypeHelper.CreateContentType(
                    name: $"{configGroup.Key} Settings {gridAlias}",
                    alias: contentTypeAlias,
                    folder: SyncGridMigrations.SettingsContainerName,
                    icon: "icon-settings color-orange",
                    description: $"Migrated : Grid Settings from {gridAlias}",
                    compositions: [.. compositions],
                    dataTypes: [.. dataTypes])
            });
        }

        return result;
    }

    private IEnumerable<SyncCompositionInfo> GetCompositions(string gridAlias, string appliesTo)
    {
        // applies to all doesn't have any compositions
        if (appliesTo == SyncGridMigrations.ApplyTo.ApplyToAll) return [];

        var appliesToAllAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, SyncGridMigrations.ApplyTo.ApplyToAll);
        return [new SyncCompositionInfo(appliesToAllAlias.ToGuid(), appliesToAllAlias)];
    }

    public Task<IEnumerable<SyncUpgradeMessage>> AnalyseFilesAsync(SyncUpgradeFile file)
    {
        var config = file.Node.Element("Config").ValueOrDefault<string?>(null);
        if (string.IsNullOrWhiteSpace(config))
            return Task.FromResult<IEnumerable<SyncUpgradeMessage>>([
                new SyncUpgradeMessage
                {
                    Upgrader = nameof(GridElementFileUpgrader),
                    FileName = file.Filename,
                    Status = SyncUpgradeStatus.Warning,
                    Message = "Grid element is present but there is no grid configuration found",
                }]);

        if (config.TryDeserialize<GridConfiguration>(out var gridConfiguration) is false || gridConfiguration is null)
            return Task.FromResult<IEnumerable<SyncUpgradeMessage>>(
                [
                    new SyncUpgradeMessage
                    {
                        Upgrader = nameof(GridElementFileUpgrader),
                        FileName = file.Filename,
                        Status = SyncUpgradeStatus.Error,
                        Message = "Grid Configuration could not be parsed, might be corrupt, and cannot be migrated at this time.",
                    }
                ]);

        return Task.FromResult<IEnumerable<SyncUpgradeMessage>>(
        [
            new SyncUpgradeMessage
            {
                Upgrader = nameof(GridElementFileUpgrader),
                FileName = file.Filename,
                Status = SyncUpgradeStatus.Info,
                Message = $"Grid with {gridConfiguration.Items?.Templates?.Count ?? 0} templates, and {gridConfiguration.Items?.Layouts?.Count ?? 0} layouts.",
            }
        ]);
    }
}

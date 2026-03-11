using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core.DataTypes;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config;

internal class GridConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ISyncGridNameHelper _gridNameHelper;

    public GridConfigSerializer(IContentTypeService contentTypeService, ISyncGridNameHelper syncGridNameHelper)
    {
        _contentTypeService = contentTypeService;
        _gridNameHelper = syncGridNameHelper;
    }

    public override string? TargetEditor => Constants.PropertyEditors.Aliases.BlockGrid;
    public string Name => nameof(GridConfigSerializer);

    public string[] Editors => [Constants.PropertyEditors.Aliases.Grid];

    public override IDictionary<string, object> GetMigratedConfiguration(string name, IDictionary<string, object> configuration)
    {
        var gridConfig = configuration.SerializeJsonString().DeserializeJson<GridConfiguration>();
        if (gridConfig is null) { 
            return new Dictionary<string, object>(); 
        }

        var configHash = configuration.GetHashCode();

        var data = new SyncGridMigrationData
        {
            GridAlias = name,
            Items = gridConfig.Items,
            Config = gridConfig.Items?.Config ?? [],
            Styles = gridConfig.Items?.Styles ?? [],
            Groups = CreateGridBlockGroups(configHash)
        };

        return new Dictionary<string, object>
        {
            ["gridColumns"] = data.Items?.Columns ?? SyncGridMigrations.DefaultGridColumns,
            ["blockGroups"] = data.Groups,
            ["blocks"] = new List<UmbBlockGridTypeModel>([
                .. GetBlocksFromTemplates(data), 
                .. GetBlocksFromLayout(data)])
        };
    }

    private UmbBlockGridTypeGroupType[] CreateGridBlockGroups(int hashValue)
    {
        return [
            new UmbBlockGridTypeGroupType
            {
                Name = "Layouts",
                Key = $"layouts_{hashValue}".ToGuid()
            },
            new UmbBlockGridTypeGroupType
            {
                Name = "Elements",
                Key = $"elements_{hashValue}".ToGuid()
            }
        ];
    }

    private List<UmbBlockGridTypeModel> GetBlocksFromTemplates(SyncGridMigrationData data)
    {
        if (data.Items?.Templates is null) return [];

        var items = new List<UmbBlockGridTypeModel>();

        var elementGroupKey = data.Groups.FirstOrDefault(x => x.Name.Equals("Elements"))?.Key ?? Guid.NewGuid();

        foreach(var item in data.Items.Templates)
        {
            foreach (var (section, index) in item.Sections?.Select((x, i) => (x, i)) ?? [])
            {
                if (section.AllowAll)
                    items.AddRange(GetAllGridBlocks(elementGroupKey));

                foreach (var allowedItem in section.Allowed ?? [])
                {
                    var contentTypeKey = FindElementContentTypeKey(allowedItem);
                    if (contentTypeKey != null)
                    {
                        items.Add(new UmbBlockGridTypeModel
                        {
                            AllowAtRoot = section.Grid == SyncGridMigrations.DefaultGridColumns,
                            AllowInAreas = true,
                            ContentElementTypeKey = contentTypeKey.Value,
                            GroupKey = elementGroupKey
                        });
                    }
                }
            }
        }
        return items;     
    }

    private List<UmbBlockGridTypeModel> GetBlocksFromLayout(SyncGridMigrationData data)
    {
        if (data.Items?.Layouts is null) return [];

        var items = new List<UmbBlockGridTypeModel>();

        var layoutGroupKey = data.Groups.FirstOrDefault(x => x.Name.Equals("Layouts"))?.Key ?? Guid.NewGuid();

        foreach (var item in data.Items?.Layouts ?? [])
        {
            if (item.Areas is null) continue;

            var contentKey = FindContentContentTypeKey(data.GridAlias, item.Name);
            if (contentKey is null) continue;

            var block = new UmbBlockGridTypeModel
            {
                AllowAtRoot = true,
                AllowInAreas = true,
                ContentElementTypeKey = contentKey.Value,
                Label = item.Label,
                Areas = GetBlockAreasFromGrid(item.Name, item.Areas),
                GroupKey = layoutGroupKey
            };

            if (data.Config.Any(x => x.AppliesTo(item.Name)))
                block.SettingsElementTypeKey = FindSettingsContentTypeKey(data.GridAlias, item.Name);

            items.Add(block);
        }

        return items;
    }

    private UmbBlockGridAreaType[] GetBlockAreasFromGrid(string itemName, List<GridLayoutArea> gridAreas)
    {
        var areas = new List<UmbBlockGridAreaType>();
        foreach (var (area, index) in gridAreas.Select((x, i) => (x, i)))
        {
            var blockArea = new UmbBlockGridAreaType
            {
                Alias = $"Area_{index + 1}",
                ColumnSpan = area.Grid,
                RowSpan = 1,
                Key = $"{itemName}_Area_{index}".ToGuid(),
                MinAllowed = 0,
            };

            if (area.AllowAll is false && area.Allowed != null)
                blockArea.SpecifiedAllowance = GetSpecificAllowancesFromAllowed(area.Allowed);

            areas.Add(blockArea);
        }

        return [.. areas];
    }

    private List<UmbBlockGridTypeAreaTypePermissions>? GetSpecificAllowancesFromAllowed(List<string>? gridAllowedList)
    {
        var allowed = new List<UmbBlockGridTypeAreaTypePermissions>();

        foreach (var allowedElement in gridAllowedList ?? [])
        {
            var elementKey = FindElementContentTypeKey(allowedElement);
            if (elementKey is null) continue;

            allowed.Add(new UmbBlockGridTypeAreaTypePermissions
            {
                MinAllowed = 0,
                ElementTypeKey = elementKey.Value,
            });
        }

        return allowed;
    }

    private Guid? FindContentContentTypeKey(string gridAlias, string layout)
    {
        var contentTypeAlias = _gridNameHelper.GetLayoutContentTypeAlias(gridAlias, layout);
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null) return contentTypeAlias.ToGuid();
        return item.Key;
    }
    private Guid? FindSettingsContentTypeKey(string gridAlias, string layout)
    {
        var contentTypeAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, layout);       
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null) return contentTypeAlias.ToGuid();
        return item.Key;
    }
    private Guid? FindElementContentTypeKey(string elementAlias)
    {
        var contentTypeAlias = _gridNameHelper.GetElementContentTypeAlias(elementAlias);      
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null) return contentTypeAlias.ToGuid();
        return item.Key;
    }

    private IEnumerable<UmbBlockGridTypeModel> GetAllGridBlocks(Guid? groupKey)
    {
        // TODO, we need to fetch these in a more efficent way, a full content type fetch is a bit Databasey.
        var allContentTypes = _contentTypeService.GetAll();

        foreach(var contentType in allContentTypes.Where(x => x.Alias.StartsWith("Grid_Element")))
        {
            yield return new UmbBlockGridTypeModel
            {
                AllowAtRoot = false,
                AllowInAreas = true,
                ContentElementTypeKey = contentType.Key,
                Label = contentType.Name,
                GroupKey = groupKey
            };
        }
    }
}

internal class SyncGridMigrationData
{
    public required string GridAlias { get; init; }
    public required GridConfigurationItems? Items { get; init; }
    public required List<GridConfigurationConfig> Config { get; init; }
    public required List<GridConfigurationStyles> Styles { get; init; }
    public required UmbBlockGridTypeGroupType[] Groups { get; init; }
}

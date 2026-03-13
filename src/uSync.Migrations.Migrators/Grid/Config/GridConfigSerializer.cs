using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Core.DataTypes;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config;

internal class GridConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    private readonly ISyncGridContentTypeFinder _gridContentTypeFinder;
    private readonly ISyncGridNameService _gridNameService;

    public GridConfigSerializer(ISyncGridContentTypeFinder gridContentTypeFinder, ISyncGridNameService gridNameService)
    {
        _gridContentTypeFinder = gridContentTypeFinder;
        _gridNameService = gridNameService;
    }

    public override string? TargetEditor => Constants.PropertyEditors.Aliases.BlockGrid;
    public string Name => nameof(GridConfigSerializer);

    public string[] Editors => [Constants.PropertyEditors.Aliases.Grid];

    public override IDictionary<string, object> GetMigratedConfiguration(string name, IDictionary<string, object> configuration)
    {
        var gridConfig = configuration.SerializeJsonString().DeserializeJson<GridConfiguration>();
        if (gridConfig is null) return new Dictionary<string, object>();
        
        var data = new SyncGridMigrationData
        {
            GridAlias = name,
            Items = gridConfig.Items,
            Config = gridConfig.Items?.Config ?? [],
            Styles = gridConfig.Items?.Styles ?? [],
            GroupHelper = new SyncGridGroupHelper(configuration.GetHashCode())
        };

        var blocks = new List<UmbBlockGridTypeModel>([
                .. GetBlocksFromTemplates(data),
                .. GetBlocksFromLayout(data)]);

        var result = new Dictionary<string, object>
        {
            ["gridColumns"] = data.Items?.Columns ?? SyncGridMigrations.DefaultGridColumns,
            ["blockGroups"] = data.GroupHelper.Groups,
            ["blocks"] = blocks.DistinctBy(x => x.ContentElementTypeKey),
        };

        return result;
    }

    private List<UmbBlockGridTypeModel> GetBlocksFromTemplates(SyncGridMigrationData data)
    {
        if (data.Items?.Templates is null) return [];

        var items = new List<UmbBlockGridTypeModel>();

        var elementGroupKey = data.GroupHelper.GetElementGroupKey();
        var templateGroupKey = data.GroupHelper.GetTemplateGroupKey();

        foreach (var item in data.Items.Templates)
        {
            var templateKey = _gridContentTypeFinder.FindTemplateContentTypeKey(data.GridAlias, item.Name);

            items.Add(new UmbBlockGridTypeModel
            {
                ContentElementTypeKey = templateKey,
                AllowAtRoot = true,
                AllowInAreas = false,
                GroupKey = templateGroupKey,
                Areas = GetAreasFromTemplateSections(data.GridAlias, item.Name, item.Sections ?? [])
            });

            items.AddRange(GetTemplateSectionsAsBlocks(data.GridAlias, elementGroupKey, item));
        }
        return items;
    }

    private List<UmbBlockGridTypeModel> GetTemplateSectionsAsBlocks(string gridAlias, Guid? elementGroupKey, GridTemplate item)
    {
        var items = new List<UmbBlockGridTypeModel>();

        foreach (var (section, index) in item.Sections?.Select((x, i) => (x, i)) ?? [])
        {

            if (section.AllowAll)
                items.AddRange(GetAllGridBlocks(elementGroupKey));

            foreach (var allowedItem in section.Allowed ?? [])
            {
                var contentTypeKey = _gridContentTypeFinder.FindLayoutContentTypeKey(gridAlias, allowedItem);
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

        return items;
    }

    private UmbBlockGridAreaType[] GetAreasFromTemplateSections(string gridAlias, string templateName, List<GridTemplateSection> sections)
    {
        var areas = new List<UmbBlockGridAreaType>();
        foreach (var (section, index) in sections.Select((x, i) => (x, i)))
        {
            areas.Add(new UmbBlockGridAreaType
            {
                Alias = section.Grid == SyncGridMigrations.DefaultGridColumns ? "FullWidth" : $"Column_{section.Grid}",
                ColumnSpan = section.Grid,
                RowSpan = 1,
                Key = _gridNameService.MakeAreaKey(gridAlias, templateName, index, section.Grid),
                MinAllowed = 0,
                SpecifiedAllowance = section.AllowAll ? null : GetSpecificAllowancesFromAllowedSections(gridAlias, section.Allowed)
            });
        }
        return [.. areas];
    }

    private List<UmbBlockGridTypeAreaTypePermissions>? GetSpecificAllowancesFromAllowedSections(string gridAlias, List<string>? gridAllowedList)
    {
        var allowed = new List<UmbBlockGridTypeAreaTypePermissions>();

        foreach (var allowedElement in gridAllowedList ?? [])
        {
            var elementKey = _gridContentTypeFinder.FindLayoutContentTypeKey(gridAlias, allowedElement);
            if (elementKey is null) continue;

            allowed.Add(new UmbBlockGridTypeAreaTypePermissions
            {
                MinAllowed = 0,
                ElementTypeKey = elementKey.Value,
            });
        }

        return allowed;
    }

    private List<UmbBlockGridTypeModel> GetBlocksFromLayout(SyncGridMigrationData data)
    {
        if (data.Items?.Layouts is null) return [];

        var items = new List<UmbBlockGridTypeModel>();

        var layoutGroupKey = data.GroupHelper.GetLayoutGroupKey();
        var elementGroupKey = data.GroupHelper.GetElementGroupKey();

        foreach (var (item, index) in data.Items.Layouts.Select((x, i) => (x, i)))
        {
            if (item.Areas is null) continue;

            var contentKey = _gridContentTypeFinder.FindLayoutContentTypeKey(data.GridAlias, item.Name);
            if (contentKey is null) continue;

            var block = new UmbBlockGridTypeModel
            {
                AllowAtRoot = true,
                AllowInAreas = true,
                ContentElementTypeKey = contentKey.Value,
                Label = item.Label,
                Areas = GetBlockAreasFromGrid(data.GridAlias, item.Name, item.Areas),
                GroupKey = layoutGroupKey
            };

            if (data.Config.Any(x => x.AppliesTo(item.Name)))
                block.SettingsElementTypeKey = _gridContentTypeFinder.FindSettingsContentTypeKey(data.GridAlias, item.Name);

            items.Add(block);

            items.AddRange(GetAllowedBlocksFromLayout(data.GridAlias, item, elementGroupKey));
        }

        return items;
    }

    private List<UmbBlockGridTypeModel> GetAllowedBlocksFromLayout(string gridAlias, GridLayout layout, Guid? groupKey)
    {
        List<UmbBlockGridTypeModel> items = [];

        foreach(var area in layout.Areas ?? [])
        {
            if (area.AllowAll)
            {
                items.AddRange(GetAllGridBlocks(groupKey));
                continue;
            }

            foreach(var allowedItem in area.Allowed ?? [])
            {
                var elementKey = _gridContentTypeFinder.FindElementContentTypeKey(allowedItem);
                if (elementKey is null) continue;

                items.Add(new UmbBlockGridTypeModel
                {
                    AllowAtRoot = false,
                    AllowInAreas = true,
                    ContentElementTypeKey = elementKey.Value,
                    GroupKey = groupKey
                });
            }
        }

        return items;
    }

    private UmbBlockGridAreaType[] GetBlockAreasFromGrid(string gridAlias, string itemName, List<GridLayoutArea> gridAreas)
    {
        var areas = new List<UmbBlockGridAreaType>();
        foreach (var (area, index) in gridAreas.Select((x, i) => (x, i)))
        {
            var blockArea = new UmbBlockGridAreaType
            {
                Alias = $"Area_{index + 1}",
                ColumnSpan = area.Grid,
                RowSpan = 1,
                Key = _gridNameService.MakeAreaKey(gridAlias, itemName, index, area.Grid),
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
            var elementKey = _gridContentTypeFinder.FindElementContentTypeKey(allowedElement);
            if (elementKey is null) continue;

            allowed.Add(new UmbBlockGridTypeAreaTypePermissions
            {
                MinAllowed = 0,
                ElementTypeKey = elementKey.Value,
            });
        }

        return allowed;
    }

    public IEnumerable<UmbBlockGridTypeModel> GetAllGridBlocks(Guid? groupKey)
    {
        // TODO, we need to fetch these in a more efficent way, a full content type fetch is a bit Databasey.
        var blockContentTypes = _gridContentTypeFinder.GetAllGridBlockContentTypes(groupKey);

        foreach (var contentType in blockContentTypes)
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
    public required SyncGridGroupHelper GroupHelper { get; set; }
}

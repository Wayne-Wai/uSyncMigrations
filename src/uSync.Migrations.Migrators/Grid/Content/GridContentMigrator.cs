using System.Text.Json.Nodes;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Migrations.Migrators.Grid.Content.BlockMigrators;
using uSync.Migrations.Migrators.Grid.Extensions;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content;

internal class GridContentMigrator : SyncValueMapperBase, ISyncMapper, ISyncPropertyMapper
{
    private readonly ISyncGridNameService _gridNameHelper;
    private readonly ISyncGridContentTypeFinder _gridContentTypeFinder;
    private readonly IDataTypeService _dataTypeService;
    private readonly SyncBlockMigratorCollection _blockMigrators;
    private readonly SyncValueMapperCollection _valueMappers;

    public GridContentMigrator(
        IEntityService entityService,
        ISyncGridNameService gridNameHelper,
        IDataTypeService dataTypeService,
        ISyncGridContentTypeFinder gridContentTypeFinder,
        SyncBlockMigratorCollection blockMigrators,
        SyncValueMapperCollection valueMappers)
        : base(entityService)
    {
        _gridNameHelper = gridNameHelper;
        _dataTypeService = dataTypeService;
        _gridContentTypeFinder = gridContentTypeFinder;
        _blockMigrators = blockMigrators;
        _valueMappers = valueMappers;
    }

    public override string Name => nameof(GridContentMigrator);
    public override string[] Editors => [SyncLegacyTypes.Grid];

    public async Task<string?> GetImportValueAsync(string value, IPropertyType propertyType)
    {
        // migrators are responsible for checking if the migration has already taken place. 
        if (value.IsProbiblyAlmostCertainlyBlockGrid())
            return value;

        if (value.TryGetGridValue(out var gridValue) is false)
            return value;

        // fix the dtge views. 
        FixDtgeViews(gridValue);

        var gridDataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey);
        if (gridDataType is null) return value;

        var block = await ConvertGridValueToBlockValue(gridDataType, gridValue);
        if (block is null) return value;

        // ensure the expose values are also set. 
        if (block.Expose.Count == 0)
            block.Expose = [.. block.ContentData.Select(x => new BlockItemVariation(x.Key, null, null))];

        // json dance, this makes sure any escaped json inside the blocks is fully expanded into the json
        // object before we save it back, we can't tell if things internally will be escaped or not
        // as that is down to the mirator, but this forces it. 
        var node = block.SerializeJsonString().DeserializeJson<JsonNode>()?.ExpandAllJsonInToken();
        return node?.SerializeJsonString(true) ?? value;
    }

    private async Task<BlockValue?> ConvertGridValueToBlockValue(IDataType dataType, GridValue grid)
    {
        var gridAlias = dataType.Name;
        var templateAlias = grid.Name;

        if (gridAlias is null || templateAlias is null) return null;
        if (grid.Sections.Any() is false) return null;

        var templateContentType = _gridContentTypeFinder.FindTemplateContentType(gridAlias, templateAlias);
        if (templateContentType is null) return null;

        var sections = grid.Sections.Select(x =>
            (Columns: x.Grid ?? 0, x.Rows));

        var gridColumns = sections.Sum(x => x.Columns);
        var blockKey = Guid.NewGuid();

        BlockGridValue block = GridMigratorHelpers.CreateBlockGrid(blockKey, templateContentType.Key);
        BlockGridLayoutItem rootLayoutItem = GridMigratorHelpers.CreateBlockLayoutItem(blockKey, null, gridColumns);

        var rootLayoutAreas = new List<BlockGridLayoutAreaItem>();

        foreach (var (section, sectionIndex) in sections.Select((x, i) => (x, i)))
        {
            var sectionLayoutItems = new List<BlockGridLayoutItem>();

            foreach (var row in section.Rows)
            {
                if (row.Name is null) continue;

                var rowResult = await ConvertGridRow(gridAlias, row, row.Name);
                if (rowResult is null) continue;

                block.ContentData.AddRange(rowResult.ContentBlocks);
                block.SettingsData.AddRange(rowResult.SettingsBlocks);

                if (rowResult.Layouts is not null)
                    sectionLayoutItems.Add(rowResult.Layouts);
            }

            var sectionArea = GridMigratorHelpers.CreateBlockLayoutAreaItem(
                _gridNameHelper.MakeAreaKey(gridAlias, templateAlias, sectionIndex, section.Columns),
                sectionLayoutItems);

            rootLayoutAreas.Add(sectionArea);
        }

        if (block.ContentData.Count == 1) return new BlockGridValue();

        rootLayoutItem.Areas = [.. rootLayoutAreas];

        block.Layout.Add(Constants.PropertyEditors.Aliases.BlockGrid, [rootLayoutItem]);

        return block;
    }

    private async Task<BlockRowResult?> ConvertGridRow(string gridAlias, GridValue.GridRow row, string rowName)
    {
        var result = new BlockRowResult();

        var rowColumns = row.Areas.Sum(x => x.Grid ?? 0);
        var rowLayoutAreas = new List<BlockGridLayoutAreaItem>();

        foreach (var (area, areaIndex) in row.Areas.Select((x, i) => (x, i)))
        {
            var areaResult = await ConvertGridArea(gridAlias, rowName, areaIndex, area);
            if (areaResult is null) continue;

            if (areaResult.Layouts is not null)
                rowLayoutAreas.Add(areaResult.Layouts);

            result.ContentBlocks.AddRange(areaResult.ContentBlocks);
            result.SettingsBlocks.AddRange(areaResult.SettingsBlocks);
        }

        // anything to add for this row ? 
        if (rowLayoutAreas.Count == 0) return null;

        // setup the row content and settings block 
        var rowContentBlock = await GetBlockItemFromRow(rowName, gridAlias);
        if (rowContentBlock is null) return result; 
        
        result.ContentBlocks.Add(rowContentBlock);

        var rowSettingsBlock = await GetSettingsBlockFromGridBlock(row, gridAlias, rowName);
        if (rowSettingsBlock is not null)
            result.SettingsBlocks.Add(rowSettingsBlock);

        // add the row layout. 
        result.Layouts = GridMigratorHelpers.CreateBlockLayoutItem(
            rowContentBlock.Key,
            rowSettingsBlock?.Key,
            rowLayoutAreas,
            rowColumns);

        return result;
    }

    private async Task<BlockAreaResult?> ConvertGridArea(string gridAlias, string rowName, int areaIndex, GridValue.GridArea area)
    {
        var result = new BlockAreaResult();
        var areaLayoutBlocks = new List<BlockGridLayoutItem>();

        foreach (var control in area.Controls)
        {
            var contentBlock = await GetBlockItemDataFromControl(control);
            if (contentBlock is null) continue;

            result.ContentBlocks.Add(contentBlock);

            var settingsBlock = await GetSettingsBlockFromGridBlock(control, gridAlias, rowName);
            if (settingsBlock is not null)
                result.SettingsBlocks.Add(settingsBlock);
            
            areaLayoutBlocks.Add(GridMigratorHelpers.CreateBlockLayoutItem(contentBlock.Key, settingsBlock?.Key, area.Grid ?? 0));
        }

        if (result.ContentBlocks.Count == 0) return null;

        // areas don't have their own content or settings block,
        // but they do have a layout element 

        result.Layouts = GridMigratorHelpers.CreateBlockLayoutAreaItem(
            _gridNameHelper.MakeAreaKey(gridAlias, rowName, areaIndex, area.Grid ?? 0),
            areaLayoutBlocks);

        return result;
    }

    private async Task<BlockItemData?> GetBlockItemDataFromControl(GridValue.GridControl control)
    {
        if (control.Value is null) return null;

        var migrators = _blockMigrators.GetMigrators(control.Editor);
        if (migrators.Any() is false)
            return null;

        var contentType = _gridContentTypeFinder.FindElementContentType(control.Editor.Alias);
        if (contentType is null) return null;

        var data = GridMigratorHelpers.CreateBlockItemData(Guid.NewGuid(), contentType.Key);

        foreach (var migrator in migrators)
        {
            foreach (var (propertyAlias, value) in migrator.GetPropertyValues(control))
            {
                var blockValue = await GetBlockPropertyValue(contentType, propertyAlias, value);
                if (blockValue is null) continue;
                data.Values.Add(blockValue);
            }
        }

        return data;
    }


    private async Task<BlockItemData?> GetSettingsBlockFromGridBlock(GridValue.GridBlock gridBlock, string gridAlias, string? layoutAlias)
    {
        if (gridBlock.HasConfigOrStyles() is false) return null;

        var contentType = _gridContentTypeFinder.FindSettingsContentType(gridAlias, layoutAlias);
        if (contentType == null) return null;

        var settingsBlock = GridMigratorHelpers.CreateBlockItemData(Guid.NewGuid(), contentType.Key);

        if (gridBlock.HasConfig())
        {
            foreach (var property in gridBlock.Config)
            {
                var value = await GetBlockPropertyValue(contentType, property.Key, property.Value?.ToString());
                if (value is null) continue;
                settingsBlock.Values.Add(value);
            }
        }

        if (gridBlock.HasStyles())
        {
            foreach(var style in gridBlock.Styles)
            {
                var value = await GetBlockPropertyValue(contentType, style.Key, style.Value?.ToString());
                if (value is null) continue;
                settingsBlock.Values.Add(value);
            }
        }

        return settingsBlock;
    }

    private async Task<BlockPropertyValue?> GetBlockPropertyValue(IContentType contentType, string propertyAlias, object? propertyValue)
    {
        if (propertyValue is null) return null;
        var propertyStringValue = propertyValue.ToString();
        if (string.IsNullOrWhiteSpace(propertyStringValue)) return null;

        var safePropertyAlias = _gridNameHelper.MakeSafeSettingsKey(propertyAlias);
        var propertyType = contentType.GetPropertyType(safePropertyAlias);
        if (propertyType is null) return null;

        var mappedValue = propertyStringValue;

        var valueMappers = _valueMappers.GetSyncMappers(propertyType.PropertyEditorAlias);
        foreach (var valueMapper in valueMappers)
        {
            if (mappedValue is null) continue;
            mappedValue = await valueMapper.GetImportValueAsync(mappedValue, propertyType.PropertyEditorAlias);
        }

        return GridMigratorHelpers.CreateBlockPropertyValue(safePropertyAlias, mappedValue ?? propertyStringValue);
    }

    private async Task<BlockItemData?> GetBlockItemFromRow(string rowName, string dataTypeAlias)
    {
        var contentType = _gridContentTypeFinder.FindLayoutContentType(dataTypeAlias, rowName);
        if (contentType is null) return null;

        return GridMigratorHelpers.CreateBlockItemData(Guid.NewGuid(), contentType.Key);
    }

    private class BlockResultBase<TLayout>
    {
        public List<BlockItemData> ContentBlocks { get; init; } = [];
        public List<BlockItemData> SettingsBlocks { get; set; } = [];
        public TLayout? Layouts { get; set; }

    }

    private class BlockRowResult : BlockResultBase<BlockGridLayoutItem>;
    private class BlockAreaResult : BlockResultBase<BlockGridLayoutAreaItem>;

    ///// <summary>
    /////  there are some settings where a DTGE control can sometimes endup without a view.
    /////  this method just finds all those cases and gives them the default view. 
    ///// </summary>
    ///// <param name="grid"></param>
    private void FixDtgeViews(GridValue grid)
    {
    //    foreach (var control in grid.Sections.SelectMany(s => s.Rows).SelectMany(r => r.Areas).SelectMany(a => a.Controls)
    //        .Where(c => c.Editor.View is null && c.Value is JsonObject o && o.ContainsKey("dtgeContentTypeAlias")))
    //    {
    //        control.Editor.View = "/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html";
    //    }
    }
}
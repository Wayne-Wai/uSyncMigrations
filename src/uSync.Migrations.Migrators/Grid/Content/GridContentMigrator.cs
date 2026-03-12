using Microsoft.AspNetCore.Identity.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content;

internal class GridContentMigrator : SyncValueMapperBase, ISyncMapper, ISyncPropertyMapper
{
    private readonly ISyncGridNameService _gridNameHelper;
    private readonly IDataTypeService _dataTypeService;
    private readonly IContentTypeService _contentTypeService;

    public GridContentMigrator(
        IEntityService entityService,
        ISyncGridNameService gridNameHelper,
        IDataTypeService dataTypeService,
        IContentTypeService contentTypeService)
        : base(entityService)
    {
        _gridNameHelper = gridNameHelper;
        _dataTypeService = dataTypeService;
        _contentTypeService = contentTypeService;
    }

    public override string Name => nameof(GridContentMigrator);
    public override string[] Editors => [SyncLegacyTypes.Grid];

    public async Task<string?> GetImportValueAsync(string value, IPropertyType propertyType)
    {
        // migrators are responsible for checking if the migration has already taken place. 
        if (value.Contains("\"Umbraco.BlockGrid\""))
            return value;

        if (value.TryDeserialize<GridValue>(out var gridValue) is false || gridValue is null)
            return value;

        // fix the dtge views. 
        FixDtgeViews(gridValue);

        var gridDataType = await _dataTypeService.GetAsync(propertyType.DataTypeKey);
        if (gridDataType is null) return value;

        var block = ConvertToBlockValue(gridDataType, gridValue);
        if (block is null) return value;

        return block.SerializeJsonString(true);
    }

    private BlockValue? ConvertToBlockValue(IDataType dataType, GridValue grid) {

        if (grid.Sections.Any() is false) return null;

        var sectionContentTypeAlias = _gridNameHelper.GetTemplateContentTypeAlias(dataType.Name ?? "", grid.Name);
        var sectionContentType = _contentTypeService.Get(sectionContentTypeAlias);
        if (sectionContentType is null) return null;


        return null;
   
    }

    /// <summary>
    ///  there are some settings where a DTGE control can sometimes endup without a view.
    ///  this method just finds all those cases and gives them the default view. 
    /// </summary>
    /// <param name="grid"></param>
    private void FixDtgeViews(GridValue grid)
    {
        foreach(var control in grid.Sections.SelectMany(s => s.Rows).SelectMany(r => r.Areas).SelectMany(a => a.Controls)
            .Where(c => c.Editor.View is null && c.Value is JsonObject o && o.ContainsKey("dtgeContentTypeAlias")))
        {
            control.Editor.View = "/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html";
        }
    }
}

using MimeKit.Encodings;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

/// <summary>
///  migrator for macro grid config items 
/// </summary>
/// <remarks>
///  macro config isn't actually migrated here, because macros require the creation of new document types
///  the macro file upgrader will create these, during the datatype import everything is already imported 
///  as required. 
/// </remarks>
/// 
internal class GridConfigMacroMigrator: GridSettingsViewMigratorBase, IGridSettingsViewMigrator
{
    private readonly IContentTypeContainerService _contentTypeContainerService;
    private readonly IContentTypeService _contentTypeService;

    public GridConfigMacroMigrator(IContentTypeContainerService contentTypeContainerService, IContentTypeService contentTypeService)
    {
        _contentTypeContainerService = contentTypeContainerService;
        _contentTypeService = contentTypeService;
    }

    public string ViewAlias => "macro";
    public string? GetDataTypeAlias(string gridAlias, string? configItemLabel)
        => null;

    /// <summary>
    ///  fetch the macro gird blocks. 
    /// </summary>
    public override async Task<IEnumerable<UmbBlockGridTypeModel>> GetAdditionalGridBlocksAsync(string gridAlias, string blockLabel, Guid? groupKey)
    {
        var macroContentTypes = await GetMacroContentTypes(gridAlias, blockLabel);

        var result = new List<UmbBlockGridTypeModel>();

        foreach (var contentType in macroContentTypes)
        {
            result.Add(new UmbBlockGridTypeModel
            {
                ContentElementTypeKey = contentType.Key,
                Label = contentType.Name,
                AllowAtRoot = false,
                AllowInAreas = true,
                GroupKey = groupKey
            });
        }

        return result;
    }

    private async Task<IEnumerable<IContentType>>GetMacroContentTypes(string gridAlias, string blockLabel)
    {
        List<EntityContainer> macroContainer = [..await _contentTypeContainerService.GetAsync("Macros", 1)];
        if (macroContainer.Count == 0) return [];
        return _contentTypeService.GetChildren(macroContainer[0].Id);
    }

    public override async Task<IEnumerable<Guid>> GetAllowedElementKeysAsync(string elementAlias)
    {
        var macros = await GetMacroContentTypes(elementAlias, string.Empty);
        return macros.Select(x => x.Key);
    }
}

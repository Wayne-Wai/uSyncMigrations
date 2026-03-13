using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Grid.Helpers;

/// <summary>
///  manages getting grid bits from the content service. 
/// </summary>
/// <remarks>
///  we have seperated this off, so if needed we can do 
///  caching and other performance related things here. 
///  
///  at the moment we hit the 'database' layer everytime.
///  Umbraco is caching so it shouldn't be a big hit, but
///  if possible it would be good to avoid so many go arounds.
/// </remarks>
internal class SyncGridContentTypeFinder : ISyncGridContentTypeFinder
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ISyncGridNameService _gridNameHelper;

    public SyncGridContentTypeFinder(IContentTypeService contentTypeService, ISyncGridNameService gridNameHelper)
    {
        _contentTypeService = contentTypeService;
        _gridNameHelper = gridNameHelper;
    }

    public IContentType? FindTemplateContentType(string gridAlias, string? templateAlias)
    {
        var contentTypeAlias = _gridNameHelper.GetTemplateContentTypeAlias(gridAlias, templateAlias);
        return FindContentType(contentTypeAlias);
    }

    public Guid FindTemplateContentTypeKey(string gridAlias, string? templateAlias)
    {
        var contentTypeAlias = _gridNameHelper.GetTemplateContentTypeAlias(gridAlias, templateAlias);
        var item = FindContentType(contentTypeAlias);
        if (item == null)
            return contentTypeAlias.ToGuid();
        return item.Key;
    }

    public IContentType? FindLayoutContentType(string gridAlias, string layout)
    {
        var contentTypeAlias = _gridNameHelper.GetLayoutContentTypeAlias(gridAlias, layout);
        return FindContentType(contentTypeAlias);
    }

    public Guid? FindLayoutContentTypeKey(string gridAlias, string? layout)
    {
        var contentTypeAlias = _gridNameHelper.GetLayoutContentTypeAlias(gridAlias, layout);
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null)
            return contentTypeAlias.ToGuid();
        return item.Key;
    }


    public IContentType? FindSettingsContentType(string gridAlias, string? layout)
    {
        var contentTypeAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, layout);
        return FindContentType(contentTypeAlias);
    }

    public Guid? FindSettingsContentTypeKey(string gridAlias, string? layout)
    {
        var contentTypeAlias = _gridNameHelper.GetSettingsContentTypeAlias(gridAlias, layout);
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null)
            return contentTypeAlias.ToGuid();
        return item.Key;
    }

    public IContentType? FindElementContentType(string elementAlias)
    {
        var contentTypeAlias = _gridNameHelper.GetElementContentTypeAlias(elementAlias);
        return FindContentType(contentTypeAlias);
    }

    public Guid? FindElementContentTypeKey(string elementAlias)
    {
        var contentTypeAlias = _gridNameHelper.GetElementContentTypeAlias(elementAlias);
        var item = _contentTypeService.Get(contentTypeAlias);
        if (item == null) 
            return contentTypeAlias.ToGuid();
        return item.Key;
    }

    public IContentType? FindContentType(string alias)
        => _contentTypeService.Get(alias);

    public IEnumerable<IContentType> GetAllGridBlockContentTypes(Guid? groupKey)
        => _contentTypeService.GetAll()
            .Where(x => x.Alias.StartsWith("Grid_Element"));
}

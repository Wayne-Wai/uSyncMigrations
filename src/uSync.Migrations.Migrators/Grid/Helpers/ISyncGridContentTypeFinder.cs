using Umbraco.Cms.Core.Models;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Helpers;

internal interface ISyncGridContentTypeFinder
{
    IContentType? FindLayoutContentType(string gridAlias, string layout);
    Guid? FindLayoutContentTypeKey(string gridAlias, string? layout);
    IContentType? FindContentType(string alias);
    IContentType? FindElementContentType(string elementAlias);
    Guid? FindElementContentTypeKey(string elementAlias);
    IContentType? FindSettingsContentType(string gridAlias, string? layout);
    Guid? FindSettingsContentTypeKey(string gridAlias, string? layout);
    IContentType? FindTemplateContentType(string gridAlias, string? templateAlias);
    Guid FindTemplateContentTypeKey(string gridAlias, string? templateAlias);
    Task<IEnumerable<IContentType>> GetAllGridBlockContentTypesAsync(Guid? groupKey);
    Guid? FindContentTypeKey(string alias);
}
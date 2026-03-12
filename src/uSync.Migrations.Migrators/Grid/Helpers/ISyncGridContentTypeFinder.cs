using Umbraco.Cms.Core.Models;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Helpers;

internal interface ISyncGridContentTypeFinder
{
    Guid? FindContentContentTypeKey(string gridAlias, string layout);
    Guid? FindElementContentTypeKey(string elementAlias);
    Guid? FindSettingsContentTypeKey(string gridAlias, string layout);
    Guid FindTemplateContentTypeKey(string gridAlias, string templateAlias);
    IEnumerable<IContentType> GetAllGridBlockContentTypes(Guid? groupKey);
}
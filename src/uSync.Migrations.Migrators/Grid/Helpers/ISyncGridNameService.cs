namespace uSync.Migrations.Migrators.Grid.Helpers;

internal interface ISyncGridNameService
{
    string GetSettingsContentTypeAlias(string alias, string? label);
    string GetDataTypeAlias(string alias, string? label);
    string GetLayoutContentTypeAlias(string alias, string? label);
    string GetTemplateContentTypeAlias(string alias, string? label);

    /// <summary>
    ///  elements are created globaly, (not per grid).
    /// </summary>
    string GetElementContentTypeAlias(string alias);
    string MakeSafeConfig(string alias);
    string MakeSafeSettingsKey(string alias);
    Guid MakeAreaKey(string gridAlias, string alias, int index, int columns);
}
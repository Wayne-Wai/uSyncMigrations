using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Grid.Helpers;

internal class SyncGridNameService : ISyncGridNameService
{
    private readonly IShortStringHelper _shortStringHelper;
    public SyncGridNameService(IShortStringHelper shortStringHelper)
    {
        _shortStringHelper = shortStringHelper;
    }

    public string GetSettingsContentTypeAlias(string alias, string? label)
        => GetBaseGridAlias("Settings", alias, label);

    public string GetLayoutContentTypeAlias(string alias, string? label)
        => GetBaseGridAlias("Layout", alias, label);

    public string GetTemplateContentTypeAlias(string alias, string? label)
        => GetBaseGridAlias("Template", alias, label);

    public string GetElementContentTypeAlias(string alias)
        => GetBaseGridAlias("Element", alias, null);

    public string GetDataTypeAlias(string alias, string? label)
        => GetBaseGridAlias("DataType", alias, label);

    private string GetBaseGridAlias(string prefix, string alias, string? label)
    {
        var newAlias = $"Grid_{prefix}_{alias}";
        if (string.IsNullOrWhiteSpace(label) is false)
        {
            newAlias += $"_{label}";
        }
        return newAlias.ToSafeAlias(_shortStringHelper);
    }

    public string MakeSafeConfig(string alias)
        => $"{alias.ToSafeAlias(_shortStringHelper)}.config";
}

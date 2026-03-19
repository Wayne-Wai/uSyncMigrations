using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

using uSync.Core.Extensions;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

internal class MacroBlockMigrator(IContentTypeService contentTypeService)
    : SyncBlockMigratorBase, ISyncBlockMigrator
{
    /// <inheritdoc />
    public string[] Aliases => ["macro"];

    /// <inheritdoc />
    public Dictionary<string, object> GetPropertyValues(GridValue.GridControl control)
    {
        if (TryGetMacroGridProperty(control, out var value) is false)
            return [];

        if (value.TryGetPropertyAsObject("macroParamsDictionary", out var parameters) && parameters is not null)
        {
            var parametersDict = new Dictionary<string, object>();
            foreach (var item in parameters)
            {
                if (item.Value is null) continue;
                parametersDict[item.Key] = item.Value;
            }

            return parametersDict;
        }

        return [];
    }

    /// <inheritdoc/>
    public override IEnumerable<BlockItemData> GetPropertyContentBlocks(GridValue.GridControl control)
    {
        if (TryGetMacroGridProperty(control, out var value) is false)
            return [];

        var macroAlias = value.GetPropertyAsString("macroAlias");
        if (string.IsNullOrWhiteSpace(macroAlias)) return [];

        var contentTypeKey = contentTypeService.Get(macroAlias)?.Key ?? Guid.Empty;
        if (contentTypeKey == Guid.Empty) return [];

        return [GridMigratorHelpers.CreateBlockItemData(Guid.NewGuid(), contentTypeKey)];
    }

    public override string? GetContentTypeAlias(GridValue.GridControl control)
    {
        if (TryGetMacroGridProperty(control, out var value) is false)
            return null;

        var macroAlias = value.GetPropertyAsString("macroAlias");
        if (string.IsNullOrWhiteSpace(macroAlias))
            return null;

        return macroAlias;
    }

    /// <summary>
    ///  Try and get the Json object for the macro settings in the grid. 
    /// </summary>
    /// <param name="control">The grid control value</param>
    /// <param name="value">the resulting json of the value</param>
    /// <returns>true if the property is a macro property.</returns>
    private static bool TryGetMacroGridProperty(GridValue.GridControl control, [NotNullWhen(true)] out JsonObject? value)
    {
        value = null;
        if (control.Value is null
            || control.Value.TryConvertToJsonObject(out var json) is false
            || json.ContainsKey("macroAlias") is false)
            return false;

        value = json;
        return true;
    }
}
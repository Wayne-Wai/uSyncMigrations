using Umbraco.Cms.Core.Models.Blocks;

using uSync.Migrations.Migrators.Grid.Extensions;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content;

internal static class GridMigratorHelpers
{

    public static BlockGridValue CreateBlockGrid(Guid key, Guid contentTypeKey)
        => new()
        {
            ContentData = [new BlockItemData
            {
                Key = key,
                ContentTypeKey = contentTypeKey
            }]
        };

    public static BlockGridLayoutItem CreateBlockLayoutItem(Guid contentKey, Guid? settingsKey, int gridColumns)
        => new()
        {
            ContentKey = contentKey,
            SettingsKey = settingsKey,
            ColumnSpan = gridColumns,
            RowSpan = 1
        };

    public static BlockGridLayoutItem CreateBlockLayoutItem(Guid contentKey, Guid? settingsKey, IEnumerable<BlockGridLayoutAreaItem> areas, int columnSpan)   
        => new()
        {
            ContentKey = contentKey,
            SettingsKey = settingsKey,
            ColumnSpan = columnSpan,
            Areas = [.. areas],
            RowSpan = 1
        };

    public static BlockGridLayoutAreaItem CreateBlockLayoutAreaItem(Guid key, IEnumerable<BlockGridLayoutItem> items)
        => new()
        {
            Key = key,
            Items = [.. items]
        };

    public static IEnumerable<BlockGridLayoutItem> GetLayoutBlocksFromGridArea(GridValue.GridArea area, List<BlockItemData> content)
    {
        foreach (var item in content)
        {
            yield return GridMigratorHelpers.CreateBlockLayoutItem(item.Key, null, area.Grid ?? 0);
        }
    }

    public static BlockPropertyValue CreateBlockPropertyValue(string alias, string value)
        => new()
        {
            Alias = alias,
            Value = value
        };

    public static BlockItemData CreateBlockItemData(Guid key, Guid contentTypeKey)
        => new()
        {
            Key = key,
            ContentTypeKey = contentTypeKey,
        };

}
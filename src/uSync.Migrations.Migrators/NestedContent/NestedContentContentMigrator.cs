using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Core.Serialization;

using static uSync.Migrations.Core.SyncMigrations;

namespace uSync.Migrations.Migrators.NestedContent;

/// <summary>
///  handles content use to be nestedContent, puts it into a blockList. 
/// </summary>
internal class NestedContentContentMigrator : SyncValueMapperBase, ISyncMapper
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<NestedContentContentMigrator> _logger;

    public NestedContentContentMigrator(IEntityService entityService, IContentTypeService contentService, ILogger<NestedContentContentMigrator> logger)
        : base(entityService)
    {
        _contentTypeService = contentService;
        _logger = logger;
    }

    public override string Name => nameof(NestedContentContentMigrator);
    public override string[] Editors => [SyncLegacyTypes.NestedContent, SyncLegacyTypes.OurNestedContent];

    private static string[] _reservedProperties = ["ncContentTypeAlias", "key", "name"];

    public override Task<string?> GetImportValueAsync(string value, string editorAlias, SyncSerializerOptions options)
    {
        // migrators are resposible for checking if the migration has already happend
        if (value.Contains("ncContentTypeAlias") is false)
            return Task.FromResult<string?>(value);


        if (TryGetNestedContent(value, out var nestedContent) is false || nestedContent == null)
        {
            _logger.LogWarning("MIGRATION WARNING: Value for editor {EditorAlias} contains 'ncContentTypeAlias' but could not be deserialized. Value: {Value}", editorAlias, value);
            return Task.FromResult<string?>(value);
        }

        BlockListValue blockListValue = new BlockListValue();

        foreach (var item in nestedContent)
        {
            var contentTypeAlias = item.TryGetValue("ncContentTypeAlias", out var alias) ? alias?.ToString() : null;
            if (contentTypeAlias == null)
            {
                _logger.LogWarning("MIGRATION WARNING: Could not find 'ncContentTypeAlias' for Nested Content item. Skipping item. Value: {Value}", value);
                continue;
            }

            var contentType = _contentTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                _logger.LogWarning("MIGRATION WARNING: Could not find content type with alias {ContentTypeAlias} for Nested Content item. Skipping item. Value: {Value}", contentTypeAlias, value);
                continue;
            }

            var blockItemData = new BlockItemData
            {
                ContentTypeKey = contentType.Key,
                Key = item.TryGetValue("key", out var key) && Guid.TryParse(key?.ToString(), out var keyGuid) ? keyGuid : Guid.NewGuid(),
            };

            foreach (var propertyValue in item.Keys)
            {
                if (_reservedProperties.Contains(propertyValue)) continue;
                blockItemData.Values.Add(new BlockPropertyValue
                {
                    Alias = propertyValue,
                    Value = item[propertyValue]
                });
            }

            blockListValue.ContentData.Add(blockItemData);
        }

        blockListValue.Expose = [.. blockListValue.ContentData.Select(x => new BlockItemVariation(x.Key, null, null))];
        blockListValue.Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
        {
            {
                "Umbraco.BlockList",
                blockListValue.ContentData.Select(x => new BlockListLayoutItem
                {
                    ContentKey = x.Key,
                    SettingsKey = null,
                })
            }
        };

        return Task.FromResult<string?>(blockListValue.SerializeJsonString() ?? value);
    }

    private static bool TryGetNestedContent(string value, out List<Dictionary<string, object>>? nestedContent)
    {
        nestedContent = null;
        if (value.Contains("ncContentTypeAlias") is false)
            return false;
        try
        {
            nestedContent = value.DeserializeJson<List<Dictionary<string, object>>>();
            return true;
        }
        catch (Exception)
        {
            // If deserialization fails (e.g. malformed or partially corrupted JSON),
            // fall back to returning the original value.
            return false;
        }
    }
}

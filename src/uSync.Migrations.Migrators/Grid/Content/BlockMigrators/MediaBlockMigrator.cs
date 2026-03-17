using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;

using uSync.Core.Extensions;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

internal class MediaBlockMigrator(IMediaService mediaService) : SyncBlockMigratorBase, ISyncBlockMigrator
{
    public string[] Aliases => ["media"];
    public Dictionary<string, object> GetPropertyValues(GridValue.GridControl control)
    {
        if (control.Value is null)
            return [];

        if (control.Value.TryConvertToJsonObject(out var value) is false)
            return [];


        Guid mediaKeyGuid = Guid.Empty;

        if (value.ContainsKey("udi")) {
            var udiValue = value.GetPropertyAsString("udi");
            if (UdiParser.TryParse(udiValue, out Udi? udi) && udi is GuidUdi guidUdi)
            {
                mediaKeyGuid = guidUdi.Guid;
            }
        }
        else if (value.ContainsKey("id"))
        {
            var intValue = value.GetPropertyAsString("id");
            if (int.TryParse(intValue, out var id))
            {
                var mediaItem = mediaService.GetById(id);
                if (mediaItem != null)
                    mediaKeyGuid = mediaItem.Key;
            }                           
        }

        List<MediaWithCrops> media = [new MediaWithCrops
        {
           Key = Guid.NewGuid(),
           MediaKey = mediaKeyGuid 
        }];

        return new Dictionary<string, object>
        {
            { control.Editor.Alias, media.SerializeJsonString(true) }
        };
    }

    private class MediaWithCrops
    {
        public Guid Key { get; set; }
        public Guid MediaKey { get; set; }
        public string MediaTypeAlias { get; set; } = string.Empty;
        public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }
        public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }
    }
}
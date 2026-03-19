using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Migrations.Migrators.GibeLinkPicker.Models;

namespace uSync.Migrations.Migrators.GibeLinkPicker;

internal class GibeLinkPickerContentMigrator : SyncValueMapperBase, ISyncMapper
{
    public GibeLinkPickerContentMigrator(IEntityService entityService)
        : base(entityService)
    { }

    public override string Name => nameof(GibeLinkPickerContentMigrator);

    public override string[] Editors => ["Gibe.LinkPicker"];

    public override Task<string?> GetImportValueAsync(string value, string editorAlias)
    {
        // TODO: Migrators are responsible to checking if the migration has already happened.
        if (string.IsNullOrWhiteSpace(value)) return Task.FromResult<string?>(value);

        List<GibeLinkPickerData> gibeLinkPickers = [.. GetPickerValues(value)];

        if (gibeLinkPickers.Count == 0)
            return Task.FromResult<string?>(value);

        var links = new List<MultiUrlPickerValueEditor.LinkDto>();

        foreach (var picker in gibeLinkPickers)
        {
            var link = new MultiUrlPickerValueEditor.LinkDto
            {
                Name = picker?.Name ?? string.Empty,
                Url = picker?.Url ?? string.Empty,
                Udi = picker?.Uid != null ? new GuidUdi(Constants.UdiEntityType.Document, Guid.Parse(picker.Uid)) : null,
            };

            if (picker?.Target == "_blank")
            {
                link.Target = picker.Target;
            }

            links.Add(link);
        }

        return Task.FromResult<string?>(links.SerializeJsonString() ?? value);
    }

    private static IEnumerable<GibeLinkPickerData> GetPickerValues(string? contentValue)
    {
        if (contentValue == null) return [];

        if (contentValue.StartsWith('['))
            return contentValue.DeserializeJson<IEnumerable<GibeLinkPickerData>>() ?? [];

        if (contentValue.TryDeserialize<GibeLinkPickerData>(out var singleLink) && singleLink is not null)
            return [singleLink];

        return [];

    }
}

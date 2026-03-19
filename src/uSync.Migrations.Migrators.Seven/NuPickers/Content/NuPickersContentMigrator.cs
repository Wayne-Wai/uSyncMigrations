using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core.Extensions;
using uSync.Core.Mapping;

namespace uSync.Migrations.Migrators.NuPickers.Content;

internal class NuPickersContentMigrator : SyncValueMapperBase, ISyncMapper, ISyncPropertyMapper
{
    public NuPickersContentMigrator(IEntityService entityService) : base(entityService)
    {
    }

    public override string Name => nameof(NuPickersContentMigrator);

    public override string[] Editors => [
        "nuPickers.SqlDropdownPicker",
        "nuPickers.EnumDropDownPicker",
        "nuPickers.EnumCheckBoxPicker",
        "DotNetTypeaheadListPicker"
    ];

    public Task<string?> GetImportValueAsync(string value, IPropertyType propertyType)
    {
        if (string.IsNullOrWhiteSpace(value) || value.IsValidJsonString() is false)
            return Task.FromResult<string?>(value);


        if (value.TryParseToJsonObject(out var jsonObject) is false)
            return Task.FromResult<string?>(value);


        return Task.FromResult<string?>(
            jsonObject.Select(x => x.Value?.ToString()).WhereNotNull().SerializeJsonString());
    }
}

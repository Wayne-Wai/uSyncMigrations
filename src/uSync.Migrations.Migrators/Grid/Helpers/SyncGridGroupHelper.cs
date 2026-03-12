using Umbraco.Extensions;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Helpers;

internal class SyncGridGroupHelper
{
    public const string TemplateGroupName = "Templates";
    public const string ElementGroupName = "Elements";
    public const string LayoutGroupname = "Layouts";

    private readonly int _hashValue;

    public UmbBlockGridTypeGroupType[] Groups { get; private set; }

    public SyncGridGroupHelper(int hashValue)
    {
        _hashValue = hashValue;
        Groups = CreateGridBlockGroups();
    }

    public Guid? GetTemplateGroupKey()
        => GetGroupKey(TemplateGroupName);

    public Guid? GetElementGroupKey()
        => GetGroupKey(ElementGroupName);

    public Guid? GetLayoutGroupKey()
        => GetGroupKey(LayoutGroupname);

    public Guid? GetGroupKey(string name)
        => GetGroupByName(name)?.Key;

    private UmbBlockGridTypeGroupType[] CreateGridBlockGroups()
    {
        return [
            new UmbBlockGridTypeGroupType
            {
                Name = TemplateGroupName,
                Key = $"templates_{_hashValue}".ToGuid()
            },
            new UmbBlockGridTypeGroupType
            {
                Name = LayoutGroupname,
                Key = $"layouts_{_hashValue}".ToGuid()
            },
            new UmbBlockGridTypeGroupType
            {
                Name = ElementGroupName,
                Key = $"elements_{_hashValue}".ToGuid()
            }
        ];
    }

    private UmbBlockGridTypeGroupType? GetGroupByName(string name)
        => Groups.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

using System.Xml.Linq;

using Umbraco.Extensions;

namespace uSync.Migrations.Core.Extensions;

public record SyncDataTypeInfo(string Name, string Alias, Guid Definition, string PropertyType, string PropertyAlias);
public record SyncCompositionInfo(Guid Key, string Alias);

public static class SyncMigrationContentTypeHelper
{
    public static XElement CreateContentType(string name, string alias, string folder, string icon, string description,
        SyncCompositionInfo[] compositions,
        SyncDataTypeInfo[] dataTypes)
        => CreateContentType(name, alias.ToGuid(), alias, folder, icon, description, compositions, dataTypes);

    public static XElement CreateContentType(string name, Guid key, string alias, string folder, string icon, string description,
        SyncCompositionInfo[] compositions,
        SyncDataTypeInfo[] dataTypes)
    {
        var node = new XElement("ContentType",
            new XAttribute("Key", key),
            new XAttribute("Alias", alias),
            new XAttribute("Level", 2),
            new XElement("Info",
                new XElement("Name", name),
                new XElement("Icon", icon ?? "icon-document"),
                new XElement("Thumbnail", "folder.png"),
                new XElement("Description", new XCData(description)),
                new XElement("AllowAtRoot", "false"),
                new XElement("ListView", Guid.Empty),
                new XElement("Variation", "Nothing"),
                new XElement("IsElement", "true"),
                new XElement("HistoryCleanup",
                    new XElement("PreventCleanup", false),
                    new XElement("KeepAllVersionsNewerThanDays", string.Empty),
                    new XElement("KeepLatestVersionPerDayForDays", string.Empty)
                ),
                new XElement("Folder", folder),
                GetCompositions(compositions),
                new XElement("DefaultTemplate", string.Empty),
                new XElement("AllowedTemplates")
            ),
            new XElement("Structure")
        );


        if (dataTypes.Length == 0) return node;

        var properties = new XElement("GenericProperties");

        foreach (var dataType in dataTypes)
        {
            properties.Add(
                new XElement("GenericProperty",
                    new XElement("Key", $"{alias}{dataType.Alias}".ToGuid()),
                    new XElement("Name", dataType.Name),
                    new XElement("Alias", dataType.PropertyAlias),
                    new XElement("Definition", dataType.Definition),
                    new XElement("Type", dataType.PropertyType),
                    new XElement("Mandatory", "false"),
                    new XElement("Validation", string.Empty),
                    new XElement("Description", new XCData(description)),
                    new XElement("SortOrder", "0"),
                    new XElement("Tab", "Content",
                        new XAttribute("Alias", "content")),
                    new XElement("Variations", "Nothing"),
                    new XElement("MandatoryMessage", string.Empty),
                    new XElement("ValidationRegEx", string.Empty),
                    new XElement("LabelOnTop", "false")
                ));
        }

        node.Add(properties);

        node.Add(new XElement("Tabs",
            new XElement("Tab",
                new XElement("Key", $"{alias}_Content".ToGuid()),
                new XElement("Caption", "Content"),
                new XElement("Alias", "content"),
                new XElement("Type", "Tab"),
                new XElement("SortOrder", "0")
            )
        ));

        return node;
    }

    private static XElement? GetCompositions(SyncCompositionInfo[] compositions)
    {
        var node = new XElement("Compositions");
        if (compositions.Length == 0) return node;

        foreach (var composition in compositions)
        {
            node.Add(new XElement("Composition",
                new XAttribute("Key", composition.Key),
                new XAttribute("Alias", composition.Alias)));
        }
        return node;
    }
}

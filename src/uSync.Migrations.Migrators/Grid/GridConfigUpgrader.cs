using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core.Extensions;
using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid;

/// <summary>
///  a special upgrader that handles the grid.editors.config.js file. 
/// </summary>
/// <remarks>
///  the grid.editors.config.js file contains grid editors that will not 
///  exist inside a modern version of umbraco, so this upgrader will
///  need to create the content types for these grid editors so 
///  when the upgrade runs we can refernece them 
/// </remarks>

internal class GridConfigUpgrader : ISyncFileUpgrader
{
    public string ItemType => "grid.editors.config.js";

    private readonly IShortStringHelper _shortStringHelper;

    public GridConfigUpgrader(IShortStringHelper shortStringHelper)
    {
        _shortStringHelper = shortStringHelper;
    }

    public async Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        if (file.Content is null) return [];

        if (file.Content.TryDeserialize<GridEditor[]>(out var config) is false || config is null) return [];

        var newContentTypes = new List<SyncUpgradeFile>();

        foreach (var editor in config)
        {
            if (editor is null) continue;

            var editorAlias = GetEditorAliasFromConfig(editor);
            var propertyType = GetPropertyTypeFromConfig(editor);
            var dataTypeNode = CreateDataType(editorAlias, propertyType, "{}");

             newContentTypes.Add(new SyncUpgradeFile
             {
                 Filename = $"DataTypes/grid/grid.editor.{editor.Alias}.datatype.config", 
                 Node = dataTypeNode 
             });

            var contentTypeNode = CreateContentType(
                editor.Name ?? editor.Alias ?? "gridEditor",
                editor.Alias ?? editor.Name ?? "gridEditor",
                editor.Icon,
                editorAlias,
                propertyType);

            newContentTypes.Add(new SyncUpgradeFile
            {
                Filename = $"ContentTypes/grid/grid.editor.{editor.Alias}.contenttype.config", 
                Node = contentTypeNode 
            });    
        }

        return newContentTypes;

    }

    private XElement CreateDataType(string name, string propertyType, string config)
    {
        return new XElement("DataType",
            new XAttribute("Key", name.ToGuid()),
            new XAttribute("Alias", name),
            new XAttribute("Level", 2),
            new XElement("Info",
                new XElement("Name", name),
                new XElement("EditorAlias", propertyType),
                new XElement("Folder", "Grid+Editors")),
            new XElement("Config", new XCData(config))
        );
    }

    private XElement CreateContentType(string name, string alias, string? icon, string dataTypeName,
        string propertyType)
    {
        return new XElement("ContentType",
            new XAttribute("Key", alias.ToGuid()),
            new XAttribute("Alias", alias),
            new XAttribute("Level", 2),
            new XElement("Info",
                new XElement("Name", name),
                new XElement("Icon", icon ?? "icon-document"),
                new XElement("Thumbnail", "folder.png"),
                new XElement("Description", new XCData($"migrated from grid configuration, editor alias: {alias}")),
                new XElement("AllowAtRoot", "false"),
                new XElement("ListView", Guid.Empty),
                new XElement("Variation", "Nothing"),
                new XElement("IsElement", "true"),
                new XElement("HistoryCleanup",
                    new XElement("PreventCleanup", false),
                    new XElement("KeepAllVersionsNewerThanDays", string.Empty),
                    new XElement("KeepLatestVersionPerDayForDays", string.Empty)
                ),
                new XElement("Folder", "Grid+Editors"),
                new XElement("Compositions"),
                new XElement("DefaultTemplate", string.Empty),
                new XElement("AllowedTemplates")
            ),
            new XElement("Structure"),
            new XElement("GenericProperties",
                new XElement("GenericProperty",
                    new XElement("Key", $"{alias}_{dataTypeName}".ToGuid()),
                    new XElement("Name", name),
                    new XElement("Alias", alias.ToSafeAlias(_shortStringHelper)),
                    new XElement("Definition", dataTypeName.ToGuid()),
                    new XElement("Type", propertyType),
                    new XElement("Mandatory", "false"),
                    new XElement("Validation", string.Empty),
                    new XElement("Description", new XCData("migrated from grid configuration")),
                    new XElement("SortOrder", "0"),
                    new XElement("Tab", "Content", 
                        new XAttribute("Alias", "content")),
                    new XElement("Variations", "Nothing"),
                    new XElement("MandatoryMessage", string.Empty),
                    new XElement("ValidationRegEx", string.Empty),
                    new XElement("LabelOnTop", "false")
                )
            ),
            new XElement("Tabs",
                new XElement("Tab",
                    new XElement("Key", $"{alias}_Content".ToGuid()),
                    new XElement("Caption", "Content"),
                    new XElement("Alias", "content"),
                    new XElement("Type", "Tab"),
                    new XElement("SortOrder", "0")
                )
            )
        );
    }

    private string GetEditorAliasFromConfig(GridEditor editor)
    {
        var alias = string.IsNullOrEmpty(editor.Alias)
            ? Path.GetFileNameWithoutExtension(editor.View) ?? editor.Alias : editor.Alias;

        return "gridEditor." + alias;
    }

    private string GetPropertyTypeFromConfig(GridEditor editor)
    {
        var view = editor.View?.ToLower() ?? editor.Alias?.ToLower() ?? string.Empty;

        return view switch
        {
            "rte" or "richtext" => Constants.PropertyEditors.Aliases.RichText,
            "dropdown" or "dropdownlist" => Constants.PropertyEditors.Aliases.DropDownListFlexible,
            "image" or "media" => Constants.PropertyEditors.Aliases.MediaPicker3,
            "checkbox" => Constants.PropertyEditors.Aliases.CheckBoxList,
            "datepicker" => Constants.PropertyEditors.Aliases.DateTime,
            "numeric" => Constants.PropertyEditors.Aliases.Integer,
            _ => Constants.PropertyEditors.Aliases.TextBox,
        };
    }
}

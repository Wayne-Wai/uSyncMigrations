using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.TinyMce, typeof(RichTextConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.TinyMCEv3")]
[SyncMigratorVersion(7, 8)]
public class RichTextBoxMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        if (dataTypeProperty.PreValues?.Count > 0)
        {
            var config = new RichTextConfiguration().MapPreValues(dataTypeProperty.PreValues) as RichTextConfiguration;

            if (config is not null)
            {
                if (config.Editor is JObject editor)
                {
                    var toolbar = editor["toolbar"] as JArray;
                    if (toolbar?.Count > 0)
                    {
                        var replacements = new Dictionary<string, string>
                        {
                            { "code", "ace" },
                            { "styleselect", "styles" },
                        };

                        foreach (var replacement in replacements)
                        {
                            var idx = toolbar.FindIndex(x => replacement.Key.Equals(x.ToString()) == true);
                            if (idx >= 0)
                            {
                                toolbar.RemoveAt(idx);
                                toolbar.Insert(idx, replacement.Value);
                            }
                        }
                    }

                    var stylesheets = editor["stylesheets"] as JArray;
                    if (stylesheets?.Count > 0)
                    {
                        for (int i = 0; i < stylesheets.Count; i++)
                        {
                            stylesheets[i].Replace($"/css/{stylesheets[i]}.css");
                        }
                    }

                    if (editor["mode"] is null)
                    {
                        editor["mode"] = "classic";
                    }
                }

                if (config.OverlaySize is null)
                {
                    config.OverlaySize = "small";
                }
            }

            return config;
        }

        return base.GetConfigValues(dataTypeProperty, context);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var richTextValue = string.Empty;

        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
        {
            richTextValue = GuidExtensions.LocalLink2Udi(contentProperty.Value);

            // Fix legacy inline macro format
            var doc = new HtmlDocument();
            doc.LoadHtml(richTextValue);
            var wrappedMacros = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' umb-macro-holder ') and contains(concat(' ', normalize-space(@class), ' '), ' mceNonEditable ')]");
            if (wrappedMacros is not null)
            {
                foreach (var wrappedMacro in wrappedMacros)
                {
                    // Find macro inside the comment
                    var macroCode = wrappedMacro.ChildNodes.FirstOrDefault(x => x.NodeType == HtmlNodeType.Comment)?.InnerHtml;
                    macroCode = macroCode?.Replace("<!--", "").Replace("-->", "");

                    if (macroCode is null)
                    {
                        continue;
                    }

                    // Replace the div with the raw macro code (using string replacement here as the HTML is not strictly valid!)
                    richTextValue = richTextValue.ReplaceFirst(wrappedMacro.OuterHtml, macroCode);
                }
            }
        }

        return richTextValue;
    }
}
using System.Text.Json.Nodes;

using Umbraco.Extensions;

using uSync.Core.Extensions;

namespace uSync.Migrations.Migrators.Grid.Models;

/// <summary>
///  the grid models are now not included in umbraco, so we have had to make our own here. 
/// </summary>
/// 

internal class GridConfiguration
{
    public GridConfigurationItems? Items { get; set; }
    public GridRteConfiguration? Rte { get; set; }
    public bool IgnoreUserStartNodes { get; set; }
    public string? MediaParentId { get; set; }
}

internal class GridConfigurationItems
{
    public List<GridConfigurationConfig>? Styles { get; set; }
    public List<GridConfigurationConfig>? Config { get; set; }
    public int Columns { get; set; }
    public List<GridTemplate>? Templates { get; set; }
    public List<GridLayout>? Layouts { get; set; }
}

public class GridConfigurationConfig
{
    public string? Label { get; set; }
    public string? Description { get; set; }
    public string? Key { get; set; }
    public string? View { get; set; }
    public string? Modifier { get; set; }
    public JsonNode? ApplyTo { get; set; }
    public List<GridSettingsConfigurationItemPreValue>? PreValues { get; set; }

    public bool AppliesTo(string alias)
    {
        var appliesToValue = GetAppliesToValue();
        if (appliesToValue == SyncGridMigrations.ApplyTo.ApplyToAll) return true;
        if (appliesToValue.Equals(alias, StringComparison.InvariantCultureIgnoreCase)) return true;
        return false;
    }

    public string GetAppliesToValue()
    {
        if (ApplyTo is null) return SyncGridMigrations.ApplyTo.ApplyToAll;

        if (ApplyTo.TryConvertToJsonObject(out var applyToObject) is false)
            return ApplyTo.ToString();

        if (applyToObject.ContainsKey("row"))
        {
            var row = applyToObject.GetValueAsString("row");
            if (string.IsNullOrWhiteSpace(row) is false)
                return row;
        }


        if (applyToObject.ContainsKey("Area"))
        {
            var area = applyToObject.GetValueAsString("Area");
            if (string.IsNullOrWhiteSpace(area) is false)
                return area;
        }

        return applyToObject.AsValue().ToString() ?? SyncGridMigrations.ApplyTo.ApplyToAll;
    }
}

public class GridSettingsConfigurationItemPreValue
{
    public string? Label { get; set; }
    public string? Value { get; set; }
}


internal class GridTemplate
{
    public required string Name { get; set; }
    public List<GridTemplateSection>? Sections { get; set; }
}

internal class GridTemplateSection
{
    public int Grid { get; set; }
    public bool AllowAll { get; set; }
    public List<string>? Allowed { get; set; }
}

internal class GridLayout
{
    public required string Name { get; set; }
    public string? Label { get; set; }
    public List<GridLayoutArea>? Areas { get; set; }
}

internal class GridLayoutArea
{
    public int Grid { get; set; }
    public List<string>? Editors { get; set; }
    public bool AllowAll { get; set; }
    public List<string>? Allowed { get; set; }
}

internal class GridRteConfiguration
{
    public List<string>? Toolbar { get; set; }
    public List<string>? Stylesheets { get; set; }
    public int MaxImageSize { get; set; }
    public string? Mode { get; set; }
    public GridRteImageDimensions? Dimensions { get; set; }
}

internal class GridRteImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
}

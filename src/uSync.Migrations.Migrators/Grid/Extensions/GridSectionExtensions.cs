using Microsoft.OpenApi;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Umbraco.Cms.Core.Models;

using uSync.Core.Extensions;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Extensions;

internal static class GridSectionExtensions
{
    public static int GetIntOrDefault(this string? value, int defaultValue)
        => int.TryParse(value, out var intValue) ? intValue : defaultValue;

    /// <summary>
    ///  the string is probably a block grid if it contains the string "Umbraco.BlockGrid" -
    ///  this is not a guarantee but it's a good indicator that the string is a block grid configuration
    /// </summary>
    public static bool IsProbiblyAlmostCertainlyBlockGrid(this string value)
        => value.Contains("\"Umbraco.BlockGrid\"");

    public static JsonSerializerOptions _jsonSerializerOptions = new()
    {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
    

    public static bool TryGetGridValue(this string value, [NotNullWhen(true)] out GridValue? gridValue)
    {
        try
        {
            gridValue = JsonSerializer.Deserialize<GridValue>(value, _jsonSerializerOptions);
            return gridValue is not null;
        }
        catch (Exception ex)
        {
            var error = ex.Message;
            gridValue = null;
            return false;
        }
    }
}

internal static class ContentTypeExtensions
{
    public static IPropertyType? GetPropertyType(this IContentType contentType, string propertyAlias)
        => contentType.PropertyTypes.FirstOrDefault(x => x.Alias.Equals(propertyAlias, StringComparison.InvariantCultureIgnoreCase));
}
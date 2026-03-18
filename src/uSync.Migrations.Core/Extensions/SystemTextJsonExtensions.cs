using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Umbraco.Extensions;

namespace uSync.Migrations.Core.Extensions;

public static class SystemTextJsonExtensions
{
    public static TObject? ConvertFromDictionary<TObject>(this IDictionary<string, object> dictionary)
        => JsonSerializer.Deserialize<TObject>(JsonSerializer.Serialize(dictionary));

    public static IDictionary<string, object>? ConvertToDictionary<TObject>(this TObject obj)
        => JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(obj));

    public static bool TryGetValueAsObject<TObject>(this IDictionary<string, object> dictionary, string key, 
        [NotNullWhen(true)] out TObject? result)
    {
        result = default;

        if (dictionary.TryGetValue(key, out var value) is false)
            return false;

        var attempt = value.TryConvertTo<TObject>();
        if (attempt.Success && attempt.Result is not null)
        {
            result = attempt.Result;
            return true;
        }

        return false;
    }
}

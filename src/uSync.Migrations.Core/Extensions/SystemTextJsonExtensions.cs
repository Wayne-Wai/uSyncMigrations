using System.Diagnostics.CodeAnalysis;

using Umbraco.Extensions;

using uSync.Core.Extensions;

namespace uSync.Migrations.Core.Extensions;

public static class SystemTextJsonExtensions
{
    public static TObject? ConvertFromDictionary<TObject>(this IDictionary<string, object> dictionary)
        => dictionary.SerializeJsonString().DeserializeJson<TObject>();

    public static IDictionary<string, object>? ConvertToDictionary<TObject>(this TObject obj)
        => obj?.SerializeJsonString().DeserializeJson<Dictionary<string, object>>() ?? [];

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

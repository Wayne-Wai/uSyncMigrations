using System.Text.Json.Serialization;

namespace uSync.Migrations.Core.Upgrade;

[JsonConverter(typeof(JsonStringEnumConverter<SyncUpgradeStatus>))]
public enum SyncUpgradeStatus
{
    Info,
    Success,
    Warning,
    Error
}
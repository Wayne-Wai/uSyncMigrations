using uSync.Migrations.Core.Persistance;

namespace uSync.Migrations.Core.Tracking;

public interface ISyncMigrationTrackingService : ISyncDataService<SyncMigratedData, string>
{
    Task AddRename(string newKey, string oldKey, string? additionalData);
    Task<string> GetOriginalKeyAsync(string key);
}
using uSync.Migrations.Core.Persistence;

namespace uSync.Migrations.Core.Tracking;

public interface ISyncMigrationTrackingService : ISyncDataService<SyncMigratedData, string>
{
    Task AddRenameAsync(string newKey, string oldKey, string? additionalData);
    Task<string> GetOriginalKeyAsync(string key);
}
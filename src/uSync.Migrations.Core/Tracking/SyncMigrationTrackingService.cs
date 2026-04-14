using Umbraco.Cms.Core.Scoping;

using uSync.Migrations.Core.Persistence;

namespace uSync.Migrations.Core.Tracking;

internal class SyncMigrationTrackingService : SyncDataServiceBase<SyncMigratedData, string>,
    ISyncMigrationTrackingService
{
    public SyncMigrationTrackingService(
        ISyncMigratedDataRepository migratedRepository,
        ICoreScopeProvider scopeProvider) : base(migratedRepository, scopeProvider)
    { }

    /// <summary>
    ///  tells us if this property has had it's id migrated, 
    /// </summary>
    public async Task<string> GetOriginalKeyAsync(string key)
    {
        var item = await GetAsync(key);
        return item?.Original ?? key;
    }

    public async Task AddRenameAsync(string newKey, string oldKey, string? additionalData)
    {
        var item = new SyncMigratedData
        {
            Key = newKey,
            Original = oldKey,
            AdditionalData = additionalData
        };
        await SaveAsync(item);
    }
}
  

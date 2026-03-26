using Umbraco.Cms.Core.Scoping;

using uSync.Migrations.Core.Persistance;

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
        return item?.Orginal ?? key;
    }

    public async Task AddRename(string newKey, string oldKey, string? additionalData)
    {
        var item = new SyncMigratedData
        {
            Key = newKey,
            Orginal = oldKey,
            AdditionalData = additionalData
        };
        await SaveAsync(item);
    }
}
  

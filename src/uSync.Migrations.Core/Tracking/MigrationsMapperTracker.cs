using uSync.Core.Mapping;
using uSync.Core.Mapping.Tracking;

namespace uSync.Migrations.Core.Tracking;

public class MigrationsMapperTracker : ISyncMapperTracker
{
    private readonly ISyncMigrationTrackingService _trackingService;
    private readonly SyncValueMapperCollection _syncValueMapperCollection;

    public MigrationsMapperTracker(
        ISyncMigrationTrackingService trackingService,
        SyncValueMapperCollection syncValueMapperCollection)
    {
        _trackingService = trackingService;
        _syncValueMapperCollection = syncValueMapperCollection;
    }

    public async Task<IEnumerable<ISyncMapper>> GetTrackingMappers(string editorAlias)
    {
        var trackedItem = await _trackingService.GetAsync(editorAlias);
        if (trackedItem is null) return [];
        return _syncValueMapperCollection.GetSyncMappers(trackedItem.Orginal);
    }
}

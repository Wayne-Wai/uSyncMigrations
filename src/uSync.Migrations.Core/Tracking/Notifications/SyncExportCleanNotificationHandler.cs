using Umbraco.Cms.Core.Events;

using uSync.Core.Notifications;

namespace uSync.Migrations.Core.Tracking.Notifications;


internal class SyncExportCleanNotificationHandler : INotificationAsyncHandler<SyncExportCleanNotification>
{
    private readonly ISyncMigrationTrackingService _syncMigratedDataService;

    public SyncExportCleanNotificationHandler(ISyncMigrationTrackingService syncMigratedDataService)
    {
        _syncMigratedDataService = syncMigratedDataService;
    }

    public async Task HandleAsync(SyncExportCleanNotification notification, CancellationToken cancellationToken)
    {
        await _syncMigratedDataService.DeleteAllAsync();
    }
}

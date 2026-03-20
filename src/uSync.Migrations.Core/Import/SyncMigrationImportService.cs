using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Web;

using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.SyncHandlers;
using uSync.BackOffice.SyncHandlers.Models;

namespace uSync.Migrations.Core.Import;

/// <summary>
///  handles the importing, (in the background). 
/// </summary>
internal class SyncMigrationImportService : ISyncMigrationImportService
{
    private readonly ISyncConfigService _syncConfigService;
    private readonly ISyncService _syncService;
    private readonly ISyncHandlerFactory _syncHandlerFactory;

    private readonly IBackgroundTaskQueue? _backgroundTaskQueue;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    public SyncMigrationImportService(
        ISyncConfigService syncConfigService,
        ISyncService syncService,
        ISyncHandlerFactory syncHandlerFactory,
        IBackgroundTaskQueue? backgroundTaskQueue,
        IUmbracoContextFactory umbracoContextFactory)
    {
        _syncConfigService = syncConfigService;
        _syncService = syncService;
        _syncHandlerFactory = syncHandlerFactory;
        _backgroundTaskQueue = backgroundTaskQueue;
        _umbracoContextFactory = umbracoContextFactory;
    }

    public void ImportInBackground(bool force, uSyncCallbacks? callbacks)
    {
        if (_backgroundTaskQueue == null)
            throw new InvalidOperationException("Background task queue is not available.");
        _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
        {
            using (ExecutionContext.SuppressFlow())
            {
                await Import("settings", force, callbacks);
                await Import("All", force, callbacks);
            }
        });
    }


    private async Task<bool> Import(string group, bool force, uSyncCallbacks? callbacks)
    {
        using (var reference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            var folders = _syncConfigService.GetFolders();
            var set = _syncConfigService.Settings.DefaultSet;
            return await Import(folders, set, group, force, callbacks);
        }
    }

    private async Task<bool> Import(string[] folders, string set, string group, bool force, uSyncCallbacks? callbacks)
    {
        var options = new SyncHandlerOptions
        {
            Action = HandlerActions.Import,
            Group = group,
            Set = set,
        };

        var handlers = _syncHandlerFactory.GetValidHandlers(options);

        var result = await _syncService.ImportAsync(folders, force, handlers, options, callbacks);
        return result.ContainsErrors() == false;
    }
}

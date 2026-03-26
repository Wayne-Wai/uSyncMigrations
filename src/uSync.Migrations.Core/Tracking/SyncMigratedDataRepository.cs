using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;

using uSync.Migrations.Core.Persistance;
using uSync.Migrations.Core.Tracking.Cache;

namespace uSync.Migrations.Core.Tracking;

internal class SyncMigratedDataRepository
    : SyncDataRespositoryBase<SyncMigratedData, string>,
        ISyncMigratedDataRepository
{
    public SyncMigratedDataRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ISyncMigratedFullDataSetCachePolicy cachePolicy,
        ILogger<SyncDataRespositoryBase<SyncMigratedData, string>> logger)
        : base(scopeAccessor, logger, appCaches,
            cachePolicy, SyncMigrationTracking.MigratedDataTableName)
    { }
 
}

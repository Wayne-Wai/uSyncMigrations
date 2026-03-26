using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;

using uSync.Migrations.Core.Persistance.Cache;

namespace uSync.Migrations.Core.Tracking.Cache;

internal class SyncMigratedFullDataSetCachePolicy : SyncFullDataSetRepositoryCachePolicy<SyncMigratedData, string>
    , ISyncMigratedFullDataSetCachePolicy
{
    public SyncMigratedFullDataSetCachePolicy(
        IAppPolicyCache globalCache,
        IScopeAccessor scopeAccessor,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(globalCache, scopeAccessor, repositoryCacheVersionService, cacheSyncService)
    { }
}

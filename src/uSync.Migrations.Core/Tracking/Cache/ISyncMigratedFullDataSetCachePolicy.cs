using uSync.Migrations.Core.Persistence.Cache;

namespace uSync.Migrations.Core.Tracking.Cache;

public interface ISyncMigratedFullDataSetCachePolicy 
    : ISyncFullDataSetRepositoryCachePolicy<SyncMigratedData, string>
{
    
}
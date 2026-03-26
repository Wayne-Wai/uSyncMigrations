using uSync.Migrations.Core.Persistance.Cache;

namespace uSync.Migrations.Core.Tracking.Cache;

public interface ISyncMigratedFullDataSetCachePolicy 
    : ISyncFullDataSetRepositoryCachePolicy<SyncMigratedData, string>
{
    
}
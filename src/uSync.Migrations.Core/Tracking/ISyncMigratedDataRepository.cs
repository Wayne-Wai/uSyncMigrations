using uSync.Migrations.Core.Persistence;

namespace uSync.Migrations.Core.Tracking;

public interface ISyncMigratedDataRepository : ISyncDataRespository<SyncMigratedData, string>
{ }
using uSync.Migrations.Core.Persistance;

namespace uSync.Migrations.Core.Tracking;

public interface ISyncMigratedDataRepository : ISyncDataRespository<SyncMigratedData, string>
{ }
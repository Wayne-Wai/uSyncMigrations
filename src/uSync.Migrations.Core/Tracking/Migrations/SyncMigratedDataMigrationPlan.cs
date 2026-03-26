using Umbraco.Cms.Infrastructure.Migrations;

using uSync.Migrations.Core.Tracking;

namespace uSync.Migrations.Core.Tracking.Migrations;

internal class SyncMigratedDataMigrationPlan : MigrationPlan
{
    public SyncMigratedDataMigrationPlan() 
        : base(SyncMigrationTracking.AppName)
    {
        From(string.Empty)
            .To<CreateMigratedDataTable>("Add SyncMigratedData Table");
    }
}

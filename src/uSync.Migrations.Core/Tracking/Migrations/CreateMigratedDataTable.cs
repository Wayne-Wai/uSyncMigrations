using Umbraco.Cms.Infrastructure.Migrations;

using uSync.Migrations.Core.Tracking;

namespace uSync.Migrations.Core.Tracking.Migrations;

internal class CreateMigratedDataTable : AsyncMigrationBase
{
    public CreateMigratedDataTable(IMigrationContext context) : base(context)
    { }

    protected override Task MigrateAsync()
    {
        if (!TableExists(SyncMigrationTracking.MigratedDataTableName))
            Create.Table<SyncMigratedData>().Do();

        return Task.CompletedTask;
    }
}

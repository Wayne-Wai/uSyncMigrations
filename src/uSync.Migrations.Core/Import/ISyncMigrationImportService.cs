using uSync.BackOffice;

namespace uSync.Migrations.Core.Import;

public interface ISyncMigrationImportService
{
    void ImportInBackground(bool force, uSyncCallbacks? callbacks);
}
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Core.Import;
using uSync.Migrations.Core.Tracking;
using uSync.Migrations.Core.Upgrade;

namespace uSync.Migrations.Core;

public static class MigrationsBuilderExtension
{
    public static IUmbracoBuilder AddSyncMigrations(this IUmbracoBuilder builder)
    {
        builder.AddSyncMigrationTracking();

        builder.Services.AddSingleton<ISyncMigrationImportService, SyncMigrationImportService>();
        builder.Services.AddSingleton<ISyncUpgradeService, SyncUpgradeService>();

        // load the loaders. 
        builder.WithCollectionBuilder<SyncFileUpgraderCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncFileUpgrader>());

        return builder;
    }
}

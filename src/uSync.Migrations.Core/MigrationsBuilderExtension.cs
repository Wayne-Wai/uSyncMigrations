using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Core.Upgrade;

namespace uSync.Migrations.Core;

public static class MigrationsBuilderExtension
{
    public static IUmbracoBuilder AdduSyncMigrations(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ISyncUpgradeService, SyncUpgradeService>();

        // load the loaders. 
        builder.WithCollectionBuilder<SyncFileUpgraderCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncFileUpgrader>());
           

        return builder;
    }
}

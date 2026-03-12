using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Migrators.Grid.Config.Properties;
using uSync.Migrations.Migrators.Grid.Helpers;

namespace uSync.Migrations.Migrators;

public static class MigratorsBuilderExtensions
{
    public static IUmbracoBuilder AddSyncMigrators(this IUmbracoBuilder builder)
    {
        // grid things. 
        builder.Services.AddTransient<ISyncGridNameService, SyncGridNameService>();
        builder.Services.AddTransient<ISyncGridContentTypeFinder, SyncGridContentTypeFinder>();

        builder.WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());

        return builder;
    }
}

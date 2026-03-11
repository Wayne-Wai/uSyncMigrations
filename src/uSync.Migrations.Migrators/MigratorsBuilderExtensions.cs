using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Migrators.Grid.Config.Settings;
using uSync.Migrations.Migrators.Grid.Helpers;

namespace uSync.Migrations.Migrators;

public static class MigratorsBuilderExtensions
{
    public static IUmbracoBuilder AddSyncMigrators(this IUmbracoBuilder builder)
    {
        // grid things. 
        builder.Services.AddSingleton<ISyncGridNameHelper, SyncGridNameHelper>();

        builder.WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());

        return builder;
    }
}

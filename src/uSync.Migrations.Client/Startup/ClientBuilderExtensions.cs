using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

using uSync.Migrations.Core;

namespace uSync.Migrations.Client.Startup;

public static class SyncMigrationsClientBuilderExtensions
{
    public static IUmbracoBuilder AddSyncMigrationsClient(this IUmbracoBuilder builder)
    {
        builder.AddMigrationsClientApi();
        builder.Services.AddSingleton<IPackageManifestReader, uSyncMigrationsManifestReader>();
        return builder;
    }
}

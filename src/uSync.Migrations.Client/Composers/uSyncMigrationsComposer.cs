using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

using uSync.Migrations.Core;

namespace uSync.Migrations.Client.Composers;

public class uSyncMigrationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();
        builder.AddMigrationsClientApi();
        builder.Services.AddSingleton<IPackageManifestReader, uSyncMigrationsManifestReader>();
    }
}

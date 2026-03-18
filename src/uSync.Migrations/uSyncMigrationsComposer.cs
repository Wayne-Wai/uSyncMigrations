using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Client.Startup;
using uSync.Migrations.Core;
using uSync.Migrations.Migrators;

namespace uSync.Migrations;

/// <summary>
///  add uSync.Migrations to the site, 
/// </summary>
[ComposeAfter(typeof(BackOffice.uSyncBackOfficeComposer))]
public class uSyncMigrationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddSyncMigrators();
        builder.AddSyncMigrations();
        builder.AddSyncMigrationsClient();
    }
}

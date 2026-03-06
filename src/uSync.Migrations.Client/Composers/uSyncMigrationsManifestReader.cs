using System.Reflection;

using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace uSync.Migrations.Client.Composers;

internal class uSyncMigrationsManifestReader : IPackageManifestReader
{
    public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        var version = Assembly.GetAssembly(typeof(uSyncMigrationsManifestReader))?.GetName().Version?.ToString() ?? "1.0.0";

        return Task.FromResult<IEnumerable<PackageManifest>>(
        [
            new PackageManifest
            {
                Id = "uSync.Migrations",
                Name = "uSync Migrations",
                Version = version,
                AllowTelemetry = true,
                Extensions = [
                    new {
                        name = "uSync Migrations Bundle",
                        alias = "uSync.Migrations.Bundle",
                        type = "bundle",
                        js = "/App_Plugins/uSyncMigrationsClient/migrations.js?v=" + version,
                    }
                ]
            }
        ]);
    }
                
}

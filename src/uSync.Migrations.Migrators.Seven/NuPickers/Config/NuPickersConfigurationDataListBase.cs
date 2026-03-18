using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;

using uSync.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Migrators.NuPickers.Configuration;

namespace uSync.Migrations.Migrators.NuPickers.Config;

internal abstract class NuPickersConfigurationDataListBase : SyncConfigurationMigratorBase
{
    private readonly IOptions<NuPickerMigrationOptions> _options;

    protected NuPickersConfigurationDataListBase(IOptions<NuPickerMigrationOptions> options)
    {
        _options = options;
    }

    protected virtual string? MapAssembly(string? assemblyFileName)
    {
        if (assemblyFileName == null)
            return assemblyFileName;

        var assemblyName = Path.Combine(Path.GetDirectoryName(assemblyFileName) ?? "", 
            Path.GetFileNameWithoutExtension(assemblyFileName));

        if (assemblyName is null) return assemblyFileName;

        return _options?.Value?.AssembliesMapping?.FirstOrDefault(x => x.Key.Equals(assemblyName)).Value ??
               assemblyFileName;
    }

    protected virtual string? MapNamespace(string? nameSpace)
    {
        if (nameSpace == null)
        {
            return nameSpace;
        }
        var namespaceOverride = _options.Value.NamespacesMapping?.OrderByDescending(x => x.Key.Length)
            .Where(x => nameSpace.Contains(x.Key));


        return namespaceOverride == null && namespaceOverride?.Any() != true
            ? nameSpace
            : nameSpace.Replace(namespaceOverride.FirstOrDefault().Key, namespaceOverride.FirstOrDefault().Value);
    }
}

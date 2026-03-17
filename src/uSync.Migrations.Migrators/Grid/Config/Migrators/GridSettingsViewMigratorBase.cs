using System.Xml.Linq;

using uSync.Migrations.Core.Upgrade;
using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Config.Migrators;

public abstract class GridSettingsViewMigratorBase
{
    public virtual Task<XElement?> GetAdditionalDataTypeAsync(string dataTypeAlias, List<GridSettingsConfigurationItemPreValue>? preValues)
        => Task.FromResult<XElement?>(null);

    public virtual Task<IEnumerable<UmbBlockGridTypeModel>> GetAdditionalGridBlocksAsync(string gridAlias, string blockLabel, Guid? groupKey) => 
        Task.FromResult(Enumerable.Empty<UmbBlockGridTypeModel>());

    public virtual Task<IEnumerable<Guid>> GetAllowedElementKeysAsync(string elementAlias)
        => Task.FromResult(Enumerable.Empty<Guid>());

}

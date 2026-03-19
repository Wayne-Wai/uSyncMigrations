using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace uSync.Migrations.Migrators.Grid.FileUpgraders;

internal class GridFileUpgraderBase
{
    private readonly IDataTypeService _dataTypeService;

    public GridFileUpgraderBase(IDataTypeService dataTypeService)
    {
        _dataTypeService = dataTypeService;
    }

    protected async Task<IDataType?> GetDataType(string alias)
    {
        return await _dataTypeService.GetAsync(alias);
    }

}

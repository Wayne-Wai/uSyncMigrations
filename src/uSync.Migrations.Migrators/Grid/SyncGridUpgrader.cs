using uSync.Migrations.Core.Upgrade;
using uSync.Core.Extensions;
using uSync.Core;
using uSync.Migrations.Migrators.Grid.Models;
using uSync.BackOffice.Services;

namespace uSync.Migrations.Migrators.Grid;

/// <summary>
///  the grid upgrader
/// </summary>
/// <remarks>
///  Upgraders run as part of the file copy process - so they only run when the user 
///  clicks 'upgrade' in the migrations tab. 
///  
///  upgraders have access to the file, and they go and tell uSync that there needs 
///  to be additional files created as part of the upgrade. 
///  
///  so for example, the grid upgrader will create new content types for the grid editors,
///  and then add those to the list of files to be saved as part of the upgrade process.
///  
///  then when the imports happen the extra content types will exist and the other parts 
///  of the upgrade process can handle it. 
/// </remarks>  
internal class SyncGridUpgrader : ISyncFileUpgrader
{
    private readonly ISyncFileService _syncFileService;

    public SyncGridUpgrader(ISyncFileService syncFileService)
    {
        _syncFileService = syncFileService;
    }

    public string ItemType => "DataType:Umbraco.Grid";

    public async Task<IEnumerable<SyncUpgradeFile>> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var config = file.Node.Element("Config").ValueOrDefault<string?>(null);
        if (string.IsNullOrWhiteSpace(config))
            return [file];

        if (config.TryDeserialize<GridConfiguration>(out var gridConfiguration) is false)
            return [file];

        // here we have the grid config, so if there is anything we want to do - pre import, 
        // we can do it here. 



        return [file];
    }
}

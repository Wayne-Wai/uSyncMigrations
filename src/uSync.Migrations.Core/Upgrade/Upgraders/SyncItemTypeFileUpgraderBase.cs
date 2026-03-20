using System.Xml.Linq;

namespace uSync.Migrations.Core.Upgrade.Upgraders;

/// <summary>
///  base class for any file upgraders that might want to also call
///  another upgrader because of the underling type. 
/// </summary>
/// <remarks>
///  the main use for this is datatypes that contain the grid, will 
///  likely want to call grid upgraders as part of the process,
///  because the grid upgrade needs to created new content types
///  for the grid editors.
/// </remarks>
public abstract class SyncItemTypeFileUpgraderBase
{
    protected readonly SyncFileUpgraderCollection _fileUpgraders;

    public SyncItemTypeFileUpgraderBase(SyncFileUpgraderCollection fileUpgraders)
    {
        _fileUpgraders = fileUpgraders;
    }

    public abstract string ItemType { get; }

    protected abstract string? GetItemKey(XElement node);

    /// <summary>
    ///  upgrade the file. it is possible here that we want to return multiple 
    ///  files, because we might need to create new things as part of the upgrade. 
    /// </summary>
    public async Task<SyncUpgradeResult> UpgradeFilesAsync(SyncUpgradeFile file)
    {
        var result = new SyncUpgradeResult(true);

        var itemKey = GetItemKey(file.Node);
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            result.Files = [file];
            return result;
        }

        var upgraders = _fileUpgraders.GetUpgraders($"{ItemType}:{itemKey}");
        if (upgraders is null)
        {
            result.Files = [file];
            return result;
        }

        var orginalFilePath = file.Filename;
        var files = new Dictionary<string, SyncUpgradeFile>()
        {
            { file.Filename, file }
        };

        foreach (var upgrader in upgraders)
        {
            var upgradeResult = await upgrader.UpgradeFilesAsync(files[orginalFilePath]);
            result.Messages.AddRange(upgradeResult.Messages);

            // loop through the upgraders, but always pass the latest version of the node
            // that way updates from one version will aways pass down.
            foreach (var update in upgradeResult.Files)
            {
                files[update.Filename] = update;
            }
        }

        result.Files = [..files.Values];

        return result;
    }

    public virtual async Task<IEnumerable<SyncUpgradeMessage>> AnalyseFilesAsync(SyncUpgradeFile file)
    {
        var itemKey = GetItemKey(file.Node);
        if (string.IsNullOrWhiteSpace(itemKey)) return [];

        var upgraders = _fileUpgraders.GetUpgraders($"{ItemType}:{itemKey}");
        if (upgraders is null) return [];

        var messages = new List<SyncUpgradeMessage>();

        foreach (var upgrader in upgraders)
        {
            messages.AddRange(await upgrader.AnalyseFilesAsync(file));           
        }
        return messages;
    }

}

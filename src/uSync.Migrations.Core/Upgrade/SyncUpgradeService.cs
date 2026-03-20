using System.Diagnostics.CodeAnalysis;

using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;

namespace uSync.Migrations.Core.Upgrade;

internal class SyncUpgradeService : ISyncUpgradeService
{
    private static int _majorVersion = BackOffice.uSync.Version.Major;

    private readonly ISyncFileService _syncFileService;
    private readonly ISyncConfigService _syncConfigService;
    private readonly SyncFileUpgraderCollection _fileUpgraders;

    /// <inheritdoc/>
    public SyncUpgradeService(ISyncFileService syncFileService, ISyncConfigService syncConfigService,
        SyncFileUpgraderCollection fileUpgraders)
    {
        _syncFileService = syncFileService;
        _syncConfigService = syncConfigService;
        _fileUpgraders = fileUpgraders;
    }

    /// <inheritdoc/>
    public async Task<bool> IgnoreLegacyFolderAsync(string folderPath, string message)
    {
        if (_syncFileService.DirectoryExists(folderPath) is false)
            return false;

        await _syncFileService.SaveFileAsync(Path.Combine(folderPath, ".ignore"),
            $"{message}\r\nDelete this file for the folder to be detected as legacy and upgradable");

        return true;
    }

    /// <inheritdoc/>
    public bool TryGetLatestLegacyFolder([NotNullWhen(true)] out string? folderPath)
    {
        folderPath = null;

        // if the default folder is not pointing to the latest version, 
        // then we don't do the checks, because the install is custom. 
        if (HasDefaultFolderConfig() is false) return false;

        for (int n = _majorVersion - 1; n >= 8; n--)
        {
            var legacyFolder = $"~/uSync/v{n}";
            if (_syncFileService.DirectoryExists(legacyFolder))
            {
                if (_syncFileService.FileExists(Path.Combine(legacyFolder, ".ignore")))
                    continue;

                folderPath = legacyFolder;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///  does the config for uSync folders have the last folder in the list
    ///  pointing to the current version of uSync?
    /// </summary>
    private bool HasDefaultFolderConfig()
    {

        var folders = _syncConfigService.GetFolders();
        if (folders is null || folders.Length == 0) return false;

        return folders.Last().Contains($"uSync/v{_majorVersion}/", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<List<SyncUpgradeMessage>> UpgradeFolderAsync(string folderPath, string targetFolder)
    {
        // move the current one out of the way. 
        BackupAndClearLatest();

        // loop through - each file gets loaded and pass to a upgrader (if we have one).

        var absPath = _syncFileService.GetAbsPath(folderPath);
        var absTarget = _syncFileService.GetAbsPath(targetFolder);

        var messages = new List<SyncUpgradeMessage>();

        foreach (var folder in _syncFileService.GetDirectories(folderPath))
        {
            foreach (var file in _syncFileService.GetFiles(folder, "*.config"))
            {
                var relativePath = file.Replace(absPath, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                // we have an upgrader for this type, so we need to read the file and pass it through the upgrader. 
                var sourceFile = await LoadFile(relativePath, file);
                if (sourceFile is null) continue;

                var itemType = sourceFile.Node.Name.LocalName;

                var upgraders = _fileUpgraders.GetUpgraders(itemType);
                if (upgraders?.Length > 0)
                {
                    foreach (var upgrader in upgraders)
                    {
                        var result = await upgrader.UpgradeFilesAsync(sourceFile);
                        if (result.Success)
                            await SaveUpgradedFilesAsync(absTarget, result.Files);

                        messages.AddRange(result.Messages);
                    }
                }
                else
                {
                    // copy to target. 
                    await SaveUpgradedFileAsync(absTarget, sourceFile);
                    //messages.Add(new SyncUpgradeMessage
                    //{
                    //    Status = SyncUpgradeStatus.Success,
                    //    Upgrader = "File Copy",
                    //    FileName = relativePath,
                    //    Message = "No upgrader found for this file, so it has been copied to the new location without changes."
                    //});
                }
            }
        }

        messages.AddRange(await UpgradeConfigFolder(absPath, absTarget));

        return messages;
    }

    /// <summary>
    ///  special case, where we lookg for a config folder in the uSync folder. if we 
    ///  find one, then 'special' Upgraders look at this to do thier thing. 
    /// </summary>
    /// <remarks>
    ///  typically the grid upgraders use this to make new content and doctypes 
    ///  for the given values in the grid config files. 
    /// </remarks>
    private async Task<List<SyncUpgradeMessage>> UpgradeConfigFolder(string folderPath, string targetPath)
    {
        var configFolder = Path.Combine(folderPath, "config");

        var messages = new List<SyncUpgradeMessage>();

        if (_syncFileService.DirectoryExists(configFolder) is false)
        {
            messages.Add(new SyncUpgradeMessage
            {
                Status = SyncUpgradeStatus.Warning,
                Upgrader = "Config Folder",
                FileName = "config",
                Message = "No config folder found, if you are migrating grid based config, there will likely be gaps in the migration without the config files."
            });
            return messages;
        }

        foreach (var file in _syncFileService.GetFiles(configFolder, "*.*"))
        {
            var relativePath = file.Replace(folderPath, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            var filename = Path.GetFileName(relativePath);

            var upgraders = _fileUpgraders.GetUpgraders(filename);
            if (upgraders?.Length > 0)
            {
                foreach (var upgrader in upgraders)
                {
                    var result = await upgrader.UpgradeFilesAsync(new SyncUpgradeFile
                    {
                        Filename = filename,
                        Node = new System.Xml.Linq.XElement("Blank"),
                        Content = await _syncFileService.LoadContentAsync(file)
                    });

                    if (result.Success)
                        await SaveUpgradedFilesAsync(targetPath, result.Files);

                    messages.AddRange(result.Messages);
                }
            }
        }

        return messages;
    }

    private async Task<SyncUpgradeFile?> LoadFile(string filename, string filePath)
    {
        try
        {
            return new SyncUpgradeFile
            {
                Filename = filename,
                Node = await _syncFileService.LoadXElementAsync(filePath)
            };
        }
        catch
        {
            return null;
        }
    }


    private void BackupAndClearLatest()
    {
        var current = LatestFolder;
        var backup = LatestFolder + "-backup";

        if (_syncFileService.DirectoryExists(current) is false) return;

        _syncFileService.CopyFolder(current, backup);
        _syncFileService.DeleteFolder(current);
        _syncFileService.CreateFolder(current);
    }


    private async Task SaveUpgradedFilesAsync(string targetFolder, IEnumerable<SyncUpgradeFile> files)
    {
        foreach (var file in files)
        {
            await SaveUpgradedFileAsync(targetFolder, file);
        }
    }

    private async Task SaveUpgradedFileAsync(string targetFolder, SyncUpgradeFile file)
    {
        var targetPath = Path.Combine(targetFolder, file.Filename.TrimStart(Path.DirectorySeparatorChar));
        await _syncFileService.SaveXElementAsync(file.Node, targetPath);
    }

    public string LatestFolder => $"~/uSync/v{_majorVersion}";
    public string LatestVersion => _majorVersion.ToString();


    public async Task<IEnumerable<SyncUpgradeMessage>> AnalyseFolderAsync(string folderPath)
    {
        List<SyncUpgradeMessage> messages = [];
        var absPath = _syncFileService.GetAbsPath(folderPath);
        foreach (var folder in _syncFileService.GetDirectories(folderPath))
        {
            foreach (var file in _syncFileService.GetFiles(folder, "*.config"))
            {
                var relativePath = file.Replace(absPath, string.Empty).TrimStart(Path.DirectorySeparatorChar);
                var sourceFile = await LoadFile(relativePath, file);
                if (sourceFile is null)
                {
                    messages.Add(new SyncUpgradeMessage
                    {
                        Status = SyncUpgradeStatus.Error,
                        Upgrader = "File Load",
                        FileName = relativePath,
                        Message = "Failed to load file for analysis"
                    });
                    continue;
                };

                var itemType = sourceFile.Node.Name.LocalName;
                var upgraders = _fileUpgraders.GetUpgraders(itemType);
                if (upgraders?.Length > 0)
                {
                    foreach (var upgrader in upgraders)
                    {
                        messages.AddRange(await upgrader.AnalyseFilesAsync(sourceFile));
                    }
                }
            }
        }

        messages.AddRange(await AnalyseConfigFolderAsync(folderPath));

        return messages;
    }

    public async Task<IEnumerable<SyncUpgradeMessage>> AnalyseConfigFolderAsync(string folderPath)
    {
        List<SyncUpgradeMessage> messages = [];
        var absPath = _syncFileService.GetAbsPath(folderPath);
        var configFolder = Path.Combine(absPath, "config");
        if (_syncFileService.DirectoryExists(configFolder) is false
            || _syncFileService.FileExists(Path.Combine(configFolder, "grid.editors.config.js")) is false)
        {
            return [
                new SyncUpgradeMessage {
                    Status = SyncUpgradeStatus.Warning,
                    Upgrader = "Config Folder",
                    FileName = "config/grid.editors.config.js",
                    Message = "No config folder found, if you are migrating grid based config, there will likely be gaps in the migration without the config files."
                }];
        }


        foreach (var file in _syncFileService.GetFiles(configFolder, "*.*"))
        {
            var relativePath = file.Replace(absPath, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            var filename = Path.GetFileName(relativePath);
            var upgraders = _fileUpgraders.GetUpgraders(filename);
            if (upgraders?.Length > 0)
            {
                foreach (var upgrader in upgraders)
                {
                    messages.AddRange(await upgrader.AnalyseFilesAsync(new SyncUpgradeFile
                    {
                        Filename = filename,
                        Node = new System.Xml.Linq.XElement("Blank"),
                        Content = await _syncFileService.LoadContentAsync(file)
                    }));
                }
            }
        }
        return messages;
    }
}

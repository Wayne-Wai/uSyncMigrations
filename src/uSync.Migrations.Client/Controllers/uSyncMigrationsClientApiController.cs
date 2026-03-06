using Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using uSync.Migrations.Core.Upgrade;

namespace uSync.Migrations.Client.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Migrations")]
public class uSyncMigrationsClientApiController : uSyncMigrationsClientApiControllerBase
{
    private readonly ISyncUpgradeService _upgradeService;

    public uSyncMigrationsClientApiController(ISyncUpgradeService upgradeService)
    {
        _upgradeService = upgradeService;
    }

    [HttpGet("check")]
    [ProducesResponseType<SyncUpgradeCheckResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Check()
    {
        var hasLegacy = _upgradeService.TryGetLatestLegacyFolder(out var legacyFolder);

        var result = new SyncUpgradeCheckResponse
        {
            HasLegacyFolder = hasLegacy,
            LegacyFolderPath = legacyFolder,
            LagacyTypes = [],
            LastestFolder = _upgradeService.LatestFolder,
            LatestVersion = _upgradeService.LatestVersion
        };

        return Ok(result);
    }

    [HttpPost("upgrade")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upgrade()
    {
        if (_upgradeService.TryGetLatestLegacyFolder(out var legacyFolder) is false)
            return BadRequest("No legacy folder found to upgrade");

        var result = await _upgradeService.UpgradeFolderAsync(legacyFolder, _upgradeService.LatestFolder);
        if (result)
        {
            await _upgradeService.IgnoreLegacyFolderAsync(legacyFolder,
                "This folder has been upgraded and will be ignored by uSync for upgrades.");
            return Ok();
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpPost("ignore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ignore()
    {
        if (_upgradeService.TryGetLatestLegacyFolder(out var legacyFolder) is false)
            return BadRequest("No legacy folder found to ignore");
        
        var result = await _upgradeService.IgnoreLegacyFolderAsync(legacyFolder,
            "This folder has been marked to be ignored by uSync for upgrades.");

        return result 
            ? Ok() 
            : StatusCode(StatusCodes.Status500InternalServerError);
    }

    public class SyncUpgradeCheckResponse
    {
        public bool HasLegacyFolder { get; set; }
        public string? LegacyFolderPath { get; set; }
        public string[] LagacyTypes { get; set; } = [];
        public required string LastestFolder { get; set; }
        public required string LatestVersion { get; set; }
    }
}
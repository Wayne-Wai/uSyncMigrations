using Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using uSync.BackOffice;
using uSync.BackOffice.Hubs;
using uSync.Migrations.Core.Import;
using uSync.Migrations.Core.Upgrade;

namespace uSync.Migrations.Client.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Migrations")]
public class uSyncMigrationsClientApiController : uSyncMigrationsClientApiControllerBase
{
    private readonly ILogger<uSyncMigrationsClientApiController> _logger;
    private readonly ISyncUpgradeService _upgradeService;
    private readonly ISyncMigrationImportService _importService;

    private readonly IHubContext<SyncHub> _hubContext;

    public uSyncMigrationsClientApiController(
        ILogger<uSyncMigrationsClientApiController> logger,
        ISyncUpgradeService upgradeService,
        ISyncMigrationImportService importService,
        IHubContext<SyncHub> hubContext)
    {
        _logger = logger;
        _upgradeService = upgradeService;
        _importService = importService;
        _hubContext = hubContext;
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
    [ProducesResponseType<List<SyncUpgradeMessage>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upgrade()
    {
        try
        {
            if (_upgradeService.TryGetLatestLegacyFolder(out var legacyFolder) is false)
                return BadRequest("No legacy folder found to upgrade");

            var result = await _upgradeService.UpgradeFolderAsync(legacyFolder, _upgradeService.LatestFolder);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the upgrade process.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
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

    [HttpPost("analyze")]
    [ProducesResponseType<IEnumerable<SyncUpgradeMessage>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Analyze()
    {
        if (_upgradeService.TryGetLatestLegacyFolder(out var legacyFolder) is false)
            return BadRequest("No legacy folder found to analyze");
        
        var result = await _upgradeService.AnalyseFolderAsync(legacyFolder);
       
        return Ok(result);
    }

    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Import(string? clientId)
    {
        _importService.ImportInBackground(true, GetCallbacksForClient(clientId));
        return Ok();
    }

    private uSyncCallbacks? GetCallbacksForClient(string? clientId)
    {
        var hubClient = clientId is null ? null :
            new HubClientService(_hubContext, clientId);

        return hubClient?.Callbacks() ?? null;

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
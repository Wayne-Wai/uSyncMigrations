using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace uSync.Migrations.Client.Controllers
{
    [ApiController]
    [BackOfficeRoute("usync-migrations/api/v{version:apiVersion}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminAccess)]
    [MapToApi(Constants.ApiName)]
    public class uSyncMigrationsClientApiControllerBase : ControllerBase
    {
    }
}

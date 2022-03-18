using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Api.Validation;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThreeDS;
using Transactions.Api.Models.External.ThreeDS;

namespace Transactions.Api.Controllers.External
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [Route("api/external/3ds")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ThreeDSController : Controller
    {
        private readonly ThreeDSService threeDSService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ITerminalsService terminalsService;
        private readonly ILogger logger;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ApiSettings apiSettings;

        public ThreeDSController(
             ThreeDSService threeDSService,
             ITerminalsService terminalsService,
             ILogger<ThreeDSController> logger,
             IHttpContextAccessorWrapper httpContextAccessor,
             ISystemSettingsService systemSettingsService,
             IOptions<ApiSettings> apiSettings)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.threeDSService = threeDSService;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.systemSettingsService = systemSettingsService;
            this.apiSettings = apiSettings.Value;
        }

        [HttpPost]
        [ValidateModelState]
        [Route("versioning")]
        public async Task<ActionResult<Versioning3DsResponse>> Versioning([FromBody] Versioning3DsRequest model)
        {
            throw new NotImplementedException(); 
        }
    }
}

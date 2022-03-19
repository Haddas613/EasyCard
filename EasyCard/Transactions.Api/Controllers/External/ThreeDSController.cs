using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Integration;
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
    public class ThreeDSController : ApiControllerBase
    {
        private readonly ThreeDSService threeDSService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ITerminalsService terminalsService;
        private readonly ILogger logger;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ApiSettings apiSettings;
        private readonly IExternalSystemsService externalSystemsService;

        public ThreeDSController(
             ThreeDSService threeDSService,
             ITerminalsService terminalsService,
             ILogger<ThreeDSController> logger,
             IHttpContextAccessorWrapper httpContextAccessor,
             ISystemSettingsService systemSettingsService,
             IOptions<ApiSettings> apiSettings,
             IExternalSystemsService externalSystemsService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.threeDSService = threeDSService;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.systemSettingsService = systemSettingsService;
            this.apiSettings = apiSettings.Value;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [ValidateModelState]
        [Route("versioning")]
        public async Task<ActionResult<Versioning3DsResponse>> Versioning([FromBody] Versioning3DsRequest model)
        {
            var res = await threeDSService.Versioning(model.CardNumber, GetCorrelationID());

            if (res.ErrorDetails != null)
            {
                return new Versioning3DsResponse
                {
                    ErrorMessage = res.ErrorDetails.ErrorDescription
                };
            }
            else
            {
                return new Versioning3DsResponse
                {
                    ThreeDSMethodUrl = res.VersioningResponse.ThreeDSMethodURL,
                    ThreeDSMethodData = res.VersioningResponse.ThreeDSMethodDataForm.ThreeDSMethodData,
                    ThreeDSServerTransID = res.VersioningResponse.ThreeDSServerTransID
                };
            }
        }

        [HttpPost]
        [ValidateModelState]
        [Route("authenticate")]
        public async Task<ActionResult<Authenticate3DsResponse>> Authenticate([FromBody] Authenticate3DsRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID);

            var shvaIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID));

            if (shvaIntegration == null)
            {
                return BadRequest("Shva is not connected to this terminal");
            }

            var settings = shvaIntegration.Settings.ToObject(typeof(Shva.ShvaTerminalSettings)) as Shva.ShvaTerminalSettings;

            if (settings == null)
            {
                throw new ApplicationException($"Could not get Shva settings");
            }

            var model = new ThreeDS.Contract.Authenticate3DsRequestModel
            {
                MerchantNumber = settings.MerchantNumber,
                ThreeDSServerTransID = request.ThreeDSServerTransID,
                CardNumber = request.CardNumber,
                Currency = request.Currency
            };

            var res = await threeDSService.Authentication(model, GetCorrelationID());

            if (res.ErrorDetails != null || res.ResponseData?.AuthenticationResponse.TransStatusEnum == ThreeDS.Models.TransStatusEnum.N)
            {
                return new Authenticate3DsResponse
                {
                    ErrorMessage = res.ErrorDetails.ErrorDescription ?? "rejected",
                    ErrorDetail = res.ErrorDetails.ErrorDetail
                };
            }
            else
            {
                return new Authenticate3DsResponse
                {
                     ChalengeRequired = res.ResponseData?.AuthenticationResponse.TransStatusEnum == ThreeDS.Models.TransStatusEnum.C,
                     AcsURL = res.ResponseData?.AuthenticationResponse?.AcsURL,
                     Base64EncodedChallengeRequest = res.Base64EncodedChallengeRequest,
                     ThreeDSServerTransID = res.ResponseData?.ThreeDSServerTransID
                };
            }
        }
    }
}

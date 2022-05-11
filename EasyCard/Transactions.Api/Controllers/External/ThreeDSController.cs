using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ThreeDS;
using Transactions.Api.Models.External.ThreeDS;
using Transactions.Business.Services;
using SharedIntegration = Shared.Integration;

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
        private readonly IThreeDSIntermediateStorage threeDSIntermediateStorage;
        private readonly IThreeDSChallengeService threeDSChallengeService;

        public ThreeDSController(
             ThreeDSService threeDSService,
             ITerminalsService terminalsService,
             ILogger<ThreeDSController> logger,
             IHttpContextAccessorWrapper httpContextAccessor,
             ISystemSettingsService systemSettingsService,
             IOptions<ApiSettings> apiSettings,
             IExternalSystemsService externalSystemsService,
             IThreeDSIntermediateStorage threeDSIntermediateStorage,
             IThreeDSChallengeService threeDSChallengeService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.threeDSService = threeDSService;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.systemSettingsService = systemSettingsService;
            this.apiSettings = apiSettings.Value;
            this.externalSystemsService = externalSystemsService;
            this.threeDSIntermediateStorage = threeDSIntermediateStorage;
            this.threeDSChallengeService = threeDSChallengeService;
        }

        [HttpPost]
        [ValidateModelState]
        [Route("versioning")]
        public async Task<ActionResult<Versioning3DsResponse>> Versioning([FromBody] Versioning3DsRequest model)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID.GetValueOrDefault()));

            var reportEntity = new Business.Entities.ThreeDSChallenge
            {
                Action = "Versioning",
                TerminalID = terminal.TerminalID,
                MerchantID = terminal.MerchantID,
                TransStatus = "exception"
            };

            try
            {
                var request = new ThreeDS.Contract.Versioning3DsRequestModel
                {
                    CardNumber = model.CardNumber,
                    NotificationURL = $"{apiSettings.CheckoutPortalUrl}/Home/Notification3Ds"
                };

                var res = await threeDSService.Versioning(request, GetCorrelationID(), reportEntity.ThreeDSChallengeID);

                if (res.ErrorDetails != null)
                {
                    reportEntity.TransStatus = "error";

                    return new Versioning3DsResponse
                    {
                        ErrorMessage = res.ErrorDetails.ErrorDescription,
                        PassThrough = res.ErrorDetails.PassThrough
                    };
                }
                else
                {
                    reportEntity.ThreeDSServerTransID = res.VersioningResponse.ThreeDSServerTransID;
                    reportEntity.TransStatus = "success";

                    return new Versioning3DsResponse
                    {
                        ThreeDSMethodUrl = res.VersioningResponse.ThreeDSMethodURL,
                        ThreeDSMethodData = res.VersioningResponse.ThreeDSMethodDataForm.ThreeDSMethodData,
                        ThreeDSServerTransID = res.VersioningResponse.ThreeDSServerTransID
                    };
                }
            }
            finally
            {
                await threeDSChallengeService.CreateEntity(reportEntity);
            }
        }

        [HttpPost]
        [ValidateModelState]
        [Route("authenticate")]
        public async Task<ActionResult<Authenticate3DsResponse>> Authenticate([FromBody] Authenticate3DsRequest request)
        {
            var reportEntity = EnsureExists(await threeDSChallengeService.GetThreeDSChallenge(request.ThreeDSServerTransID));

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID.GetValueOrDefault()));

            if (reportEntity.TerminalID != terminal.TerminalID)
            {
                throw new SecurityException("Invalid 3DSecure storage data access");
            }

            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID.GetValueOrDefault());

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
                Currency = request.Currency,
                NotificationURL = $"{apiSettings.CheckoutPortalUrl}/Home/Notification3Ds",
                MerchantName = terminal.Merchant.BusinessName,
                Amount = request.Amount,
                BrowserDetails = request.BrowserDetails,
                CardExpiration = request.CardExpiration
            };

            reportEntity.Action = "Authentication";
            reportEntity.TransStatus = "exception";

            try
            {
                var res = await threeDSService.Authentication(model, GetCorrelationID(), reportEntity.ThreeDSChallengeID);

                if (res.ErrorDetails != null ||
                    res.ResponseData?.AuthenticationResponse == null ||
                    string.IsNullOrWhiteSpace(res.ResponseData?.ThreeDSServerTransID) ||
                    res.ResponseData?.AuthenticationResponse.TransStatusEnum == ThreeDS.Models.TransStatusEnum.N)
                {
                    reportEntity.TransStatus = "error";

                    return new Authenticate3DsResponse
                    {
                        ErrorMessage = (res.ErrorDetails?.ErrorDescription ?? res.ResponseData?.AuthenticationResponse?.CardholderInfo) ?? "rejected",
                        ErrorDetail = res.ErrorDetails?.ErrorDetail
                    };
                }
                else
                {
                    bool chalengeRequired = res.ResponseData?.AuthenticationResponse.TransStatusEnum == ThreeDS.Models.TransStatusEnum.C;

                    if (!chalengeRequired)
                    {
                        if (string.IsNullOrWhiteSpace(res.ResponseData.AuthenticationResponse.AuthenticationValue))
                        {
                            reportEntity.TransStatus = "error";

                            return new Authenticate3DsResponse
                            {
                                ErrorMessage = "AuthenticationValue is empty",
                            };
                        }

                        reportEntity.TransStatus = "success";

                        await threeDSIntermediateStorage.StoreIntermediateData(new SharedIntegration.Models.ThreeDSIntermediateData(res.ResponseData.ThreeDSServerTransID, res.ResponseData.AuthenticationResponse.AuthenticationValue, res.ResponseData.AuthenticationResponse.Eci, res.ResponseData.AuthenticationResponse.Xid, reportEntity.ThreeDSChallengeID));
                    }
                    else
                    {
                        reportEntity.TransStatus = "chalengeRequired";
                    }

                    return new Authenticate3DsResponse
                    {
                        ChalengeRequired = chalengeRequired,
                        AcsURL = res.ResponseData?.AuthenticationResponse?.AcsURL,
                        Base64EncodedChallengeRequest = res.ResponseData?.Base64EncodedChallengeRequest,
                        ThreeDSServerTransID = res.ResponseData?.ThreeDSServerTransID
                    };
                }
            }
            finally
            {
                await threeDSChallengeService.UpdateEntity(reportEntity);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("authenticateCallback")]
        public async Task<ActionResult<Authenticate3DsResponse>> AuthenticateCallback()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var res = await reader.ReadToEndAsync();

                    var authRes = JsonConvert.DeserializeObject<Transactions.Api.Models.External.ThreeDS.Authenticate3DsCallback>(res);

                    if (authRes != null)
                    {
                        var reportEntity = await threeDSChallengeService.GetThreeDSChallenge(authRes.ThreeDSServerTransID);

                        if (reportEntity == null)
                        {
                            logger.LogError($"ThreeDSChallenge entity is not found: {authRes.ThreeDSServerTransID}");
                        }
                        else
                        {
                            reportEntity.Action = "AuthenticateCallback";
                            reportEntity.TransStatus = authRes.TransStatus;
                            await threeDSChallengeService.UpdateEntity(reportEntity);
                        }

                        await threeDSIntermediateStorage.StoreIntermediateData(new SharedIntegration.Models.ThreeDSIntermediateData(authRes.ThreeDSServerTransID, authRes.AuthenticationValue, authRes.Eci, authRes.Xid, reportEntity?.ThreeDSChallengeID)
                        {
                            TransStatus = authRes.TransStatus,
                            Request = res
                        });
                    }
                    else
                    {
                        logger.LogError($"ThreeDS AuthenticateCallback data is empty: {res}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ThreeDS AuthenticateCallback error: {ex.Message}");
            }

            return Ok();
        }
    }
}

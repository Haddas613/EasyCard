using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClearingHouse;
using ClearingHouse.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.ClearingHouse;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Integration;
using SharedApi = Shared.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/clearing-house")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClearingHouseApiController : ApiControllerBase
    {
        private readonly ClearingHouseAggregator clearingHouseAggregator;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IExternalSystemsService externalSystemsService;

        public ClearingHouseApiController(
            ClearingHouseAggregator clearingHouseAggregator,
            IMapper mapper,
            ITerminalsService terminalsService,
            IExternalSystemsService externalSystemsService)
        {
            this.clearingHouseAggregator = clearingHouseAggregator;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<SharedApi.Models.OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID);

            var clearingHouseIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.ClearingHouseExternalSystemID));

            if (clearingHouseIntegration == null)
            {
                return BadRequest("Clearing House is not connected to this terminal");
            }

            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(clearingHouseIntegration.ExternalSystemID), nameof(ExternalSystem));
            var settingsType = Type.GetType(externalSystem.SettingsTypeFullName);
            var settings = request.Settings.ToObject(settingsType);

            if (settings == null)
            {
                throw new ApplicationException($"Could not create instance of {externalSystem.SettingsTypeFullName}");
            }

            //TODO: temporary implementation. Make a request to clearingHouse as well
            if (settings is IExternalSystemSettings externalSystemSettings)
            {
                clearingHouseIntegration.Valid = await externalSystemSettings.Valid();
            }
            else
            {
                clearingHouseIntegration.Valid = true;
            }

            //TODO: save on success?
            //mapper.Map(request, clearingHouseIntegration);
            //await terminalsService.SaveTerminalExternalSystem(clearingHouseIntegration, terminal);
            var response = new SharedApi.Models.OperationResponse(Resources.MessagesResource.ConnectionSuccess, SharedApi.Models.Enums.StatusEnum.Success);

            if (!clearingHouseIntegration.Valid)
            {
                response.Status = SharedApi.Models.Enums.StatusEnum.Error;
                response.Message = Resources.MessagesResource.ConnectionFailed;
            }

            return response;
        }

        [HttpGet]
        [Route("merchants")]
        public async Task<ActionResult<SummariesResponse<MerchantSummary>>> GetMerchants([FromQuery]GetCHMerchantsRequest request)
        {
            var response = await clearingHouseAggregator.GetMerchants(new GetMerchantsQuery { MerchantID = request.MerchantID, MerchantName = request.MerchantName });

            return Ok(response);
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute]string entityID)
        {
            if (string.IsNullOrWhiteSpace(entityID))
            {
                return NotFound();
            }

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await clearingHouseAggregator.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
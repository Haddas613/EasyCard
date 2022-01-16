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
using Upay;
using SharedApi = Shared.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/upay")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UpayApiController : ApiControllerBase
    {
        private readonly UpayAggregator upayAggregator;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IExternalSystemsService externalSystemsService;

        public UpayApiController(
            UpayAggregator upayAggregator,
            IMapper mapper,
            ITerminalsService terminalsService,
            IExternalSystemsService externalSystemsService)
        {
            this.upayAggregator = upayAggregator;
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

            var uPayIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.UpayExternalSystemID));

            if (uPayIntegration == null)
            {
                return BadRequest("UPay is not connected to this terminal");
            }

            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(uPayIntegration.ExternalSystemID), nameof(ExternalSystem));
            var settingsType = Type.GetType(externalSystem.SettingsTypeFullName);
            var settings = request.Settings.ToObject(settingsType);

            if (settings == null)
            {
                throw new ApplicationException($"Could not create instance of {externalSystem.SettingsTypeFullName}");
            }

            //TODO: temporary implementation. Make a request to uPay as well
            if (settings is IExternalSystemSettings externalSystemSettings)
            {
                uPayIntegration.Valid = await externalSystemSettings.Valid();
            }
            else
            {
                uPayIntegration.Valid = true;
            }

            //TODO: save on success?
            //mapper.Map(request, shvaIntegration);
            await terminalsService.SaveTerminalExternalSystem(uPayIntegration, terminal);
            var response = new SharedApi.Models.OperationResponse(Resources.MessagesResource.ConnectionSuccess, SharedApi.Models.Enums.StatusEnum.Success);

            if (!uPayIntegration.Valid)
            {
                response.Status = SharedApi.Models.Enums.StatusEnum.Error;
                response.Message = Resources.MessagesResource.ConnectionFailed;
            }

            return response;
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute]string entityID)
        {
            if (string.IsNullOrWhiteSpace(entityID))
            {
                return NotFound();
            }

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await upayAggregator.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
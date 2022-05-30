using AutoMapper;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.Nayax;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nayax;
using Nayax.Models;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/nayax")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class NayaxApiController : ApiControllerBase
    {
        private readonly Nayax.NayaxProcessor nayaxProcessor;
        private readonly ITerminalsService terminalsService;
        private readonly IPinPadDevicesService pinPadDeviceService;
        private readonly IMapper mapper;
        private readonly IExternalSystemsService externalSystemsService;

        public NayaxApiController(
            NayaxProcessor nayaxProcessor,
            ITerminalsService terminalsService,
            IMapper mapper,
            IPinPadDevicesService pinPadDeviceService,
            IExternalSystemsService externalSystemsService)
        {
            this.nayaxProcessor = nayaxProcessor;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.pinPadDeviceService = pinPadDeviceService;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID);
           // NayaxTerminalSettings
            var nayaxIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));

            if (nayaxIntegration == null)
            {
                return BadRequest("Nayax PinPad is not connected to this terminal");
            }

            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(nayaxIntegration.ExternalSystemID), nameof(ExternalSystem));
            var settings = request.Settings.ToObject<NayaxTerminalCollection>();

            if (settings == null)
            {
                throw new ApplicationException($"Could not create instance of {nameof(NayaxTerminalCollection)}");
            }

            //TODO: temporary implementation. Make a request to nayax as well
            if (settings is IExternalSystemSettings externalSystemSettings)
            {
                nayaxIntegration.Valid = await externalSystemSettings.Valid();
            }
            else
            {
                nayaxIntegration.Valid = true;
            }

            bool connection = true;
            foreach (var device in settings.devices)
            {
                AuthRequest req = new AuthRequest(device.TerminalID, request.TerminalID);//settings.. //request.Settings.externalSystemSettings.);

                var getDetailsResult = await nayaxProcessor.TestConnection(req);
                connection = connection && getDetailsResult;
            }
            

            //TODO: save on success?
            //mapper.Map(request, nayaxIntegration);
            //await terminalsService.SaveTerminalExternalSystem(nayaxIntegration, terminal);
            var response = new OperationResponse(Resources.MessagesResource.ConnectionSuccess, StatusEnum.Success);

            if (!nayaxIntegration.Valid || !connection)
            {
                response.Status = StatusEnum.Error;
                response.Message = Resources.MessagesResource.ConnectionFailed;
            }

            return response;
        }

        [HttpPost]
        [Route("pair-device")]
        public async Task<ActionResult<OperationResponse>> PairDevice(PairRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.ECTerminalID.Value));
            var nayaxIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
            var devices = nayaxIntegration.Settings.ToObject<NayaxTerminalCollection>();

            if (devices.devices?.Any(d => d.TerminalID == request.terminalID) == false)
            {
                return BadRequest(new OperationResponse(NayaxMessagesResource.DeviceNotFound, StatusEnum.Error));
            }

            var dbDevice = await pinPadDeviceService.GetDevice(request.terminalID);

            if (dbDevice != null)
            {
                return Ok(new OperationResponse(NayaxMessagesResource.DeviceIsAlreadyPaired, StatusEnum.Error));
            }

            var response = new OperationResponse(NayaxMessagesResource.DevicePairedSuccessfully, StatusEnum.Success);

            try
            {
                //get settings
                var pairResult = await nayaxProcessor.PairDevice(request);

                if (!pairResult.Success)
                {
                    response.Status = StatusEnum.Error;
                    response.Message = NayaxMessagesResource.CouldNotPairTheDevice;

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("authenticate-device")]
        public async Task<ActionResult<OperationResponse>> AuthenticateDevice(AuthenticateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.ECTerminalID.Value));
            var nayaxIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));

            var devices = nayaxIntegration.Settings.ToObject<NayaxTerminalCollection>();
            var device = devices.devices?.FirstOrDefault(d => d.TerminalID == request.terminalID);

            if (device is null)
            {
                return BadRequest(new OperationResponse(NayaxMessagesResource.DeviceNotFound, StatusEnum.Error));
            }

            //get settings
            var pairResult = await nayaxProcessor.AuthenticateDevice(request);

            var response = new OperationResponse(NayaxMessagesResource.DeviceAuthenticatedSuccessfully, StatusEnum.Success);

            if (!pairResult.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = NayaxMessagesResource.CouldNotPairTheDevice;

                return BadRequest(response);
            }

            //TODO: request correlation id
            var newDevice = new Business.Entities.Integration.PinPadDevice
            {
                DeviceTerminalID = request.terminalID,
                PosName = device.PosName,
                CorrelationId = GetCorrelationID(),
                TerminalID = request.ECTerminalID
            };

            await pinPadDeviceService.AddPinPadDevice(newDevice);

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

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await nayaxProcessor.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}

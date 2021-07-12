using Merchants.Api.Models.Integrations.Nayax;
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

        public NayaxApiController(
            NayaxProcessor nayaxProcessor,
            ITerminalsService terminalsService)
        {
            this.nayaxProcessor = nayaxProcessor;
            this.terminalsService = terminalsService;
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

            if (devices.devices?.Any(d => d.TerminalID == request.terminalID && d.PosName == request.posName) == false)
            {
                return BadRequest(new OperationResponse(NayaxMessagesResource.DeviceNotFound, StatusEnum.Error));
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

            if (devices.devices?.Any(d => d.TerminalID == request.terminalID) == false)
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

            return Ok(response);
        }
    }
}

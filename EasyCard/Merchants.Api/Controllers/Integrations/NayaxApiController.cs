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
        public async Task<ActionResult<OperationResponse>> PairDevice(PairRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.ECTerminalID.Value));
            var nayaxIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));

            //get settings
            var pairResult = await nayaxProcessor.PairDevice(request);

            var response = new OperationResponse(NayaxMessagesResource.DevicePairedSuccessfully, StatusEnum.Success);

            if (!pairResult.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = NayaxMessagesResource.CouldNotPairTheDevice;

                return BadRequest(response);
            }

            NayaxTerminalSettings terminalSettings = nayaxIntegration.Settings.ToObject<NayaxTerminalSettings>();
            terminalSettings.TerminalID = request.terminalID;
            terminalSettings.PosName = request.posName;
            nayaxIntegration.Settings = JObject.FromObject(terminalSettings);
            await terminalsService.SaveTerminalExternalSystem(nayaxIntegration, terminal);

            return Ok(response);
        }

    }
}

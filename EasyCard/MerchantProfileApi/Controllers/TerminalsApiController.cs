using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServerClient;
using MerchantProfileApi.Extensions;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Security;

namespace MerchantProfileApi.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/terminals")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TerminalsApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;
        private readonly IUserManagementClient userManagementClient;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ICryptoServiceCompact cryptoServiceCompact;

        public TerminalsApiController(IMerchantsService merchantsService, ITerminalsService terminalsService, IMapper mapper, IUserManagementClient userManagementClient, ISystemSettingsService systemSettingsService, ICryptoServiceCompact cryptoServiceCompact)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.userManagementClient = userManagementClient;
            this.systemSettingsService = systemSettingsService;
            this.cryptoServiceCompact = cryptoServiceCompact;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalSummary>>> GetTerminals([FromQuery] TerminalsFilter filter)
        {
            var merchantId = User.GetMerchantID();

            var query = terminalsService.GetTerminals().Filter(filter);

            var response = new SummariesResponse<TerminalSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<TerminalSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }

        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<TerminalResponse>> GetTerminal([FromRoute]Guid terminalID)
        {
            var terminal = mapper.Map<TerminalResponse>(EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID)));

            var systemSettings = await systemSettingsService.GetSystemSettings();

            mapper.Map(systemSettings, terminal);

            return Ok(terminal);
        }

        // TODO: concurrency check, handle exceptions
        [HttpPut]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UpdateTerminal([FromRoute]Guid terminalID, [FromBody]UpdateTerminalRequest model)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            mapper.Map(model, terminal);

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalID));
        }

        [HttpPost]
        [Route("{terminalID}/resetApiKey")]
        public async Task<ActionResult<OperationResponse>> CreateTerminalApiKey([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            var opResult = await userManagementClient.CreateTerminalApiKey(new CreateTerminalApiKeyRequest { TerminalID = terminal.TerminalID, MerchantID = terminal.MerchantID });

            // TODO: CreateTerminalApiKey failed case
            return Ok(new OperationResponse (Messages.PrivateKeyUpdated, StatusEnum.Success, opResult.ApiKey));
        }

        // TODO: concurrency check, handle exceptions
        [HttpPost]
        [Route("{terminalID}/resetSharedApiKey")]
        public async Task<ActionResult<OperationResponse>> CreateSharedTerminalApiKey([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            var sharedApiKey = cryptoServiceCompact.EncryptCompact(Guid.NewGuid().ToString());
            terminal.SharedApiKey = Convert.FromBase64String(sharedApiKey);

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.SharedKeyUpdated, StatusEnum.Success, sharedApiKey));
        }
    }
}
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Extensions;
using System;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/terminal")]
    [ApiController]
    public class TerminalApiController : ApiControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;

        public TerminalApiController(IMerchantsService merchantsService, ITerminalsService terminalsService, IMapper mapper)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalSummary>>> GetTerminals([FromQuery] TerminalsFilter filter)
        {
            var query = terminalsService.GetTerminals().Filter(filter);

            var response = new SummariesResponse<TerminalSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<TerminalSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }

        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<TerminalResponse>> GetTerminal([FromRoute]long terminalID)
        {
            var terminal = mapper.Map<TerminalResponse>(EnsureExists(await terminalsService.GetTerminals().Include(t => t.Integrations)
                .FirstOrDefaultAsync(m => m.TerminalID == terminalID)));

            return Ok(terminal);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateTerminal([FromBody]TerminalRequest model)
        {
            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == model.MerchantID));

            var newTerminal = mapper.Map<Terminal>(model);

            await terminalsService.CreateEntity(newTerminal);

            return CreatedAtAction(nameof(GetTerminal), new { terminalID = newTerminal.TerminalID }, new OperationResponse(Messages.TerminalCreated, StatusEnum.Success, newTerminal.TerminalID));
        }

        [HttpPut]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UpdateTerminal([FromRoute]long terminalID, [FromBody]UpdateTerminalRequest model)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(d => d.TerminalID == terminalID));

            mapper.Map(model, terminal);

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalID));
        }

        [HttpPut]
        [Route("{terminalID}/externalsystem")]
        public async Task<ActionResult<OperationResponse>> SaveTerminalExternalSystem([FromRoute]long terminalID, [FromBody]ExternalSystemRequest model)
        {
            var externalSystem = new TerminalExternalSystem();

            mapper.Map(model, externalSystem);
            externalSystem.TerminalID = terminalID;

            await terminalsService.SaveTerminalExternalSystem(externalSystem);

            return Ok(new OperationResponse(Messages.ExternalSystemSaved, StatusEnum.Success, terminalID));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

namespace MerchantProfileApi.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/terminals")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class TerminalApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IMerchantsService merchantsService;
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
        public async Task<ActionResult<TerminalResponse>> GetTerminal([FromRoute]Guid terminalID)
        {
            var terminal = mapper.Map<TerminalResponse>(EnsureExists(await terminalsService.GetTerminals().Include(t => t.Integrations)
                .FirstOrDefaultAsync(m => m.TerminalID == terminalID)));

            return Ok(terminal);
        }
    }
}
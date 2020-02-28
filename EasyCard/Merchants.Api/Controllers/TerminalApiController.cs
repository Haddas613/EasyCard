using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class TerminalApiController : ControllerBase
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<TerminalSummary>))]
        public async Task<IActionResult> GetTerminals([FromQuery] TerminalsFilter filter)
        {
            var query = terminalsService.GetTerminals().Filter(filter);

            var response = new SummariesResponse<TerminalSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<TerminalSummary>(query.ApplyPagination(filter)).ToListAsync();

            return new JsonResult(response) { StatusCode = 200 };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TerminalResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> GetTerminal([FromRoute]long terminalID)
        {
            var terminal = await mapper.ProjectTo<TerminalResponse>(terminalsService.GetTerminals())
                .FirstOrDefaultAsync(m => m.TerminalID == terminalID).EnsureExists();

            return new JsonResult(terminal) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateTerminal([FromBody]TerminalRequest model)
        {
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == model.MerchantID).EnsureExists();

            var newTerminal = mapper.Map<Terminal>(model);

            await terminalsService.CreateEntity(newTerminal);

            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, newTerminal.TerminalID)) { StatusCode = 201 };
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> UpdateTerminal([FromRoute]long terminalID, [FromBody]UpdateTerminalRequest model)
        {
            var terminal = await terminalsService.GetTerminals().FirstOrDefaultAsync(d => d.TerminalID == terminalID).EnsureExists();

            mapper.Map(model, terminal);

            await terminalsService.UpdateEntity(terminal);

            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, terminalID)) { StatusCode = 201 };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Business.Services;
using MerchantsApi.Models.Terminal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Models;

namespace MerchantsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/terminal")]
    [ApiController]
    public class TerminalApiController : ControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly ITerminalsService terminalsService;

        public TerminalApiController(IMerchantsService merchantsService, ITerminalsService terminalsService)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(SummariesResponse<TerminalSummary>))]
        public async Task<IActionResult> GetTerminals(TerminalsFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TerminalResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> GetTerminal([FromRoute]long terminalID)
        {
            var terminal = await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID);

            if (terminal == null)
                return NotFound(new OperationResponse($"Terminal {terminalID} not found", StatusEnum.Error));

            //TODO: Automapper
            return new JsonResult(new TerminalResponse { MerchantID = terminal.MerchantID, Label = terminal.Label, TerminalID = terminal.TerminalID }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateTerminal([FromBody]TerminalRequest terminal)
        {
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == terminal.MerchantID);

            if (merchant == null)
            {
                return NotFound(new OperationResponse { Status = StatusEnum.Error });
            }

            var newTerminal = new Merchants.Business.Entities.Terminal.Terminal
            {
                MerchantID = terminal.MerchantID,
                Label = terminal.Label,
                Created = DateTime.UtcNow
            };

            merchant.Terminals.Add(newTerminal);

            await merchantsService.UpdateEntity(merchant);

            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, newTerminal.TerminalID)) { StatusCode = 201 };

        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> UpdateTerminal([FromRoute]long terminalID, [FromBody]UpdateTerminalRequest terminal)
        {
            throw new NotImplementedException();
        }
    }
}
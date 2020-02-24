using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantsApi.Business.Entities;
using MerchantsApi.Business.Services;
using MerchantsApi.Models.Terminal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> GetTerminals(TerminalsFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TerminalResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> GetTerminal([FromRoute]long terminalID)
        {
            var terminal = await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID);

            if (terminal == null)
                return NotFound(new OperationResponse($"Terminal {terminalID} not found", Shared.Models.Enums.StatusEnum.Error));

            //TODO: Automapper
            return new JsonResult(new TerminalResponse { MerchantID = terminal.MerchantID, Label = terminal.Label, TerminalID = terminal.TerminalID }) { StatusCode = 200 };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateTerminal([FromBody]TerminalRequest terminal)
        {
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == terminal.MerchantID);

            if (merchant == null)
            {
                return NotFound(new OperationResponse { Status = Shared.Models.Enums.StatusEnum.Error });
            }

            var newTerminal = new Terminal 
            {
                MerchantID = terminal.MerchantID,
                Label = terminal.Label,
                Created = DateTime.UtcNow
            };

            merchant.Terminals.Add(newTerminal);

            var res = await merchantsService.SaveChanges();

            return new JsonResult(new OperationResponse("ok", Shared.Models.Enums.StatusEnum.Success, newTerminal.TerminalID)) { StatusCode = 201 };

        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> UpdateTerminal([FromRoute]long terminalID, [FromBody]UpdateTerminalRequest terminal)
        {
            throw new NotImplementedException();
        }
    }
}
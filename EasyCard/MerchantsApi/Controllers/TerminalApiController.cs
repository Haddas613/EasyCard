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

        public TerminalApiController(IMerchantsService merchantsService)
        {
            this.merchantsService = merchantsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(SummariesResponse<TerminalSummary>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> GetTerminals(GetTerminalsFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TerminalResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{terminalID}")]
        public async Task<IActionResult> GetTerminal([FromRoute]long terminalID)
        {
            throw new NotImplementedException();
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

            var newTerminal = new Terminal();

            merchant.Terminals.Add(newTerminal);

            var res = await merchantsService.SaveChanges();

            return new JsonResult(res);

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
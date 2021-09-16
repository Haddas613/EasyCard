using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reporting.Api.Models.Tokens;
using Reporting.Business.Services;
using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using Reporting.Shared.Models.Tokens;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Helpers.Services;
using Transactions.Business.Services;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/cardtokens")]
    //[Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.Admin)]
    [ApiController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class CardTokenController : ApiControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly ITerminalsService terminalsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;

        public CardTokenController(IMapper mapper,
            IMerchantsService merchantsService,
            ITerminalsService terminalsService,
            ICreditCardTokenService creditCardTokenService)
        {
            this.mapper = mapper;
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.creditCardTokenService = creditCardTokenService;
        }

        [HttpGet]
        [Route("terminals-tokens")]
        public async Task<ActionResult<SummariesResponse<TerminalTokensResponse>>> GetTerminalsTokens([FromQuery]TerminalsTokensFilter filter)
        {
            var terminals = await mapper.ProjectTo<TerminalTokensResponse>(terminalsService.GetTerminals().Filter(filter).ApplyPagination(filter)).ToListAsync();
            var response = new SummariesResponse<TerminalTokensResponse>
            {
                NumberOfRecords = terminals.Count
            };

            var terminalsIds = terminals.Select(t => t.TerminalID).ToList();

            filter.DateFrom = DateTime.UtcNow.AddDays(-30);
            filter.DateTo = DateTime.UtcNow;
            var dateFromString = filter.DateFrom.Value.ToString("MM'/'yy");

            var q = creditCardTokenService.GetTokens(true);

            var tokens = await q
                .Where(t => terminalsIds.Contains(t.TerminalID.Value) && (t.Created >= filter.DateFrom && t.Created <= filter.DateTo))
                .GroupBy(k => k.TerminalID)
                .Select(t => new TokenResult { TerminalID = t.Key, Count = t.Count(), Expired = 0})

                .Union(q
                    .Where(t => terminalsIds.Contains(t.TerminalID.Value) && t.CardExpiration.ToDate() > filter.DateFrom)
                    .GroupBy(k => k.TerminalID)
                    .Select(t => new TokenResult { TerminalID = t.Key, Count = 0, Expired = t.Count() }))

                .ToListAsync();
            //.ToDictionaryAsync(k => k.Key, v => v.AsEnumerable());

            //foreach(var terminal in terminals)
            //{
            //    if (tokens.ContainsKey(terminal.TerminalID))
            //    {
            //        var t = tokens[terminal.TerminalID];
            //        terminal.Total = t.Count();
            //        terminal.Expired = t.Count(e => e.CardExpiration.Expired);
            //        terminal.Deleted = t.Count(e => !e.Active);
            //    }
            //}

            response.Data = terminals;
            return Ok(response);
        }

        public class TokenResult
        {
            public Guid? TerminalID { get; set; }

            public int Count { get; set; }

            public int Expired { get; set; }
        }
    }
}

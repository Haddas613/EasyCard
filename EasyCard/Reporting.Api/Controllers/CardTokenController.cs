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
using Reporting.Api.Models;
using Reporting.Api.Models.Tokens;
using Reporting.Business.Services;
using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using Reporting.Shared.Models.Tokens;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Helpers.Services;
using Transactions.Business.Services;
using Z.EntityFramework.Plus;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/cardtokens")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.Admin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-terminals-tokens")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(TerminalTokensResponse)
                    .GetObjectMeta(TerminalTokensResponseResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [Route("terminals-tokens")]
        public async Task<ActionResult<SummariesResponse<TerminalTokensResponse>>> GetTerminalsTokens([FromQuery]TerminalsTokensFilter filter)
        {
            var terminalsQuery = terminalsService.GetTerminals().Filter(filter);
            
            var numberOfRecords = terminalsQuery.DeferredCount().FutureValue();

            var terminals = await mapper.ProjectTo<TerminalTokensResponse>(terminalsQuery.Include(t => t.Merchant).Filter(filter).ApplyPagination(filter, 15))
                .Future()
                .ToListAsync();

            var response = new SummariesResponse<TerminalTokensResponse>
            {
                NumberOfRecords = numberOfRecords.Value
            };

            var terminalsIds = terminals.Select(t => t.TerminalID).ToList();

            var q = creditCardTokenService.GetTokens(true);

            filter.DateTo = (filter.DateTo ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date).AddDays(1);
            filter.DateFrom = filter.DateFrom ?? filter.DateTo.Value.AddDays(-30);

            var tokens = await q
                .Where(t => terminalsIds.Contains(t.TerminalID.Value) && t.ReplacementOfTokenID == null && (t.Created > filter.DateFrom && t.Created < filter.DateTo))
                .GroupBy(k => k.TerminalID)
                .Select(t => new TerminalTokenSubResult { TerminalID = t.Key, Created = t.Count(), Updated = 0, Expired = 0 })

                .Union(q
                    .Where(t => terminalsIds.Contains(t.TerminalID.Value) && t.ReplacementOfTokenID != null  && (t.Created > filter.DateFrom && t.Created < filter.DateTo))
                    .GroupBy(k => k.TerminalID)
                    .Select(t => new TerminalTokenSubResult { TerminalID = t.Key, Created = 0, Updated = t.Count(), Expired = 0 }))

                .Union(q
                    .Where(t => terminalsIds.Contains(t.TerminalID.Value) && (t.ExpirationDate > filter.DateFrom && t.ExpirationDate < filter.DateTo))
                    .GroupBy(k => k.TerminalID)
                    .Select(t => new TerminalTokenSubResult { TerminalID = t.Key, Created = 0, Updated = 0, Expired = t.Count() }))

                .ToDictionaryAsync(k => k.TerminalID, v => v);

            foreach (var terminal in terminals)
            {
                if (tokens.ContainsKey(terminal.TerminalID))
                {
                    var t = tokens[terminal.TerminalID];
                    terminal.CreatedCount = t.Created;
                    terminal.ExpiredCount = t.Expired;
                    terminal.UpdatedCount = t.Updated;
                }
            }

            response.Data = terminals;
            return Ok(response);
        }
    }
}

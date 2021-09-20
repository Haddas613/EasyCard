using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reporting.Api.Extensions.Filtering;
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
using SharedIntegration = Shared.Integration;

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
        private readonly ITransactionsService transactionsService;

        public CardTokenController(IMapper mapper,
            IMerchantsService merchantsService,
            ITerminalsService terminalsService,
            ICreditCardTokenService creditCardTokenService,
            ITransactionsService transactionsService)
        {
            this.mapper = mapper;
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.creditCardTokenService = creditCardTokenService;
            this.transactionsService = transactionsService;
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
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-tokens-transactions")]
        public TableMeta GetMetadataTokenTransactions()
        {
            return new TableMeta
            {
                Columns = typeof(TokenTransactionsResponse)
                    .GetObjectMeta(TokenTransactionsResponseResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [Route("terminals-tokens")]
        public async Task<ActionResult<SummariesResponse<TerminalTokensResponse>>> GetTerminalsTokens([FromQuery]TerminalsTokensFilter filter)
        {
            var terminalsQuery = terminalsService.GetTerminals().Filter(filter);
            
            var numberOfRecords = terminalsQuery.DeferredCount().FutureValue();

            var terminals = await mapper.ProjectTo<TerminalTokensResponse>(terminalsQuery.Include(t => t.Merchant).OrderByDescending(t => t.TerminalID).ApplyPagination(filter, 15))
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


            using (var dbTransaction = creditCardTokenService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var tokens = await q
                    .Where(t => terminalsIds.Contains(t.TerminalID.Value) && (t.Created > filter.DateFrom && t.Created < filter.DateTo))
                    .GroupBy(k => k.TerminalID)
                    .Select(t => new TerminalTokenSubResult
                    {
                        TerminalID = t.Key,
                        Created = t.Sum(e => e.ReplacementOfTokenID == null ? 1 : 0),
                        Updated = t.Sum(e => e.ReplacementOfTokenID == null ? 0 : 1),
                        Expired = 0
                    })

                    .Union(q
                        .Where(t => terminalsIds.Contains(t.TerminalID.Value) && (t.ExpirationDate > filter.DateFrom && t.ExpirationDate < filter.DateTo))
                        .GroupBy(k => k.TerminalID)
                        .Select(t => new TerminalTokenSubResult { TerminalID = t.Key, Created = 0, Updated = 0, Expired = t.Count() }))

                    .GroupBy(k => k.TerminalID)
                    .Select(t => new TerminalTokenSubResult { TerminalID = t.Key, Created = t.Sum(e => e.Created), Updated = t.Sum(e => e.Updated), Expired = t.Sum(e => e.Expired) })
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
            }
            response.Data = terminals;

            return Ok(response);
        }

        [HttpGet]
        [Route("tokens-transactions")]
        public async Task<ActionResult<SummariesResponse<TokenTransactionsResponse>>> GetTokenTransactions([FromQuery]TokensTransactionsFilter filter)
        {
            var response = new SummariesResponse<TokenTransactionsResponse>();

            filter.DateTo = (filter.DateTo ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date).AddDays(1);
            filter.DateFrom = filter.DateFrom ?? filter.DateTo.Value.AddDays(-30);

            var tokensQuery = creditCardTokenService.GetTokens(true).Filter(filter);

            using (var dbTransaction = creditCardTokenService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = tokensQuery.DeferredCount().FutureValue();

                tokensQuery = tokensQuery.ApplyPagination(filter, 15).OrderByDescending(t => t.CreditCardTokenID);
                var tokens = await mapper.ProjectTo<TokenTransactionsResponse>(tokensQuery)
                    .Future()
                    .ToListAsync();

                var transactionsQuery = transactionsService.GetTransactions().Where(t => t.TransactionTimestamp >= filter.DateFrom && t.TransactionTimestamp < filter.DateTo);

                var subres = await tokensQuery.Select(t => t.CreditCardTokenID)
                    .Join(transactionsQuery, o => o, i => i.CreditCardToken, (to, tr) => new TokenTransactionsSubResult {
                        CreditCardTokenID = to,
                        PaymentTransactionID = tr.PaymentTransactionID,
                        ProductionTransactions = tr.Status > 0 ? 1 : 0,
                        FailedTransactions = tr.Status < (Transactions.Shared.Enums.TransactionStatusEnum )(-1) ? 1 : 0,
                        TotalSum = tr.SpecialTransactionType == SharedIntegration.Models.SpecialTransactionTypeEnum.Refund ? 0 : tr.TotalAmount,
                        TotalRefund = tr.SpecialTransactionType == SharedIntegration.Models.SpecialTransactionTypeEnum.Refund ? tr.TotalAmount : 0,
                    })
                    .GroupBy(k => k.CreditCardTokenID)
                    .Select(t => new TokenTransactionsSubResult
                    {
                        CreditCardTokenID = t.Key,
                        ProductionTransactions = t.Sum(e => e.ProductionTransactions),
                        FailedTransactions = t.Sum(e => e.FailedTransactions),
                        TotalSum = t.Sum(e => e.TotalSum)
                    })
                    .ToDictionaryAsync(k => k.CreditCardTokenID, v => v);

                foreach (var token in tokens)
                {
                    if (subres.ContainsKey(token.CreditCardTokenID))
                    {
                        var sub = subres[token.CreditCardTokenID];
                        token.ProductionTransactions = sub.ProductionTransactions;
                        token.FailedTransactions = sub.FailedTransactions;
                        token.TotalSum = sub.TotalSum;
                        token.TotalRefund = sub.TotalRefund;
                    }
                }
                response.Data = tokens;
                response.NumberOfRecords = numberOfRecords.Value;
            }

            return Ok(response);
        }
    }
}

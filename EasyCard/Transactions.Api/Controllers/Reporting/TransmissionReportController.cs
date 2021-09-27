using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Merchants.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Api.Validation;
using Transactions.Api.Validation.Options;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;

namespace Transactions.Api.Controllers.Reporting
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/reporting/transmission")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class TransmissionReportController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ApplicationSettings appSettings;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransmissionReportController(ITransactionsService transactionsService, IMapper mapper,
            ITerminalsService terminalsService, ILogger<TransactionsApiController> logger,
            IHttpContextAccessorWrapper httpContextAccessor, IOptions<ApplicationSettings> appSettings)
        {
            this.transactionsService = transactionsService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.appSettings = appSettings.Value;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(TransmissionReportSummaryAdmin) : typeof(TransmissionReportSummary))
                    .GetObjectMeta(TransmissionReportSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesAmountResponse<TransmissionReportSummary>>> GetTransmissionReport([FromQuery]TransmitReportFilter filter)
        {
            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dataQuery = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.SortDesc)
                    .ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit);

                var numberOfRecords = query.DeferredCount().FutureValue();
                var totalAmount = query.DeferredSum(e => e.TotalAmount).FutureValue();
                var response = new SummariesAmountResponse<TransmissionReportSummary>();

                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var summary = await mapper.ProjectTo<TransmissionReportSummaryAdmin>(dataQuery).Future().ToListAsync();

                    var terminalsId = summary.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    //TODO: Merchant name instead of BusinessName
                    summary.ForEach(s =>
                    {
                        if (terminals.ContainsKey(s.TerminalID))
                        {
                            s.TerminalName = terminals[s.TerminalID].Label;
                            s.MerchantName = terminals[s.TerminalID].BusinessName;
                        }
                    });

                    response.Data = summary;
                    response.NumberOfRecords = numberOfRecords.Value;
                    response.TotalAmount = totalAmount.Value;
                    return Ok(response);
                }
                else
                {
                    // TODO: try to remove ProjectTo
                    response.Data = await mapper.ProjectTo<TransmissionReportSummary>(dataQuery).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecords.Value;
                    response.TotalAmount = totalAmount.Value;
                    return Ok(response);
                }
            }
        }
    }
}
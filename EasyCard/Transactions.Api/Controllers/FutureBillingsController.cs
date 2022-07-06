using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Security;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Billing;
using Transactions.Business.Services;
using Z.EntityFramework.Plus;

namespace Transactions.Api.Controllers
{
    // TODO: move to reporting service
    [Route("api/future-billings")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    public class FutureBillingsController : ApiControllerBase
    {
        private readonly IFutureBillingsService futureBillingsService;
        private readonly IMapper mapper;
        private readonly Shared.ApplicationSettings appSettings;

        public FutureBillingsController(
            IFutureBillingsService futureBillingsService,
            IMapper mapper,
            IOptions<Shared.ApplicationSettings> appSettings)
        {
            this.futureBillingsService = futureBillingsService;
            this.mapper = mapper;
            this.appSettings = appSettings.Value;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(FutureBillingSummary)
                    .GetObjectMeta(BillingDealSummaryResource.ResourceManager, CurrentCulture)
            };
        }

        /// <summary>
        /// Get future billing by filters
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<FutureBillingSummary>>> GetFutureBillingDeals([FromQuery] FutureBillingDealsFilter filter)
        {
            var queryRes = (await futureBillingsService.GetFutureBillings(filter.TerminalID, filter.ConsumerID, filter.BillingDealID, filter.DateFrom, filter.DateTo))
                .AsQueryable();

            var response = new SummariesResponse<FutureBillingSummary>();
            response.NumberOfRecords = queryRes.Count();

            response.Data = mapper.Map<IEnumerable<FutureBillingSummary>>(queryRes.ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit));

            return Ok(response);
        }
    }
}
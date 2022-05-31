using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/future-billings")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    public class FutureBillingsController : ApiControllerBase
    {
        private readonly IFutureBillingsService futureBillingsService;
        private readonly IMapper mapper;

        public FutureBillingsController(
            IFutureBillingsService futureBillingsService,
            IMapper mapper)
        {
            this.futureBillingsService = futureBillingsService;
            this.mapper = mapper;
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
            var query = futureBillingsService.GetFutureBillings().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var response = new SummariesResponse<FutureBillingSummary>();

            response.Data = await mapper.ProjectTo<FutureBillingSummary>(query.ApplyPagination(filter)).Future().ToListAsync();

            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }
    }
}
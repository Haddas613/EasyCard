using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reporting.Business.Services;
using Reporting.Shared.Models;
using Shared.Api;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/dashboard")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DashboardController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IDashboardService dashboardService;
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;
        private readonly ISystemSettingsService systemSettingsService;

        public DashboardController(IMerchantsService merchantsService, ITerminalsService terminalsService, IMapper mapper, ISystemSettingsService systemSettingsService, IDashboardService dashboardService)
        {
            this.merchantsService = merchantsService;
            this.mapper = mapper;
            this.systemSettingsService = systemSettingsService;
            this.dashboardService = dashboardService;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [Route("transactionsTotals")]
        public async Task<ActionResult<IEnumerable<TransactionsTotals>>> GetTransactionsTotals([FromQuery]MerchantDashboardQuery query)
        {
            var res = await dashboardService.GetTransactionsTotals(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("paymentTypeTotals")]
        public async Task<ActionResult<IEnumerable<PaymentTypeTotals>>> GetPaymentTypeTotals([FromQuery] MerchantDashboardQuery query)
        {
            var res = await dashboardService.GetPaymentTypeTotals(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("transactionTimeline")]
        public async Task<ActionResult<IEnumerable<TransactionTimeline>>> GetTransactionTimeline([FromQuery] MerchantDashboardQuery query)
        {
            var res = await dashboardService.GetTransactionTimeline(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("itemsTotals")]
        public async Task<ActionResult<IEnumerable<ItemsTotals>>> GetItemsTotals([FromQuery] MerchantDashboardQuery query)
        {
            var res = await dashboardService.GetItemsTotals(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("consumersTotals")]
        public async Task<ActionResult<IEnumerable<ConsumersTotals>>> GetConsumersTotals([FromQuery] MerchantDashboardQuery query)
        {
            var res = await dashboardService.GetConsumersTotals(query);

            return Ok(res);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reporting.Business.Services;
using Reporting.Shared.Models;
using Shared.Api;
using Shared.Helpers.Security;
using Transactions.Business.Entities.Reporting;
using Transactions.Business.Services;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/reports")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MerchantReportsController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IReportingService reportingService;
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;
        private readonly ISystemSettingsService systemSettingsService;

        public MerchantReportsController(IMerchantsService merchantsService, ITerminalsService terminalsService, IMapper mapper, ISystemSettingsService systemSettingsService, IReportingService reportingService)
        {
            this.merchantsService = merchantsService;
            this.mapper = mapper;
            this.systemSettingsService = systemSettingsService;
            this.reportingService = reportingService;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [Route("billings")]
        public async Task<ActionResult<IEnumerable<BillingSummaryReport>>> GetBillingSummaryReport()
        {
            var sharedTerminals = false;

            if (User.IsTerminal())
            {
                var terminal = EnsureExists(await terminalsService.GetTerminal((User.GetTerminalID()?.FirstOrDefault()).GetValueOrDefault()));

                sharedTerminals = terminal.Settings.SharedCreditCardTokens == true;
            }

            var res = await reportingService.GetBillingSummaryReport(sharedTerminals).ToListAsync();

            return Ok(res);
        }
    }
}

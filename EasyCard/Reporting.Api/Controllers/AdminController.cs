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
using Reporting.Shared.Models.Admin;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Helpers.Services;

namespace Reporting.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.Admin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : ApiControllerBase
    {
        private readonly IAdminService adminService;
        private readonly IMerchantsService merchantsService;
        private readonly ITerminalsService terminalsService;

        public AdminController(IAdminService adminService, IMerchantsService merchantsService, ITerminalsService terminalsService)
        {
            this.adminService = adminService;
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [Route("sms-timelines")]
        public async Task<ActionResult<AdminSmsTimelines>> GetSmsTotals([FromQuery]DashboardQuery query)
        {
            var res = await adminService.GetSmsTotals(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("transactions-totals")]
        public async Task<ActionResult<IEnumerable<TransactionsTotals>>> GetTransactionsTotals([FromQuery]DashboardQuery query)
        {
            var res = await adminService.GetTransactionsTotals(query);

            return Ok(res);
        }

        [HttpGet]
        [Route("merchants-totals")]
        public async Task<ActionResult<IEnumerable<MerchantsTotals>>> GetItemsTotals([FromQuery] DashboardQuery query)
        {
            var res = await adminService.GetMerchantsTotals(query);

            if (res.Any())
            {
                var merchantIDs = res.Select(r => r.MerchantID).Distinct();
                var merchants = await merchantsService.GetMerchants()
                    .Where(e => merchantIDs.Contains(e.MerchantID))
                    .ToDictionaryAsync(k => k.MerchantID, v => v.BusinessName ?? v.MarketingName);

                foreach(var r in res)
                {
                    r.MerchantName = merchants[r.MerchantID];
                }
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("tds-challenge-report")]
        public async Task<ActionResult<SummariesResponse<ThreeDSChallengeSummary>>> ThreeDSChallengeReport([FromQuery] ThreeDSChallengeReportQuery query)
        {
            var res = new SummariesResponse<ThreeDSChallengeSummary>();
            var data = await adminService.GetThreeDSChallengeReport(query);

            res.Data = data;
            res.NumberOfRecords = data.Count(); //TODO

            if (data.Any())
            {
                var TerminalIDs = data.Select(r => r.TerminalID).Distinct();
                var terminals = await terminalsService.GetTerminals()
                    .Where(e => TerminalIDs.Contains(e.TerminalID))
                    .Select(d => new { d.TerminalID, d.Label, d.Merchant.BusinessName})
                    .ToDictionaryAsync(k => k.TerminalID);

                foreach (var r in data)
                {
                    var terminal = terminals[r.TerminalID];
                    r.MerchantName = terminal.BusinessName;
                    r.TerminalName = terminal.Label;
                }
            }

            return Ok(res);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-tds-challenge-report")]
        public TableMeta ThreeDSChallengeReportMeta()
        {
            return new TableMeta
            {
                Columns = typeof(ThreeDSChallengeSummary)
                    .GetObjectMeta(ThreeDSChallengeSummaryResource.ResourceManager, CurrentCulture)
            };
        }
    }
}

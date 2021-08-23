using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Masav;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Z.EntityFramework.Plus;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [Route("api/masav")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class MasavFileController : ApiControllerBase
    {
        private readonly ILogger logger;
        private readonly IMasavFileService masavFileService;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;

        public MasavFileController(
            ILogger<MasavFileController> logger,
            IMasavFileService masavFileService,
            IMapper mapper,
            ITerminalsService terminalsService)
        {
            this.logger = logger;
            this.masavFileService = masavFileService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(MasavFileSummary)
                    .GetObjectMeta(MasavFileResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-row")]
        public TableMeta GetRowMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(MasavFileRowSummary)
                    .GetObjectMeta(MasavFileRowResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<MasavFileSummary>>> GetMasavFiles([FromQuery] MasavFileFilter filter)
        {
            var query = masavFileService.GetMasavFiles().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var response = new SummariesResponse<MasavFileSummary>();

            response.Data = await mapper.ProjectTo<MasavFileSummary>(query.ApplyPagination(filter)).Future().ToListAsync();

            var terminalsId = response.Data.Select(t => t.TerminalID).Distinct();

            var terminals = await terminalsService.GetTerminals()
                .Where(t => terminalsId.Contains(t.TerminalID))
                .Select(t => new { t.TerminalID, t.Label })
                .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label });

            foreach (var data in response.Data)
            {
                if (data.TerminalID.HasValue && terminals.ContainsKey(data.TerminalID.Value))
                {
                    data.TerminalName = terminals[data.TerminalID.Value].Label;
                }
            }

            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<MasavFileSummary>> GetMasavFile([FromRoute] long id)
        {
            var dbEntity = EnsureExists(await masavFileService.GetMasavFiles().FirstOrDefaultAsync(m => m.MasavFileID == id));
            var response = mapper.Map<MasavFileSummary>(dbEntity);

            if (response.TerminalID.HasValue)
            {
                var terminal = await terminalsService.GetTerminals().Select(s => s.Label).FirstOrDefaultAsync();

                if (terminal != null)
                {
                    response.TerminalName = terminal;
                }
            }

            return Ok(response);
        }

        [HttpGet("rows")]
        public async Task<ActionResult<SummariesResponse<MasavFileRowSummary>>> GetMasavFileRows([FromQuery] MasavFileRowFilter filter)
        {
            var query = masavFileService.GetMasavFileRows().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var masavFile = EnsureExists(await masavFileService.GetMasavFiles().FirstOrDefaultAsync(f => f.MasavFileID == filter.MasavFileID));

            var response = new SummariesResponse<MasavFileRowSummary>();

            response.Data = await mapper.ProjectTo<MasavFileRowSummary>(query.ApplyPagination(filter)).Future().ToListAsync();

            //TODO: add to entity?
            foreach (var row in response.Data)
            {
                row.Currency = masavFile.Currency;
            }

            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }
    }
}

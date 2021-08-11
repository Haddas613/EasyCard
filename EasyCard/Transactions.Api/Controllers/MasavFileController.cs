using AutoMapper;
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
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiController]
    public class MasavFileController : ApiControllerBase
    {
        private readonly ILogger logger;
        private readonly IMasavFileService masavFileService;
        private readonly IMapper mapper;

        static bool ensureonce = false;

        public MasavFileController(
            ILogger<MasavFileController> logger,
            IMasavFileService masavFileService,
            IMapper mapper)
        {
            this.logger = logger;
            this.masavFileService = masavFileService;
            this.mapper = mapper;
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

            response.NumberOfRecords = numberOfRecordsFuture.Value;

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

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("generate")]
        public async Task<ActionResult<OperationResponse>> GenerateMasavFile()
        {
            //TODO: implement
            var response = new OperationResponse();

            return Ok(response);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Audit;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/audit")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class AuditApiController : ApiControllerBase
    {
        private readonly IMapper mapper;
        private readonly IMerchantsService merchantsService;

        public AuditApiController(IMapper mapper, IMerchantsService merchantsService)
        {
            this.mapper = mapper;
            this.merchantsService = merchantsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(AuditEntryResponse)
                    .GetObjectMeta(AuditResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<AuditEntryResponse>>> Get([FromQuery] AuditFilter filter)
        {
            var query = merchantsService.GetMerchantHistories()
                .Include(a => a.Merchant)
                .Include(a => a.Terminal)
                .AsQueryable()
                .Filter(filter);

            using (var dbTransaction = merchantsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<AuditEntryResponse> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(AuditEntryResponse.MerchantHistoryID), filter.OrderByDirection).ApplyPagination(filter);

                response.Data = await mapper.ProjectTo<AuditEntryResponse>(query).ToListAsync();

                return Ok(response);
            }
        }
    }
}
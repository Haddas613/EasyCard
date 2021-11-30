using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClearingHouse;
using ClearingHouse.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.ClearingHouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/clearing-house")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClearingHouseApiController : ApiControllerBase
    {
        private readonly ClearingHouseAggregator clearingHouseAggregator;
        private readonly IMapper mapper;

        public ClearingHouseApiController(ClearingHouseAggregator clearingHouseAggregator, IMapper mapper)
        {
            this.clearingHouseAggregator = clearingHouseAggregator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("merchants")]
        public async Task<ActionResult<SummariesResponse<MerchantSummary>>> GetMerchants([FromQuery]GetCHMerchantsRequest request)
        {
            var response = await clearingHouseAggregator.GetMerchants(new GetMerchantsQuery { MerchantID = request.MerchantID, MerchantName = request.MerchantName });

            return Ok(response);
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute]string entityID)
        {
            if (string.IsNullOrWhiteSpace(entityID))
            {
                return NotFound();
            }

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await clearingHouseAggregator.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
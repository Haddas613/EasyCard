using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearingHouse;
using ClearingHouse.Models;
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

        public ClearingHouseApiController(ClearingHouseAggregator clearingHouseAggregator)
        {
            this.clearingHouseAggregator = clearingHouseAggregator;
        }

        [HttpGet]
        [Route("merchants")]
        public async Task<ActionResult<SummariesResponse<MerchantSummary>>> GetMerchants([FromQuery]GetCHMerchantsRequest request)
        {
            var response = await clearingHouseAggregator.GetMerchants(new GetMerchantsQuery { MerchantID = request.MerchantID, MerchantName = request.MerchantName });

            return Ok(response);
        }
    }
}
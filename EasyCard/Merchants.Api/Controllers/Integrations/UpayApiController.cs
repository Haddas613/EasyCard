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
using Upay;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/upay")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UpayApiController : ApiControllerBase
    {
        private readonly UpayAggregator upayAggregator;
        private readonly IMapper mapper;

        public UpayApiController(UpayAggregator upayAggregator, IMapper mapper)
        {
            this.upayAggregator = upayAggregator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute]string entityID)
        {
            if (string.IsNullOrWhiteSpace(entityID))
            {
                return NotFound();
            }

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await upayAggregator.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
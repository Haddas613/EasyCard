using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit;
using ClearingHouse;
using ClearingHouse.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.ClearingHouse;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Integration;
using Upay;
using SharedApi = Shared.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/bit")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BitApiController : ApiControllerBase
    {
        private readonly BitProcessor bitProcessor;
        private readonly IMapper mapper;

        public BitApiController(
            BitProcessor bitProcessor,
            IMapper mapper)
        {
            this.bitProcessor = bitProcessor;
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

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await bitProcessor.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Transactions.Api.Controllers
{
    [Route("api/system")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class SystemApiController : ApiControllerBase
    {
        private readonly IRequestLogStorageService requestLogStorageService;

        public SystemApiController(IRequestLogStorageService requestLogStorageService)
        {
            this.requestLogStorageService = requestLogStorageService;
        }

        [HttpGet("log")]
        public async Task<ActionResult<LogRequestEntity>> GetCorrelationLog([FromQuery]DateTime? date, [FromQuery]string correlationId)
        {
            if (!date.HasValue || string.IsNullOrWhiteSpace(correlationId))
            {
                return BadRequest();
            }

            return EnsureExists(await requestLogStorageService.Get(date.Value, correlationId));
        }
    }
}
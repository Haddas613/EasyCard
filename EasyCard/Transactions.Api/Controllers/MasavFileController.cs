using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public MasavFileController(ILogger<MasavFileController> logger)
        {
            this.logger = logger;
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

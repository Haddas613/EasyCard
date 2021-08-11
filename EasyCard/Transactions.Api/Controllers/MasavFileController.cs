using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Masav;

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
                    .GetObjectMeta(MasavFileResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MasavFileSummary>>> GetMasavFiles(MasavFileFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet("rows")]
        public async Task<ActionResult<IEnumerable<MasavFileSummary>>> GetMasavFileRows(MasavFileRowFilter filter)
        {
            throw new NotImplementedException();
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

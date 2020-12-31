using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/terminal-templates")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)] // TODO: bearer
    [ApiController]
    public class TerminalTemplatesApiController : ApiControllerBase
    {
        private readonly ITerminalTemplatesService terminalTemplatesService;

        public TerminalTemplatesApiController(ITerminalTemplatesService terminalTemplatesService)
        {
            this.terminalTemplatesService = terminalTemplatesService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(TerminalTemplatesSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(TerminalTemplatesSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalTemplatesSummary>>> GetTerminalTemplates()// TODO: Add filters & pagination, actual data
        {
            var response = new SummariesResponse<TerminalTemplatesSummary> { NumberOfRecords = 1 };

            response.Data = new List<TerminalTemplatesSummary>
            {
                new TerminalTemplatesSummary { Label = "default", TerminalTemplateID = 1 }
            };

            return Ok(response);
        }

    }
}
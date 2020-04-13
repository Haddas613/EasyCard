using AutoMapper;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/dictionaries")]
    [ApiController]
    public class DictionariesApiController : ApiControllerBase
    {
        private readonly IMapper mapper;
        private readonly IExternalSystemsService externalSystemsService;

        public DictionariesApiController(IMapper mapper, IExternalSystemsService externalSystemsService)
        {
            this.mapper = mapper;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpGet]
        [Route("externalsystems")]
        public async Task<ActionResult<SummariesResponse<ExternalSystemSummary>>> GetExternalSystems()
        {
            var exSystems = externalSystemsService.ExternalSystems;

            var response = new SummariesResponse<ExternalSystemSummary> { NumberOfRecords = exSystems.Count, Data = mapper.Map<IEnumerable<ExternalSystemSummary>>(exSystems) };

            return Ok(response);
        }
    }
}

using AutoMapper;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Models;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/dictionaries")]
    [ApiController]
    public class DictionariesApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;

        public DictionariesApiController(ITerminalsService terminalsService, IMapper mapper)
        {
            this.terminalsService = terminalsService;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("externalsystems")]
        public async Task<ActionResult<SummariesResponse<ExternalSystemSummary>>> GetExternalSystems()
        {
            var query = terminalsService.GetExternalSystems();

            var response = new SummariesResponse<ExternalSystemSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<ExternalSystemSummary>(query).ToListAsync();

            return Ok(response);
        }
    }
}

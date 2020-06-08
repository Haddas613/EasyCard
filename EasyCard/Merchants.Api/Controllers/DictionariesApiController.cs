using AutoMapper;
using Merchants.Api.Models.Dictionaries;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Services;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/dictionaries")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
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
            var exSystems = externalSystemsService.GetExternalSystems();

            var response = new SummariesResponse<ExternalSystemSummary> { NumberOfRecords = exSystems.Count(), Data = mapper.Map<IEnumerable<ExternalSystemSummary>>(exSystems) };

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<MerchantsDictionaries>> GetTerminals([FromQuery]string language)
        {
            var dictionaries = DictionariesService.GetDictionaries(language);

            return Ok(dictionaries);
        }
    }
}

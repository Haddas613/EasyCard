using AutoMapper;
using Merchants.Api.Models.Dictionaries;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Services;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
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

        //[ResponseCache(VaryByQueryKeys = new string[] { "showForTemplatesOnly" }, Duration = 3600)]
        [HttpGet]
        [Route("externalsystems")]
        public async Task<ActionResult<SummariesResponse<ExternalSystemSummary>>> GetExternalSystems(bool showForTemplatesOnly = false)
        {
            var exSystems = externalSystemsService.GetExternalSystems();

            if (showForTemplatesOnly)
            {
                exSystems = exSystems.Where(s => s.CanBeUsedInTerminalTemplate);
            }

            var response = new SummariesResponse<ExternalSystemSummary> { NumberOfRecords = exSystems.Count(), Data = mapper.Map<IEnumerable<ExternalSystemSummary>>(exSystems) };

            return Ok(response);
        }

        [HttpGet]
        [Route("merchant")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<MerchantsDictionaries>> GetDictionaries()
        {
            var dictionaries = DictionariesService.GetDictionaries(CurrentCulture);

            return Ok(dictionaries);
        }
    }
}

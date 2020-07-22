using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using System.Threading.Tasks;
using Transactions.Api.Models.Dictionaries;
using Transactions.Api.Services;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/dictionaries")]
    [ApiController]
    public class DictionariesApiController : ApiControllerBase
    {
        private readonly IMapper mapper;

        public DictionariesApiController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("transaction")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 3600)]
        public async Task<ActionResult<TransactionsDictionaries>> GetTransactionDictionaries([FromQuery]string language)
        {
            var dictionaries = DictionariesService.GetDictionaries(language);
            return Ok(dictionaries);
        }
    }
}

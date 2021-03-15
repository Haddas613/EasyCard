using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Localization;
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
        public async Task<ActionResult<TransactionsDictionaries>> GetTransactionDictionaries()
        {
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture?.Culture;

            var dictionaries = DictionariesService.GetDictionaries(culture);
            return Ok(dictionaries);
        }
    }
}

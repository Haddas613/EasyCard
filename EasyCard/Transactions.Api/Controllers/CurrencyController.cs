using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Transactions.Api.Models.Currency;
using SharedApi = Shared.Api;

namespace Transactions.Api.Controllers
{
    [Route("api/currency")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class CurrencyController : ApiControllerBase
    {
        private readonly ICurrencyRateService currencyRateService;

        public CurrencyController(ICurrencyRateService currencyRateService)
        {
            this.currencyRateService = currencyRateService;
        }

        [HttpPost]
        public async Task<ActionResult<OperationResponse>> UpdateCurrencyRates([FromBody]CurrencyRateUpdateRequest request)
        {
            var result = new OperationResponse { Message = "OK", Status = SharedApi.Models.Enums.StatusEnum.Success };

            var newRate = new Merchants.Business.Entities.Billing.CurrencyRate
            {
                Currency = request.Currency,
                Rate = request.Rate,
                Date = request.Date
            };

            await currencyRateService.CreateOrUpdate(newRate);

            return Ok(result);
        }
    }
}
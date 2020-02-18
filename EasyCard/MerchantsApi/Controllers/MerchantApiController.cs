using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantsApi.Business.Services;
using MerchantsApi.Models.Merchant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MerchantsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/merchant")]
    [ApiController]
    public class MerchantApiController : ControllerBase
    {
        private readonly IMerchantsService merchantsService;

        public MerchantApiController(IMerchantsService merchantsService)
        {
            this.merchantsService = merchantsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<MerchantSummary>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> GetMerchants([FromQuery]MerchantsFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MerchantResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(OperationResponse))]
        [Route("{merchantID}")]
        public async Task<IActionResult> GetMerchant([FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<MerchantHistory>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(OperationResponse))]
        [Route("{merchantID}/history")]
        public async Task<IActionResult> GetMerchantHistory([FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateMerchant([FromBody]MerchantRequest merchant)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(OperationResponse))]
        [Route("{merchantID}")]
        public async Task<IActionResult> UpdateMerchant([FromRoute]long merchantID, [FromBody]UpdateMerchantRequest merchant)
        {
            throw new NotImplementedException();
        }

    }
}
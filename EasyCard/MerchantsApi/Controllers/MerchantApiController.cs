using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantsApi.Business.Services;
using MerchantsApi.Models.Merchant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(m => m.MerchantID == merchantID);

            if (merchant == null)
                return NotFound(new OperationResponse($"Merchant {merchantID} not found", Shared.Models.Enums.StatusEnum.Error));

            //TODO: Automapper
            return new JsonResult(new MerchantResponse { MerchantID = merchant.MerchantID, BusinessName = merchant.BusinessName }) { StatusCode = 200 };
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
            //todo: Automapper
            var entity = new Business.Entities.Merchant();
            entity.BusinessName = merchant.BusinessName;
            entity.Created = DateTime.UtcNow;

            merchantsService.AddMerchant(entity);
            await merchantsService.SaveChanges();//todo: <- Return this

            return new JsonResult(new OperationResponse("ok", Shared.Models.Enums.StatusEnum.Success, entity.MerchantID)) { StatusCode = 201 };
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
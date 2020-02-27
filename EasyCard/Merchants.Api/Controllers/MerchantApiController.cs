using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Merchant;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Extensions;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/merchant")]
    [ApiController]
    public class MerchantApiController : ApiControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;

        public MerchantApiController(IMerchantsService merchantsService, IMapper mapper)
        {
            this.merchantsService = merchantsService;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<MerchantSummary>))]
        public async Task<IActionResult> GetMerchants([FromQuery]MerchantsFilter filter)
        {
            var query = merchantsService.GetMerchants().Filter(filter);

            var response = new SummariesResponse<MerchantSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<MerchantSummary>(query.ApplyPagination(filter)).ToListAsync();

            return new JsonResult(response) { StatusCode = 200 };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MerchantResponse))]
        [Route("{merchantID}")]
        public async Task<IActionResult> GetMerchant([FromRoute]long merchantID)
        {
            var merchant = await mapper.ProjectTo<MerchantResponse>(merchantsService.GetMerchants())
                .FirstOrDefaultAsync(m => m.MerchantID == merchantID).EnsureExists();

            return new JsonResult(merchant) { StatusCode = 200 };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummariesResponse<MerchantHistory>))]
        [Route("{merchantID}/history")]
        public async Task<IActionResult> GetMerchantHistory([FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<IActionResult> CreateMerchant([FromBody]MerchantRequest merchant)
        {
            var newMerchant = mapper.Map<Merchant>(merchant);
            await merchantsService.CreateEntity(newMerchant);

            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, newMerchant.MerchantID)) { StatusCode = 201 };
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("{merchantID}")]
        public async Task<IActionResult> UpdateMerchant([FromRoute]long merchantID, [FromBody]UpdateMerchantRequest model)
        {
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == merchantID).EnsureExists();

            mapper.Map(model, merchant);

            await merchantsService.UpdateEntity(merchant);

            return new JsonResult(new OperationResponse("ok", StatusEnum.Success, merchantID)) { StatusCode = 201 };
        }

    }
}
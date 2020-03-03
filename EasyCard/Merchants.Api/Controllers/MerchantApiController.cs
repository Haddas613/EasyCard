using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Merchant;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Services;
using Merchants.Shared;
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
    [Consumes("application/json")]
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
        public async Task<ActionResult<SummariesResponse<MerchantSummary>>> GetMerchants([FromQuery]MerchantsFilter filter)
        {
            var query = merchantsService.GetMerchants().Filter(filter);

            var response = new SummariesResponse<MerchantSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<MerchantSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }

        [HttpGet]
        [Route("{merchantID}")]
        public async Task<ActionResult<MerchantResponse>> GetMerchant([FromRoute]long merchantID)
        {
            var merchant = await mapper.ProjectTo<MerchantResponse>(merchantsService.GetMerchants())
                .FirstOrDefaultAsync(m => m.MerchantID == merchantID).EnsureExists();

            if (merchant == null)
            {
                return new NotFoundObjectResult(new { });
            }

            return Ok(merchant);
        }

        [HttpGet]
        [Route("{merchantID}/history")]
        public async Task<IActionResult> GetMerchantHistory([FromRoute]long merchantID)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateMerchant([FromBody]MerchantRequest merchant)
        {
            var newMerchant = mapper.Map<Merchant>(merchant);
            await merchantsService.CreateEntity(newMerchant);

            return CreatedAtAction(nameof(GetMerchant), new { merchantID = newMerchant.MerchantID }, new OperationResponse(Messages.MerchantCreated, StatusEnum.Success, newMerchant.MerchantID));
        }

        [HttpPut]
        [Route("{merchantID}")]
        public async Task<ActionResult<OperationResponse>> UpdateMerchant([FromRoute]long merchantID, [FromBody]UpdateMerchantRequest model)
        {
            var merchant = await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == merchantID).EnsureExists();

            mapper.Map(model, merchant);

            await merchantsService.UpdateEntity(merchant);

            return Ok(new OperationResponse(Messages.MerchantUpdated, StatusEnum.Success, merchantID));
        }
    }
}
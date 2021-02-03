using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Extensions;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/merchant")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class MerchantApiController : ApiControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;

        public MerchantApiController(IMerchantsService merchantsService, IMapper mapper, ITerminalsService terminalsService)
        {
            this.merchantsService = merchantsService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(MerchantSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(MerchantSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<MerchantSummary>>> GetMerchants([FromQuery]MerchantsFilter filter)
        {
            // TODO: validate filters (see transactions list)

            var query = merchantsService.GetMerchants().AsNoTracking().Filter(filter);

            using (var dbTransaction = merchantsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<MerchantSummary> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(Merchant.MerchantID), filter.OrderByDirection).ApplyPagination(filter);

                response.Data = await mapper.ProjectTo<MerchantSummary>(query).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{merchantID}")]
        public async Task<ActionResult<MerchantResponse>> GetMerchant([FromRoute]Guid merchantID)
        {
            using (var dbTransaction = merchantsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbMerchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(m => m.MerchantID == merchantID));

                var merchant = mapper.Map<MerchantResponse>(dbMerchant);

                merchant.Terminals = await mapper.ProjectTo<Models.Terminal.TerminalSummary>(terminalsService.GetTerminals().AsNoTracking().Where(d => d.MerchantID == dbMerchant.MerchantID)).ToListAsync();
                merchant.Users = await mapper.ProjectTo<UserSummary>(merchantsService.GetMerchantUsers(merchant.MerchantID)).ToListAsync();

                return Ok(merchant);
            }
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
        public async Task<ActionResult<OperationResponse>> UpdateMerchant([FromRoute]Guid merchantID, [FromBody]UpdateMerchantRequest model)
        {
            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == merchantID));

            mapper.Map(model, merchant);

            await merchantsService.UpdateEntity(merchant);

            return Ok(new OperationResponse(Messages.MerchantUpdated, StatusEnum.Success, merchantID));
        }
    }
}
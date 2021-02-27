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
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Extensions;
using Z.EntityFramework.Plus;

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
        private readonly ApplicationSettings config;
        private readonly IImpersonationService impersonationService;

        public MerchantApiController(
            IMerchantsService merchantsService,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<ApplicationSettings> config,
            IImpersonationService impersonationService)
        {
            this.merchantsService = merchantsService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.config = config.Value;
            this.impersonationService = impersonationService;
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

            var query = merchantsService.GetMerchants().Filter(filter);

            using (var dbTransaction = merchantsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = query.DeferredCount().FutureValue();

                var response = new SummariesResponse<MerchantSummary>();

                query = query.OrderByDynamic(filter.SortBy ?? nameof(Merchant.MerchantID), filter.SortDesc).ApplyPagination(filter);

                response.Data = await mapper.ProjectTo<MerchantSummary>(query).Future().ToListAsync();

                response.NumberOfRecords = numberOfRecords.Value;

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

                merchant.Terminals = await mapper.ProjectTo<Models.Terminal.TerminalSummary>(terminalsService.GetTerminals().Where(d => d.MerchantID == dbMerchant.MerchantID)).ToListAsync();
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

        [HttpPost]
        [Route("{merchantID:guid}/loginAsMerchant")]
        public async Task<ActionResult<OperationResponse>> LoginAsMerchant([FromRoute]Guid merchantID)
        {
            var updateResponse = await impersonationService.LoginAsMerchant(merchantID);

            if (updateResponse.Status != StatusEnum.Success)
            {
                return new ObjectResult(updateResponse) { StatusCode = 400 };
            }

            updateResponse.Message = config.MerchantProfileURL;

            return new ObjectResult(updateResponse) { StatusCode = 200 };
        }
    }
}
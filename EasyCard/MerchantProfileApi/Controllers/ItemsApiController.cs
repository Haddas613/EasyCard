using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MerchantProfileApi.Extensions;
using MerchantProfileApi.Models.Billing;
using MerchantProfileApi.Models.Terminal;
using Merchants.Business.Entities.Billing;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;

namespace MerchantProfileApi.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/items")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class ItemsApiController : ApiControllerBase
    {
        private readonly IItemsService itemsService;
        private readonly IMapper mapper;

        public ItemsApiController(IItemsService itemsService, IMapper mapper)
        {
            this.itemsService = itemsService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<ItemSummary>>> GetItems([FromQuery] ItemsFilter filter)
        {
            var query = itemsService.GetItems().Filter(filter);

            using (var dbTransaction = itemsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<ItemSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<ItemSummary>(query.ApplyPagination(filter)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{itemID}")]
        public async Task<ActionResult<ItemResponse>> GetItem([FromRoute] Guid itemID)
        {
            using (var dbTransaction = itemsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbItem = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

                var item = mapper.Map<ItemResponse>(dbItem);

                return Ok(item);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateItem([FromBody] ItemRequest model)
        {
            var newItem = mapper.Map<Item>(model);
            await itemsService.CreateEntity(newItem);

            return CreatedAtAction(nameof(GetItem), new { itemID = newItem.ItemID }, new OperationResponse(Messages.ItemCreated, StatusEnum.Success, newItem.ItemID.ToString()));
        }

        [HttpPut]
        [Route("{itemID}")]
        public async Task<ActionResult<OperationResponse>> UpdateItem([FromRoute] Guid itemID, [FromBody] UpdateItemRequest model)
        {
            var item = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

            mapper.Map(model, item);

            await itemsService.UpdateEntity(item);

            return Ok(new OperationResponse(Messages.ItemUpdated, StatusEnum.Success, itemID.ToString()));
        }

        [HttpDelete]
        [Route("{itemID}")]
        public async Task<ActionResult<OperationResponse>> DeleteItem([FromRoute] Guid itemID)
        {
            var item = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

            item.Active = false;

            await itemsService.UpdateEntity(item);

            return Ok(new OperationResponse(Messages.ItemDeleted, StatusEnum.Success, itemID.ToString()));
        }
    }
}

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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.IO;
using Shared.Helpers.Security;
using Z.EntityFramework.Plus;

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
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IMerchantsService merchantsService;
        private readonly ICurrencyRateService currencyRateService;
        private readonly ITerminalsService terminalsService;
        private readonly BasicServices.Services.IExcelService excelService;

        public ItemsApiController(
            IItemsService itemsService,
            IMapper mapper,
            IHttpContextAccessorWrapper httpContextAccessor,
            IMerchantsService merchantsService,
            ICurrencyRateService currencyRateService,
            BasicServices.Services.IExcelService excelService,
            ITerminalsService terminalsService)
        {
            this.itemsService = itemsService;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.merchantsService = merchantsService;
            this.currencyRateService = currencyRateService;
            this.terminalsService = terminalsService;
            this.excelService = excelService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(ItemSummary)
                    .GetObjectMeta(ItemResource.ResourceManager, CurrentCulture)
            };
        }

        /// <summary>
        /// Get custom selling Items
        /// </summary>
        /// <param name="filter">Search filters</param>
        /// <returns>Items list</returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<ItemSummary>>> GetItems([FromQuery] ItemsFilter filter)
        {
            var query = itemsService.GetItems().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = itemsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<ItemSummary>();

                if (filter.Currency == null)
                {
                    response.Data = await mapper.ProjectTo<ItemSummary>(query.OrderByDescending(i => i.Created).ApplyPagination(filter)).Future().ToListAsync();
                }
                else
                {
                    var rates = await currencyRateService.GetLatestRates(); // TODO: caching
                    var currency = filter.Currency.GetValueOrDefault(CurrencyEnum.ILS);

                    if (filter.TerminalID.HasValue)
                    {
                        var terminal = EnsureExists(await terminalsService.GetTerminal(filter.TerminalID.Value));
                        rates.EURRate = terminal.Settings.EuroRate > 0 ? terminal.Settings.EuroRate : rates.EURRate;
                        rates.USDRate = terminal.Settings.DollarRate > 0 ? terminal.Settings.DollarRate : rates.USDRate;
                    }

                    var data = await query.OrderByDescending(i => i.Created).ApplyPagination(filter).Future().ToListAsync();

                    response.Data = data.Select(d => new ItemSummary
                    {
                        Currency = currency,
                        ItemID = d.ItemID,
                        ItemName = d.ItemName,
                        Price = rates.Convert(d.Currency, d.Price, currency),
                        ExternalReference = d.ExternalReference,
                        WoocommerceID = d.WoocommerceID
                    });
                }

                response.NumberOfRecords = numberOfRecordsFuture.Value;
                return Ok(response);
            }
        }

        /// <summary>
        /// Get Item by ID
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <returns>Item details</returns>
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

        /// <summary>
        /// Create new custom Item
        /// </summary>
        /// <param name="model">New Item details</param>
        /// <returns>Operation result</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateItem([FromBody] ItemRequest model)
        {
            var newItem = mapper.Map<Item>(model);

            newItem.MerchantID = User.GetMerchantID().GetValueOrDefault();

            newItem.ApplyAuditInfo(httpContextAccessor);

            await itemsService.CreateEntity(newItem);

            return CreatedAtAction(nameof(GetItem), new { itemID = newItem.ItemID }, new OperationResponse(Messages.ItemCreated, StatusEnum.Success, newItem.ItemID));
        }

        /// <summary>
        /// Update Item
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <param name="model">Update exusting Item details</param>
        /// <returns>Operation result</returns>
        [HttpPut]
        [Route("{itemID}")]
        public async Task<ActionResult<OperationResponse>> UpdateItem([FromRoute] Guid itemID, [FromBody] UpdateItemRequest model)
        {
            var item = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

            mapper.Map(model, item);

            item.ApplyAuditInfo(httpContextAccessor);

            await itemsService.UpdateEntity(item);

            return Ok(new OperationResponse(Messages.ItemUpdated, StatusEnum.Success, itemID));
        }

        /// <summary>
        /// Delete Item
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <returns>Operation result</returns>
        [HttpDelete]
        [Route("{itemID}")]
        public async Task<ActionResult<OperationResponse>> DeleteItem([FromRoute] Guid itemID)
        {
            var item = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

            item.Active = false;

            item.ApplyAuditInfo(httpContextAccessor);

            await itemsService.UpdateEntity(item);

            return Ok(new OperationResponse(Messages.ItemDeleted, StatusEnum.Success, itemID));
        }

        /// <summary>
        /// Delete sevaral items
        /// </summary>
        /// <param name="ids">IDs of Itms to delete</param>
        /// <returns>Operation result</returns>
        [HttpPost]
        [Route("bulkdelete")]
        public async Task<ActionResult<OperationResponse>> BulkDeleteItems([FromBody] List<Guid> ids)
        {
            int deletedCount = 0;

            foreach (var itemID in ids)
            {
                var item = EnsureExists(await itemsService.GetItems().FirstOrDefaultAsync(m => m.ItemID == itemID));

                item.Active = false;

                item.ApplyAuditInfo(httpContextAccessor);

                await itemsService.UpdateEntity(item);

                deletedCount++;
            }

            return Ok(new OperationResponse(Messages.ItemsDeletedCnt?.Replace("{count}", deletedCount.ToString()), StatusEnum.Success));
        }

        [HttpGet]
        [Route("$excel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<SummariesResponse<ItemSummary>>> GetItemsExcel([FromQuery] ItemsFilter filter)
        {
            var query = itemsService.GetItems().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = itemsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                List<ItemSummary> items = null;

                if (filter.Currency == null)
                {
                    items = await mapper.ProjectTo<ItemSummary>(query.OrderByDescending(i => i.Created).ApplyPagination(filter)).Future().ToListAsync();
                }
                else
                {
                    var rates = await currencyRateService.GetLatestRates(); // TODO: caching
                    var currency = filter.Currency.GetValueOrDefault(CurrencyEnum.ILS);

                    if (filter.TerminalID.HasValue)
                    {
                        var terminal = EnsureExists(await terminalsService.GetTerminal(filter.TerminalID.Value));
                        rates.EURRate = terminal.Settings.EuroRate > 0 ? terminal.Settings.EuroRate : rates.EURRate;
                        rates.USDRate = terminal.Settings.DollarRate > 0 ? terminal.Settings.DollarRate : rates.USDRate;
                    }

                    var data = await query.OrderByDescending(i => i.Created).ApplyPagination(filter).Future().ToListAsync();

                    items = data.Select(d => new ItemSummary
                    {
                        Currency = currency,
                        ItemID = d.ItemID,
                        ItemName = d.ItemName,
                        Price = rates.Convert(d.Currency, d.Price, currency),
                        ExternalReference = d.ExternalReference,
                        WoocommerceID = d.WoocommerceID
                    }).ToList();
                }

                var mapping = ItemResource.ResourceManager.GetExcelColumnNames<ItemSummary>();

                var filename = FileNameHelpers.RemoveIllegalFilenameCharacters($"Items-{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}.xlsx");
                var res = await excelService.GenerateFile($"{User.GetMerchantID()}/{filename}", "Invoices", items, mapping);

                return Ok(new OperationResponse { Status = StatusEnum.Success, EntityReference = res });
            }
        }
    }
}

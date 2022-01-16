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
using Microsoft.Extensions.Logging;
using ProfileApi;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Security;
using Shared.Helpers.Security;
using Transactions.Api.Client;
using Z.EntityFramework.Plus;

namespace MerchantProfileApi.Controllers
{
    /// <summary>
    /// End-customers reference
    /// </summary>
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/consumers")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class ConsumersApiController : ApiControllerBase
    {
        private readonly IConsumersService consumersService;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ITransactionsApiClient transactionsApiClient;
        private readonly ILogger logger;

        public ConsumersApiController(
            IConsumersService consumersService,
            IMapper mapper,
            ITerminalsService terminalsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ITransactionsApiClient transactionsApiClient,
            ILogger<ConsumersApiController> logger)
        {
            this.consumersService = consumersService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.httpContextAccessor = httpContextAccessor;
            this.transactionsApiClient = transactionsApiClient;
            this.logger = logger;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(ConsumerSummary)
                    .GetObjectMeta(ItemResource.ResourceManager, CurrentCulture)
            };
        }

        /// <summary>
        /// End-customers list
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<ConsumerSummary>>> GetConsumers([FromQuery] ConsumersFilter filter)
        {
            var merchantID = User.GetMerchantID();

            var query = consumersService.GetConsumers().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = consumersService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<ConsumerSummary>();

                response.Data = await mapper.ProjectTo<ConsumerSummary>(query.ApplyPagination(filter)).Future().ToListAsync();
                response.NumberOfRecords = numberOfRecordsFuture.Value;

                return Ok(response);
            }
        }

        /// <summary>
        /// End-customer details
        /// </summary>
        /// <param name="consumerID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{consumerID}")]
        public async Task<ActionResult<ConsumerResponse>> GetConsumer([FromRoute] Guid consumerID)
        {
            using (var dbTransaction = consumersService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbConsumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID));

                var consumer = mapper.Map<ConsumerResponse>(dbConsumer);

                return Ok(consumer);
            }
        }

        /// <summary>
        /// Create end-customer record
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateConsumer([FromBody] ConsumerRequest model)
        {
            var newConsumer = mapper.Map<Consumer>(model);

            // NOTE: this is security assignment
            //mapper.Map(terminal, newConsumer);

            newConsumer.MerchantID = User.GetMerchantID().GetValueOrDefault();

            newConsumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.CreateEntity(newConsumer);

            return CreatedAtAction(nameof(GetConsumer), new { consumerID = newConsumer.ConsumerID }, new OperationResponse(Messages.ConsumerCreated, StatusEnum.Success, newConsumer.ConsumerID));
        }

        /// <summary>
        /// Update end-customer details
        /// </summary>
        /// <param name="consumerID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{consumerID}")]
        public async Task<ActionResult<OperationResponse>> UpdateConsumer([FromRoute] Guid consumerID, [FromBody] UpdateConsumerRequest model)
        {
            var consumer = EnsureConcurrency(EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID)), model);

            mapper.Map(model, consumer);

            consumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerUpdated, StatusEnum.Success, consumerID));
        }

        /// <summary>
        /// Delete end-customer record
        /// </summary>
        /// <param name="consumerID"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{consumerID}")]
        public async Task<ActionResult<OperationResponse>> DeleteConsumer([FromRoute] Guid consumerID)
        {
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID));

            consumer.Active = false;

            var consumerDataDeleteResponse = await transactionsApiClient.DeleteConsumerRelatedData(consumerID);

            if (consumerDataDeleteResponse.Status == StatusEnum.Error)
            {
                logger.LogError($"{nameof(DeleteConsumer)}: {consumerID} ERROR. Details: {consumerDataDeleteResponse.Message}");
            }

            consumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerDeleted, StatusEnum.Success, consumerID));
        }

        /// <summary>
        /// Delete customers
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("bulkdelete")]
        public async Task<ActionResult<OperationResponse>> BulkDeleteConsumers([FromBody] List<Guid> ids)
        {
            int deletedCount = 0;

            foreach (var consumerID in ids)
            {
                var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID));

                consumer.Active = false;

                var consumerDataDeleteResponse = await transactionsApiClient.DeleteConsumerRelatedData(consumerID);

                if (consumerDataDeleteResponse.Status == StatusEnum.Error)
                {
                    logger.LogError($"{nameof(BulkDeleteConsumers)}: {consumerID} ERROR. Details: {consumerDataDeleteResponse.Message}");
                    continue;
                }

                consumer.ApplyAuditInfo(httpContextAccessor);

                await consumersService.UpdateEntity(consumer);

                deletedCount++;
            }

            return Ok(new OperationResponse(Messages.ConsumersDeletedCnt?.Replace("{count}", deletedCount.ToString()), StatusEnum.Success));
        }
    }
}

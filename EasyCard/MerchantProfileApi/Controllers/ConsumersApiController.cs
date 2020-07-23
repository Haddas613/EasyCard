﻿using System;
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
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Security;
using Shared.Helpers.Security;

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

        public ConsumersApiController(IConsumersService consumersService, IMapper mapper, ITerminalsService terminalsService, IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.consumersService = consumersService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(ConsumerSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(ItemResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
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

            using (var dbTransaction = consumersService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<ConsumerSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<ConsumerSummary>(query.ApplyPagination(filter)).ToListAsync();

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
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == model.TerminalID));

            var newConsumer = mapper.Map<Consumer>(model);

            // NOTE: this is security assignment
            mapper.Map(terminal, newConsumer);

            newConsumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.CreateEntity(newConsumer);

            return CreatedAtAction(nameof(GetConsumer), new { consumerID = newConsumer.ConsumerID }, new OperationResponse(Messages.ConsumerCreated, StatusEnum.Success, newConsumer.ConsumerID.ToString()));
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

            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == consumer.TerminalID));

            mapper.Map(model, consumer);

            consumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerUpdated, StatusEnum.Success, consumerID.ToString()));
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

            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == consumer.TerminalID));

            consumer.Active = false;

            consumer.ApplyAuditInfo(httpContextAccessor);

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerDeleted, StatusEnum.Success, consumerID.ToString()));
        }
    }
}
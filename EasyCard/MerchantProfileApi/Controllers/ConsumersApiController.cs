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
    [Route("api/consumers")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class ConsumersApiController : ApiControllerBase
    {
        private readonly IConsumersService consumersService;
        private readonly IMapper mapper;

        public ConsumersApiController(IConsumersService consumersService, IMapper mapper)
        {
            this.consumersService = consumersService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<ConsumerSummary>>> GetConsumers([FromQuery] ConsumersFilter filter)
        {
            var query = consumersService.GetConsumers().Filter(filter);

            using (var dbTransaction = consumersService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<ConsumerSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<ConsumerSummary>(query.ApplyPagination(filter)).ToListAsync();

                return Ok(response);
            }
        }

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateConsumer([FromBody] ConsumerRequest model)
        {
            var newConsumer = mapper.Map<Consumer>(model);
            await consumersService.CreateEntity(newConsumer);

            return CreatedAtAction(nameof(GetConsumer), new { consumerID = newConsumer.ConsumerID }, new OperationResponse(Messages.ConsumerCreated, StatusEnum.Success, newConsumer.ConsumerID.ToString()));
        }

        [HttpPut]
        [Route("{consumerID}")]
        public async Task<ActionResult<OperationResponse>> UpdateConsumer([FromRoute] Guid consumerID, [FromBody] UpdateConsumerRequest model)
        {
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID));

            mapper.Map(model, consumer);

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerUpdated, StatusEnum.Success, consumerID.ToString()));
        }

        [HttpDelete]
        [Route("{consumerID}")]
        public async Task<ActionResult<OperationResponse>> DeleteConsumer([FromRoute] Guid consumerID)
        {
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(m => m.ConsumerID == consumerID));

            consumer.Active = false;

            await consumersService.UpdateEntity(consumer);

            return Ok(new OperationResponse(Messages.ConsumerDeleted, StatusEnum.Success, consumerID.ToString()));
        }
    }
}

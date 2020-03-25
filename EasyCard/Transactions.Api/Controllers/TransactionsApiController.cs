using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared.Enums;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transactions")]
    [ApiController]
    public class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransactionsApiController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
        }

        [HttpGet]
        [Route("{transactionID}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] long transactionID)
        {
            var transaction = mapper.Map<TransactionResponse>(EnsureExists(await transactionsService.GetTransactions()
                .FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));

            return Ok(transaction);
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetTransactions([FromQuery] TransactionsFilter filter)
        {
            var query = transactionsService.GetTransactions().Filter(filter);

            var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<TransactionSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("create/withtoken")]
        public async Task<ActionResult<OperationResponse>> CreateTransactionWithToken([FromBody] TransactionRequestWithToken model)
        {
            // TODO: business vlidators (not only model validation)

            var token = EnsureExists(await keyValueStorage.Get(model.CardToken), "CardToken");

            return await ProcessTransaction(model, mapper.Map<ProcessTransactionOptions>(token), nameof(CreateTransactionWithToken));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("create/withcc")]
        public async Task<ActionResult<OperationResponse>> CreateTransactionWithCreditCard([FromBody] TransactionRequestWithCreditCard model)
        {
            // TODO: business vlidators (not only model validation)

            var transactionOptions = new ProcessTransactionOptions
            {
                CreditCardSecureDetails = model.CreditCardSecureDetails,
                TerminalID = model.TerminalID, //TODO: check claims
                MerchantID = 1, //TODO
            };

            return await ProcessTransaction(model, transactionOptions, nameof(CreateTransactionWithCreditCard));
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(TransactionRequest model, ProcessTransactionOptions transactionOptions, string actionName)
        {
            // TODO: get terminalID from token
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync()); // TODO: 403

            var aggregator = aggregatorResolver.GetAggregator(terminal);
            var processor = processorResolver.GetProcessor(terminal);

            var transaction = mapper.Map<PaymentTransaction>(model);

            await transactionsService.CreateEntity(transaction);

            // create transaction in aggregator (Clearing House)
            try
            {
                var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);

                var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);

                if (!aggregatorResponse.Success)
                {
                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse(aggregatorResponse.ErrorMessage, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
                }

                // TODO: use converter instead of mapper (?)
                mapper.Map(aggregatorResponse, transaction);

                // TODO: change transaction status
                await transactionsService.UpdateEntity(transaction);
            }
            catch (Exception ex)
            {
                // TODO: can be 403
                // TODO: update transaction
                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
            }

            // create transaction in processor (Shva)
            try
            {
                var processorRequest = mapper.Map<ProcessorTransactionRequest>(transaction);

                var processorMessageID = Guid.NewGuid().ToString();

                var processorResponse = await processor.CreateTransaction(processorRequest, processorMessageID, GetCorrelationID() /*TODO: update integration message*/);

                if (!processorResponse.Success)
                {
                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse(processorResponse.ErrorMessage, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
                }

                // TODO: use converter instead of mapper (?)
                mapper.Map(processorResponse, transaction);

                // TODO: change transaction status

                await transactionsService.UpdateEntity(transaction);
            }
            catch (Exception ex)
            {
                // TODO: can be 403
                // TODO: update transaction
                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
            }

            // commit transaction in aggregator (Clearing House)
            try
            {
                var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);

                if (!commitAggregatorResponse.Success)
                {
                    // In case of failed commit, transaction should not be transmitted to Shva

                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse(commitAggregatorResponse.ErrorMessage, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
                }

                // TODO: use converter instead of mapper (?)
                mapper.Map(commitAggregatorResponse, transaction);

                // TODO: change transaction status

                await transactionsService.UpdateEntity(transaction);
            }
            catch (Exception ex)
            {
                // TODO: can be 403
                // TODO: update transaction
                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.TransactionNumber)); // TODO: convert message
            }

            return CreatedAtAction(actionName, new OperationResponse("ok", StatusEnum.Success, transaction.TransactionNumber));
        }
    }
}
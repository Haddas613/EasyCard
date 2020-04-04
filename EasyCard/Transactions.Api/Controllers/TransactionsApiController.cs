using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Validation;
using Shared.Business.Exceptions;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using SharedBusiness = Shared.Business;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transactions")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransactionsApiController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService, ILogger<TransactionsApiController> logger)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("{transactionID}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] Guid transactionID)
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

        /// <summary>
        /// Create the charge based on credit card or previously stored credit card token (J4 deal)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("create")]
        [ValidateModelState]
        [SwaggerRequestExample(typeof(CreateTransactionRequest), typeof(CreateTransactionRequestExample))]
        public async Task<ActionResult<OperationResponse>> CreateTransaction([FromBody] CreateTransactionRequest model)
        {
            // TODO: validate that credit card details should be absent
            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(model, token, nameof(CreateTransaction));
            }
            else
            {
                return await ProcessTransaction(model, null, nameof(CreateTransaction));
            }
        }

        /// <summary>
        /// Blocking funds on credit card (J5 deal)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("blocking")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> BlockCreditCard([FromBody] BlockCreditCardRequest model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if credit card is valid
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("checking")]
        public async Task<ActionResult<OperationResponse>> CheckCreditCard([FromBody] CheckCreditCardRequest model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refund request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("refund")]
        public async Task<ActionResult<OperationResponse>> Refund([FromBody] RefundRequest model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initial request for set of billing trnsactions
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("initalBillingDeal")]
        public async Task<ActionResult<OperationResponse>> InitalBillingDeal([FromBody] InitalBillingDealRequest model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Billing deal
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("nextBillingDeal")]
        public async Task<ActionResult<OperationResponse>> NextBillingDeal([FromBody] NextBillingDealRequest model)
        {
            throw new NotImplementedException();
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(CreateTransactionRequest model, CreditCardTokenKeyVault token, string actionName)
        {
            //var terminalID = User.GetTerminalID();

            // TODO: caching
            // TODO: redo ".Include(t => t.Integrations)"
            var terminal = SecureExists(await terminalsService.GetTerminals().Where(d => d.TerminalID == model.TerminalID).Include(t => t.Integrations).ThenInclude(d => d.ExternalSystem).FirstOrDefaultAsync());

            var transaction = mapper.Map<PaymentTransaction>(model);
            mapper.Map(terminal, transaction);

            if (token != null)
            {
                if (token.TerminalID != terminal.TerminalID)
                {
                    throw new SecurityException(SharedBusiness.Messages.ApiMessages.YouHaveNoAccess);
                }

                mapper.Map(token, transaction.CreditCardDetails);
            }
            else
            {
                mapper.Map(model.CreditCardSecureDetails, transaction.CreditCardDetails);
            }

            var terminalAggregator = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.ExternalSystem.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                Messages.AggregatorNotDefined);

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.ExternalSystem.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            var processor = processorResolver.GetProcessor(terminalProcessor);

            transaction.Calculate();

            //transaction.ProcessorTerminalID = "123456"; // TODO: map from terminal settings

            await transactionsService.CreateEntity(transaction);

            // create transaction in aggregator (Clearing House)
            try
            {
                var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);

                aggregatorRequest.AggregatorSettings = terminalAggregator.Settings;

                var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);

                if (!aggregatorResponse.Success)
                {
                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, aggregatorResponse.Errors));
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
                logger.LogError(ex, "Aggregator Create Transaction request failed");

                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.PaymentTransactionID.ToString())); // TODO: convert message
            }

            // TODO: cc vendorm isTourist

            // create transaction in processor (Shva)
            try
            {
                var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

                if (token != null)
                {
                    mapper.Map(token, processorRequest.CreditCardToken);
                }
                else
                {
                    mapper.Map(model.CreditCardSecureDetails, processorRequest.CreditCardToken);
                }

                processorRequest.ProcessorSettings = terminalProcessor.Settings; //new Shva.Configuration.ShvaTerminalSettings { MerchantNumber = "0882021014", UserName = "ABLCH", Password = "E9900C" }; // TODO: get from terminal

                var processorMessageID = Guid.NewGuid().ToString();

                var processorResponse = await processor.CreateTransaction(processorRequest, processorMessageID, GetCorrelationID() /*TODO: update integration message*/);

                if (!processorResponse.Success)
                {
                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse(processorResponse.ErrorMessage, StatusEnum.Error, transaction.PaymentTransactionID.ToString())); // TODO: convert message
                }

                // TODO: use converter instead of mapper (?)
                mapper.Map(processorResponse, transaction);

                // TODO: change transaction status

                await transactionsService.UpdateEntity(transaction);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Processor Create Transaction request failed");
                // TODO: can be 403
                // TODO: update transaction
                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.PaymentTransactionID.ToString())); // TODO: convert message
            }

            // commit transaction in aggregator (Clearing House)
            try
            {
                var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                var chSettings = new ClearingHouse.ClearingHouseTerminalSettings() { MerchantReference = "5eb62fca-a37b-4192-b7f1-e75784561682" }; // TODO: get from terminal
                commitAggregatorRequest.AggregatorSettings = chSettings;

                var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);

                if (!commitAggregatorResponse.Success)
                {
                    // In case of failed commit, transaction should not be transmitted to Shva

                    // TODO: can be 403
                    // TODO: update transaction
                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, commitAggregatorResponse.Errors));
                }

                // TODO: use converter instead of mapper (?)
                mapper.Map(commitAggregatorResponse, transaction);

                // TODO: change transaction status

                await transactionsService.UpdateEntity(transaction);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Aggregator Commit Transaction request failed");
                // TODO: can be 403
                // TODO: update transaction
                return BadRequest(new OperationResponse(ex.Message, StatusEnum.Error, transaction.PaymentTransactionID.ToString())); // TODO: convert message
            }

            return CreatedAtAction(actionName, new OperationResponse(Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID.ToString()));
        }
    }
}
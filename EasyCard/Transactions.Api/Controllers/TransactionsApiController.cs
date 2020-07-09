﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Exceptions;
using Shared.Business.Extensions;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Api.Validation;
using Transactions.Api.Validation.Options;
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
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;
        private readonly ApplicationSettings appSettings;

        private readonly ITerminalsService terminalsService;

        public TransactionsApiController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService, ILogger<TransactionsApiController> logger,
            IOptions<ApplicationSettings> appSettings)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.appSettings = appSettings.Value;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(TransactionSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(TransactionSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        [Route("{transactionID}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] Guid transactionID)
        {
            Debug.WriteLine(User);

            var transaction = mapper.Map<TransactionResponse>(EnsureExists(await transactionsService.GetTransactions()
                .FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));

            return Ok(transaction);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{transactionID}/history")]
        public async Task<ActionResult<TransactionResponse>> GetTransactionHistory([FromRoute] Guid transactionID)
        {
            var transaction = EnsureExists(await transactionsService.GetTransactions()
                .Where(m => m.PaymentTransactionID == transactionID).Select(d => d.PaymentTransactionID).FirstOrDefaultAsync());

            var query = transactionsService.GetTransactionHistory(transactionID);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<Models.Transactions.TransactionHistory> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<Models.Transactions.TransactionHistory>(query).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetTransactions([FromQuery] TransactionsFilter filter)
        {
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            TransactionsFilterValidator.ValidateFilter(filter, new TransactionFilterValidationOptions { MaximumPageSize = appSettings.FiltersGlobalPageSizeLimit });

            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.OrderByDirection).ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit);

                // TODO: validate generated sql
                var sql = query.ToSql();

                // TODO: try to remove ProjectTo
                response.Data = await mapper.ProjectTo<TransactionSummary>(query).ToListAsync();

                return Ok(response);
            }
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
            Debug.WriteLine(User);
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: validate that credit card details should be absent
            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(model, token);
            }
            else
            {
                return await ProcessTransaction(model, null);
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
            var transaction = mapper.Map<CreateTransactionRequest>(model);

            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(transaction, token, JDealTypeEnum.J5);
            }
            else
            {
                return await ProcessTransaction(transaction, null, JDealTypeEnum.J5);
            }
        }

        /// <summary>
        /// Check if credit card is valid
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("checking")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> CheckCreditCard([FromBody] CheckCreditCardRequest model)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(model);

            return await ProcessTransaction(transaction, null, JDealTypeEnum.J2);
        }

        /// <summary>
        /// Refund request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("refund")]
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> Refund([FromBody] RefundRequest model)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(model);

            if (!string.IsNullOrWhiteSpace(model.CreditCardToken))
            {
                var token = EnsureExists(await keyValueStorage.Get(model.CreditCardToken), "CreditCardToken");
                return await ProcessTransaction(transaction, token, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
            else
            {
                return await ProcessTransaction(transaction, null, JDealTypeEnum.J4, SpecialTransactionTypeEnum.Refund);
            }
        }

        /// <summary>
        /// Initial request for set of billing trnsactions
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        [Route("initalBillingDeal")]
        [ValidateModelState]
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
        [ValidateModelState]
        public async Task<ActionResult<OperationResponse>> NextBillingDeal([FromBody] NextBillingDealRequest model)
        {
            throw new NotImplementedException();
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialDealID = null)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, token, jDealType, specialTransactionType, initialDealID);

            var transaction = mapper.Map<PaymentTransaction>(model);
            mapper.Map(terminal, transaction);

            transaction.SpecialTransactionType = specialTransactionType;
            transaction.JDealType = jDealType;

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

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            var terminalAggregator = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                Messages.AggregatorNotDefined);

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            transaction.AggregatorID = terminalAggregator.ExternalSystemID;
            transaction.ProcessorID = terminalProcessor.ExternalSystemID;

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            var processor = processorResolver.GetProcessor(terminalProcessor);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);
            mapper.Map(processorSettings, transaction);

            transaction.Calculate();

            await transactionsService.CreateEntity(transaction);

            // create transaction in aggregator (Clearing House)
            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;

                    var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByAggregator, rejectionMessage: aggregatorResponse.ErrorMessage, rejectionReason: aggregatorResponse.RejectReasonCode);

                        return BadRequest(new OperationResponse($"{Messages.RejectedByAggregator}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, aggregatorResponse.Errors));
                    }

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByAggregator);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Aggregator Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            BadRequestObjectResult processorFailedRsponse = null;

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

                processorRequest.ProcessorSettings = processorSettings;

                var processorResponse = await processor.CreateTransaction(processorRequest);
                mapper.Map(processorResponse, transaction);

                if (!processorResponse.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial,  rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                    processorFailedRsponse = BadRequest(new OperationResponse($"{Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, processorResponse.Errors));
                }

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Processor Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, TransactionFinalizationStatusEnum.Initial, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                processorFailedRsponse = BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
            }

            // reject to clearing house in case of shva error
            if (processorFailedRsponse != null && aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;
                    aggregatorRequest.RejectionReason = TransactionStatusEnum.FailedToConfirmByProcesor.ToString();

                    var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        logger.LogError($"Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier));
                    }

                    await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                    return processorFailedRsponse;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            // commit transaction in aggregator (Clearing House)
            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);

                    var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                    commitAggregatorRequest.AggregatorSettings = terminalAggregator.Settings;

                    var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);
                    mapper.Map(commitAggregatorResponse, transaction);

                    if (!commitAggregatorResponse.Success)
                    {
                        // NOTE: In case of failed commit, transaction should not be transmitted to Shva

                        logger.LogError($"Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                        return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, commitAggregatorResponse.Errors));
                    }

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CommitedByAggregator);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID.ToString(), HttpContext.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, new OperationResponse(Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID.ToString()));
        }
    }
}
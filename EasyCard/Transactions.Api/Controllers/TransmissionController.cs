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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Validation;
using Shared.Business.Security;
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
using Transactions.Api.Validation;
using Transactions.Api.Validation.Options;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transmission")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class TransmissionController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;
        private readonly ApplicationSettings appSettings;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransmissionController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService, ILogger<TransactionsApiController> logger,
            IOptions<ApplicationSettings> appSettings, IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get transactions which are not transmitted yet
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetNotTransmittedTransactions([FromQuery] TransmissionFilter filter)
        {
            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter).Where(d => d.Status == TransactionStatusEnum.CommitedByAggregator);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.OrderByDirection);

                response.Data = await mapper.ProjectTo<TransactionSummary>(query.ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpPost]
        [Route("transmit")]
        public async Task<ActionResult<SummariesResponse<TransmitTransactionResponse>>> TransmitTransactions(TransmitTransactionsRequest transmitTransactionsRequest)
        {
            if (transmitTransactionsRequest.PaymentTransactionIDs == null || transmitTransactionsRequest.PaymentTransactionIDs.Count() == 0)
            {
                return BadRequest(new OperationResponse(Messages.TransactionsForTransmissionRequired, null, HttpContext.TraceIdentifier, nameof(transmitTransactionsRequest.PaymentTransactionIDs), Messages.TransactionsForTransmissionRequired));
            }

            if (transmitTransactionsRequest.PaymentTransactionIDs.Count() > appSettings.TransmissionMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize), null, HttpContext.TraceIdentifier, nameof(transmitTransactionsRequest.PaymentTransactionIDs), string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize)));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(transmitTransactionsRequest.TerminalID));

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            var processor = processorResolver.GetProcessor(terminalProcessor);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);

            IEnumerable<TransmissionInfo> transactionsToTransmit = null;

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                transactionsToTransmit = await transactionsService.StartTransmission(terminal.TerminalID, transmitTransactionsRequest.PaymentTransactionIDs, dbTransaction);
                await dbTransaction.CommitAsync();
            }

            var processorIds = transactionsToTransmit.Select(d => d.ShvaTranRecord).ToList();

            if (processorIds.Count == 0)
            {
                return BadRequest(new OperationResponse(Messages.ThereAreNoTransactionsToTransmit, StatusEnum.Error));
            }

            var processorRequest = new ProcessorTransmitTransactionsRequest { TransactionIDs = processorIds, ProcessorSettings = processorSettings, CorrelationId = GetCorrelationID() };

            var processorResponse = await processor.TransmitTransactions(processorRequest);

            var response = new SummariesResponse<TransmitTransactionResponse>();
            var transactionsResponse = new List<TransmitTransactionResponse>();

            foreach (var transactionID in transmitTransactionsRequest.PaymentTransactionIDs)
            {
                var preparedTransaction = transactionsToTransmit.FirstOrDefault(d => d.PaymentTransactionID == transactionID);

                if (preparedTransaction != null)
                {
                    var failedTransaction = processorResponse.FailedTransactions?.FirstOrDefault(d => d == preparedTransaction.ShvaTranRecord);

                    if (failedTransaction != null)
                    {
                        transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.TransmissionFailed, PaymentTransactionID = transactionID });
                    }
                    else
                    {
                        transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.Transmitted, PaymentTransactionID = transactionID });
                    }
                }
                else
                {
                    transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.NotFoundOrInvalidStatus, PaymentTransactionID = transactionID });
                }
            }

            response.NumberOfRecords = transactionsResponse.Count;
            response.Data = transactionsResponse;

            var transmissionDate = DateTime.UtcNow;

            // TODO: use with batch or with queue
            var transactionIDs = transactionsResponse.Where(d => d.TransmissionStatus == TransmissionStatusEnum.TransmissionFailed || d.TransmissionStatus == TransmissionStatusEnum.Transmitted).Select(d => d.PaymentTransactionID).ToList();
            var transactions = await transactionsService.GetTransactions().Where(d => transactionIDs.Contains(d.PaymentTransactionID)).ToListAsync();

            foreach (var transaction in transactions)
            {
                var preparedTransaction = transactionsToTransmit.FirstOrDefault(d => d.PaymentTransactionID == transaction.PaymentTransactionID);
                if (preparedTransaction != null)
                {
                    var failedTransaction = processorResponse.FailedTransactions?.FirstOrDefault(d => d == preparedTransaction.ShvaTranRecord);

                    if (failedTransaction != null)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.TransmissionToProcessorFailed);

                        // TODO: cancel in clearing house
                    }
                    else
                    {
                        transaction.ShvaTransactionDetails.TransmissionDate = transmissionDate;
                        transaction.ShvaTransactionDetails.ManuallyTransmitted = httpContextAccessor.GetUser().IsMerchant();

                        // TODO: Transmission ID from Shva
                        transaction.ShvaTransactionDetails.ShvaTransmissionNumber = processorResponse.TransmissionReference;
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.TransmittedByProcessor);
                    }
                }
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("cancel")]
        public async Task<ActionResult<OperationResponse>> CancelNotTransmittedTransaction(CancelTransmissionRequest cancelTransmissionRequest)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(cancelTransmissionRequest.TerminalID));

            var terminalAggregator = ValidateExists(
               terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
               Messages.AggregatorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);

            PaymentTransaction transaction = null;

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                transaction = EnsureExists(await transactionsService.GetTransactions()
                 .FirstOrDefaultAsync(m => m.PaymentTransactionID == cancelTransmissionRequest.PaymentTransactionID && m.TerminalID == cancelTransmissionRequest.TerminalID));

                if (transaction.Status != TransactionStatusEnum.CommitedByAggregator)
                {
                    return BadRequest(new OperationResponse(Messages.TransactionStatusIsNotValid, StatusEnum.Error));
                }

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CancelledByMerchant, dbTransaction: dbTransaction);

                await dbTransaction.CommitAsync();
            }

            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                await transactionsService.ReloadEntity(transaction);

                // cancel in clearing house
                try
                {
                    var aggregatorTransaction = await aggregator.GetTransaction(transaction.ClearingHouseTransactionDetails?.ClearingHouseTransactionID?.ToString()); // TODO: abstract aggregator
                    mapper.Map(aggregatorTransaction, transaction);

                    var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;
                    aggregatorRequest.RejectionReason = TransactionStatusEnum.CancelledByMerchant.ToString();

                    var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        logger.LogError($"Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, transaction.Status, TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, HttpContext.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), aggregatorResponse.ErrorMessage));
                    }

                    await transactionsService.UpdateEntityWithStatus(transaction, transaction.Status, TransactionFinalizationStatusEnum.CanceledByAggregator);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, transaction.Status, TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                    return BadRequest(new OperationResponse($"{Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, HttpContext.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), null));
                }
            }

            return new OperationResponse(Messages.TransactionCanceled, StatusEnum.Success);
        }
    }
}
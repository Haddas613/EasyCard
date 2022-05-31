using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Merchants.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Queue;
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
using Z.EntityFramework.Plus;

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
        private readonly IShvaTerminalsService shvaTerminalsService;
        private readonly IQueue transmissionQueue;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransmissionController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService, ILogger<TransactionsApiController> logger,
            IOptions<ApplicationSettings> appSettings, IHttpContextAccessorWrapper httpContextAccessor, IShvaTerminalsService shvaTerminalsService, IQueueResolver queueResolver)
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
            this.shvaTerminalsService = shvaTerminalsService;
            this.transmissionQueue = queueResolver.GetQueue(QueueResolver.TransmissionQueue);
        }

        /// <summary>
        /// Get transactions which are not transmitted yet
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetNotTransmittedTransactions([FromQuery] TransmissionFilter filter)
        {
            var query = transactionsService.GetTransactions()
                .Filter(filter);

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

                query = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.SortDesc);

                response.Data = await mapper.ProjectTo<TransactionSummary>(query.ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Authorize(Policy = Policy.AnyAdmin)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("nontransmittedtransactionterminals")]
        public async Task<ActionResult<IEnumerable<Guid>>> GetNonTransmittedTransactionsTerminals()
        {
            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                //TODO: Check with terminal settings
                var allTerminals = await transactionsService.GetTransactions().Where(d => d.Status == TransactionStatusEnum.AwaitingForTransmission)
                    .Select(t => t.TerminalID).Distinct().ToListAsync();

                if (allTerminals.Count == 0)
                {
                    return Ok(allTerminals);
                }

                var terminalSettings = await terminalsService.GetTerminals().Where(t => allTerminals.Contains(t.TerminalID)).ToDictionaryAsync(k => k.TerminalID, v => v.Settings);

                var filteredTransactions = new List<Guid>();
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone);

                foreach (var t in allTerminals)
                {
                    if (terminalSettings.ContainsKey(t))
                    {
                        var schedule = terminalSettings[t]?.TransmissionSchedule;
                        schedule = null; // Temporary disabled partial schedule
                        if (schedule == null || schedule.Value.ScheduleApply(now))
                        {
                            filteredTransactions.Add(t);
                        }
                    }
                }

                return Ok(filteredTransactions);
            }
        }

        /// <summary>
        /// Transmit transactions
        /// </summary>
        /// <param name="transmitTransactionsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("transmit")]
        public async Task<ActionResult<SummariesResponse<TransmitTransactionResponse>>> TransmitTransactions(TransmitTransactionsRequest transmitTransactionsRequest)
        {
            if (transmitTransactionsRequest.PaymentTransactionIDs == null || transmitTransactionsRequest.PaymentTransactionIDs.Count() == 0)
            {
                return BadRequest(new OperationResponse(Messages.TransactionsForTransmissionRequired, null, httpContextAccessor.TraceIdentifier, nameof(transmitTransactionsRequest.PaymentTransactionIDs), Messages.TransactionsForTransmissionRequired));
            }

            if (transmitTransactionsRequest.PaymentTransactionIDs.Count() > appSettings.TransmissionMaxBatchSize)
            {
                return BadRequest(new OperationResponse(string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize), null, httpContextAccessor.TraceIdentifier, nameof(transmitTransactionsRequest.PaymentTransactionIDs), string.Format(Messages.TransmissionLimit, appSettings.TransmissionMaxBatchSize)));
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(transmitTransactionsRequest.TerminalID));

            IEnumerable<TransmissionInfo> transactionsToTransmit = null;

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                transactionsToTransmit = await transactionsService.StartTransmission(terminal.TerminalID, transmitTransactionsRequest.PaymentTransactionIDs, dbTransaction);
                await dbTransaction.CommitAsync();
            }

            transactionsToTransmit = transactionsToTransmit.Where(d => d.TranRecord != null && d.ShvaTerminalID != null).ToList();

            var processorIds = transactionsToTransmit.Select(d => d.TranRecord).Where(d => d != null).ToList();

            if (processorIds.Count == 0)
            {
                return BadRequest(new OperationResponse(Messages.ThereAreNoTransactionsToTransmit, StatusEnum.Error));
            }

            var response = new SummariesResponse<TransmitTransactionResponse>();
            var transactionsResponse = new List<TransmitTransactionResponse>();

            foreach (var shvaTerminalID in transactionsToTransmit.Select(t => t.ShvaTerminalID).Where(s => s != null).Distinct())
            {
                await TransmitInternal(transmitTransactionsRequest, terminal, transactionsToTransmit, processorIds, response, transactionsResponse, shvaTerminalID);
            }

            return Ok(response);
        }

        private async Task TransmitInternal(TransmitTransactionsRequest transmitTransactionsRequest, Merchants.Business.Entities.Terminal.Terminal terminal, IEnumerable<TransmissionInfo> transactionsToTransmit, List<string> processorIds, SummariesResponse<TransmitTransactionResponse> response, List<TransmitTransactionResponse> transactionsResponse, string shvaTerminalID)
        {
            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            var processor = processorResolver.GetProcessor(terminalProcessor);

            var shvaTerminalSettings = await shvaTerminalsService.GetShvaTerminal(shvaTerminalID);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(
                terminalProcessor,
                shvaTerminalSettings != null ? JObject.FromObject(shvaTerminalSettings) : terminalProcessor.Settings);

            var processorRequest = new ProcessorTransmitTransactionsRequest
            {
                TransactionIDs = processorIds,
                ProcessorSettings = processorSettings,
                CorrelationId = GetCorrelationID(),
                TerminalID = terminal.TerminalID
            };

            var processorResponse = await processor.TransmitTransactions(processorRequest);

            foreach (var transactionID in transmitTransactionsRequest.PaymentTransactionIDs)
            {
                var preparedTransaction = transactionsToTransmit.FirstOrDefault(d => d.PaymentTransactionID == transactionID);

                if (preparedTransaction != null)
                {
                    var failedTransaction = processorResponse.FailedTransactions?.FirstOrDefault(d => d == preparedTransaction.ShvaDealID);

                    if (failedTransaction != null)
                    {
                        transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.TransmissionFailed, PaymentTransactionID = transactionID });
                        logger.LogError($"TransmissionFailed for transaction: {transactionID}");
                    }
                    else
                    {
                        transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.Transmitted, PaymentTransactionID = transactionID });
                        logger.LogInformation($"Transaction Transmitted: {transactionID}");
                    }
                }
                else
                {
                    transactionsResponse.Add(new TransmitTransactionResponse { TransmissionStatus = TransmissionStatusEnum.NotFoundOrInvalidStatus, PaymentTransactionID = transactionID });
                    logger.LogError($"NotFoundOrInvalidStatus for transaction: {transactionID}");
                }
            }

            response.NumberOfRecords = transactionsResponse.Count;
            response.Data = transactionsResponse;

            var transmissionDate = DateTime.UtcNow;

            // TODO: use with batch or with queue
            var transactionIDs = transactionsResponse.Where(d => d.TransmissionStatus == TransmissionStatusEnum.TransmissionFailed || d.TransmissionStatus == TransmissionStatusEnum.Transmitted).Select(d => d.PaymentTransactionID).ToList();
            var transactions = await transactionsService.GetTransactionsForUpdate().Where(d => transactionIDs.Contains(d.PaymentTransactionID)).ToListAsync();

            foreach (var transaction in transactions)
            {
                var preparedTransaction = transactionsToTransmit.FirstOrDefault(d => d.PaymentTransactionID == transaction.PaymentTransactionID);
                if (preparedTransaction != null)
                {
                    var failedTransaction = processorResponse.FailedTransactions?.FirstOrDefault(d => d == preparedTransaction.ShvaDealID);

                    if (failedTransaction != null)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.TransmissionToProcessorFailed);

                        // TODO: cancel in clearing house - but - it is possible to retry transmission
                        // TODO: cancel invoice
                    }
                    else
                    {
                        transaction.ShvaTransactionDetails.TransmissionDate = transmissionDate;
                        transaction.ShvaTransactionDetails.ManuallyTransmitted = httpContextAccessor.GetUser().IsMerchant();
                        transaction.ShvaTransactionDetails.ShvaTransmissionNumber = processorResponse.TransmissionReference;

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed, transactionOperationCode: TransactionOperationCodesEnum.TransmittedByProcessor);
                    }
                }
            }

            _ = NotifyAggregator(terminal, transactionsResponse, transmissionDate, transactions);
        }

        private async Task NotifyAggregator(Merchants.Business.Entities.Terminal.Terminal terminal, List<TransmitTransactionResponse> transactionsResponse, DateTime transmissionDate, List<PaymentTransaction> transactions)
        {
            try
            {
                ClearingHouse.ClearingHouseAggregator clearingHouseAggregator = null;

                var terminalAggregator = terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator);

                if (terminalAggregator != null && terminalAggregator.ExternalSystemID == 10) //TODO: constants?
                {
                    clearingHouseAggregator = aggregatorResolver.GetAggregator(terminalAggregator) as ClearingHouse.ClearingHouseAggregator;
                }

                //Notify ClearingHouse about successfully transmitted transactions
                if (clearingHouseAggregator != null)
                {
                    var transmittedTransactionsId = transactionsResponse.Where(t => t.TransmissionStatus == TransmissionStatusEnum.Transmitted).Select(t => t.PaymentTransactionID).ToList();
                    var transmittedCHTransactions = transactions
                        .Where(t => t.ClearingHouseTransactionDetails?.ClearingHouseTransactionID.HasValue == true && transmittedTransactionsId.Contains(t.PaymentTransactionID))
                        .Select(t => t.ClearingHouseTransactionDetails.ClearingHouseTransactionID.Value)
                        .ToList();

                    if (transmittedCHTransactions.Count > 0)
                    {
                        await clearingHouseAggregator.UpdateTransmission(transmissionDate, transmittedCHTransactions);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"ClearingHouseTransmit: Error {e.Message}");
            }
        }

        // TODO: reuse CancelNotTransmittedTransactionInternal
        [HttpPost]
        [Route("cancel")]
        public async Task<ActionResult<OperationResponse>> CancelNotTransmittedTransaction(CancelTransmissionRequest cancelTransmissionRequest)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(cancelTransmissionRequest.TerminalID));

            var terminalAggregator = ValidateExists(
               terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
               Messages.AggregatorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

            if (!aggregator.AllowTransmissionCancellation())
            {
                return BadRequest(new OperationResponse(Messages.FailedToCancelByAggregator, StatusEnum.Error));
            }

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);

            PaymentTransaction transaction = null;

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                transaction = EnsureExists(await transactionsService.GetTransactionsForUpdate()
                 .FirstOrDefaultAsync(m => m.PaymentTransactionID == cancelTransmissionRequest.PaymentTransactionID && m.TerminalID == cancelTransmissionRequest.TerminalID));

                // TODO: reuse helpers
                if (transaction.Status != TransactionStatusEnum.AwaitingForTransmission)
                {
                    return BadRequest(new OperationResponse(Messages.TransactionStatusIsNotValid, StatusEnum.Error));
                }

                if (transaction.InvoiceID.HasValue)
                {
                    return BadRequest(new OperationResponse(Messages.CannotCancelInvoicedTransaction, StatusEnum.Error));
                }

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CancelledByMerchant, dbTransaction: dbTransaction);

                await dbTransaction.CommitAsync();
            }

            // TODO: remove invoice

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

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("transmitByTerminal/{terminalID:guid}")]
        public async Task<ActionResult<OperationResponse>> TransmitByTerminal([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            if (terminal.Status == Merchants.Shared.Enums.TerminalStatusEnum.Disabled)
            {
                return new OperationResponse
                {
                    Status = StatusEnum.Error,
                    Message = $"Terminal is disabled"
                };
            }

            var trans = await transactionsService.GetTransactions()
                .Filter(new TransmissionFilter { TerminalID = terminalID })
                .Select(d => d.PaymentTransactionID).ToListAsync();

            var numberOfRecords = trans.Count();

            for (int i = 0; i < numberOfRecords; i += appSettings.TransmissionMaxBatchSize)
            {
                var req = new TransmitTransactionsRequest
                {
                    PaymentTransactionIDs = trans.Skip(i).Take(appSettings.TransmissionMaxBatchSize),
                    TerminalID = terminalID
                };

                await transmissionQueue.PushToQueue(req);
            }

            return Ok(new OperationResponse { Message = Messages.TransactionsQueued.Replace("@count", numberOfRecords.ToString()), Status = StatusEnum.Success });
        }

        // TODO: db-transaction
        internal async Task<ActionResult<OperationResponse>> CancelNotTransmittedTransactionInternal(Merchants.Business.Entities.Terminal.Terminal terminal, PaymentTransaction transaction, IAggregator aggregator, object aggregatorSettings)
        {
            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CancelledByMerchant);

            // TODO: remove invoice

            if (aggregator != null && aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
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
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

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransmissionController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
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

        /// <summary>
        /// Get transactions which are not transmitted yet
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetNotTransmittedTransactions([FromQuery] TransactionsFilter filter)
        {
            TransactionsFilterValidator.ValidateFilter(filter, new TransactionFilterValidationOptions { MaximumPageSize = appSettings.FiltersGlobalPageSizeLimit });

            var query = transactionsService.GetTransactions().AsNoTracking().Filter(filter);

            var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

            query = query.OrderByDynamic(filter.SortBy ?? nameof(PaymentTransaction.PaymentTransactionID), filter.OrderByDirection);

            response.Data = await mapper.ProjectTo<TransactionSummary>(query.ApplyPagination(filter, appSettings.FiltersGlobalPageSizeLimit)).ToListAsync();

            return Ok(response);
        }

        [HttpPost]
        [Route("transmit")]
        public async Task<ActionResult<OperationResponse>> TransmitTransactions(TransmitTransactionsRequest transmitTransactionsRequest)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(transmitTransactionsRequest.TerminalID));

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Messages.ProcessorNotDefined);

            var processor = processorResolver.GetProcessor(terminalProcessor);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);

            var query = transactionsService.GetTransactions(); //.Where(t => /*t.TerminalID == transmitTransactionsRequest.TerminalID &&*/ t.Status == TransactionStatusEnum.CommitedToAggregator);

            // TODO: check if not all transactions can be transmitted
            if (transmitTransactionsRequest.PaymentTransactionIDs?.Count() > 0)
            {
                query = query.Where(t => transmitTransactionsRequest.PaymentTransactionIDs.Contains(t.PaymentTransactionID));
            }

            //query = query.Take(appSettings.FiltersGlobalPageSizeLimit);

            var res = query.ToList();

            var updated = query.UpdateFromQuery(x => new PaymentTransaction { Status = TransactionStatusEnum.TransmissionInProgress });

            // TODO: update status to TransmissionInProgress

            // TODO: common deal id
            var processorIds = await query.Select(d => d.ShvaTransactionDetails.ShvaDealID).ToListAsync();

            var processorRequest = new ProcessorTransmitTransactionsRequest { TransactionIDs = processorIds, ProcessorSettings = processorSettings, CorrelationId = GetCorrelationID() };

            var processorRespnse = await processor.TransmitTransactions(processorRequest);

            // TODO: list of transmitted transactions
            return new OperationResponse(Messages.TransactionCreated, StatusEnum.Success);
        }

        [HttpDelete]
        [Route("cancel")]
        public async Task<ActionResult<OperationResponse>> CancelNotTransmittedTransaction(TransmitTransactionsRequest cancelTransmissionRequest)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(cancelTransmissionRequest.TerminalID));

            var terminalAggregator = ValidateExists(
               terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
               Messages.AggregatorNotDefined);

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);

            //using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            //{
            //    var transaction = EnsureExists(await transactionsService.GetTransactions()
            //     .FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID));

            //    if (transaction.Status != TransactionStatusEnum.CommitedToAggregator)
            //    {
            //        return BadRequest(new OperationResponse(Messages.TransactionStatusIsNotValid, StatusEnum.Error));
            //    }

            //    // TODO: clearing house cancellation

            //    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.CancelledByMerchant);

                
            //}

            return new OperationResponse(Messages.TransactionCreated, StatusEnum.Success);
        }
    }
}
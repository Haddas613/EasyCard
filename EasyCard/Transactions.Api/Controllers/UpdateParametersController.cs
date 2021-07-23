using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Entities.Terminal;
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
using Transactions.Api.Models.UpdateParameters;
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
    [Route("api/update")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UpdateParametersController : ApiControllerBase
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
        private readonly IQueue updateParametersQueue;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public UpdateParametersController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
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
            updateParametersQueue = queueResolver.GetQueue(QueueResolver.UpdateTerminalSHVAParametersQueue);
        }

        [HttpPost]
        [Route("send-to-queue")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<SendTerminalsToQueueResponse>> SendTerminalsToUpdateParametersQueue()
        {
            var response = new SummariesResponse<Guid>();

            var terminals = await terminalsService.GetTerminals()
                .Where(t => t.Status != Merchants.Shared.Enums.TerminalStatusEnum.Disabled)
                .Select(t => t.TerminalID)
                .ToListAsync();

            var batch = new List<Guid>(appSettings.UpdateParametersTerminalsMaxBatchSize);

            for (int i = 0; i < terminals.Count; i += appSettings.UpdateParametersTerminalsMaxBatchSize)
            {
                await updateParametersQueue.PushToQueue(
                    terminals.Skip(i).Take(appSettings.UpdateParametersTerminalsMaxBatchSize));
            }

            return new SendTerminalsToQueueResponse { Status = StatusEnum.Success, Message = Messages.TransactionsQueued, Count = terminals.Count };
        }

        [HttpPost]
        [Route("updateByTerminal/{terminalID:guid}")]
        public async Task<ActionResult<UpdateParametersResponse>> UpdateParameters([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            TerminalExternalSystem terminalPinpadProcessor = null;
            bool terminalPinpadAllow = false;

            try
            {
                terminalPinpadProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.PinpadProcessor),
                Transactions.Shared.Messages.ProcessorNotDefined);
                terminalPinpadAllow = true;
            }
            catch (Exception)
            {

            }

            IProcessor pinpadProcessor = null;
            if (terminalPinpadAllow)
            {
                pinpadProcessor = processorResolver.GetProcessor(terminalPinpadProcessor);
            }

            var terminalProcessor = ValidateExists(
                    terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                    Messages.ProcessorNotDefined);

            var processor = processorResolver.GetProcessor(terminalProcessor);
            var processorSettings = processorResolver.GetProcessorTerminalSettings(
                terminalProcessor,
                terminalProcessor.Settings);

            object pinpadProcessorSettings = null;
            if (terminalPinpadAllow)
            {
                pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
            }

            var processorRequest = new ProcessorUpdateParametersRequest { TerminalID = terminalID, ProcessorSettings = processorSettings, CorrelationId = GetCorrelationID() };
            var pinpadProcessorRequest = new ProcessorUpdateParametersRequest { TerminalID = terminalID, ProcessorSettings = pinpadProcessorSettings, CorrelationId = GetCorrelationID() };
          
            await processor.ParamsUpdateTransaction(processorRequest); //todo implement it in emulator

            if (terminalPinpadAllow)
            {
                await pinpadProcessor.ParamsUpdateTransaction(pinpadProcessorRequest);
            }
           
            return new UpdateParametersResponse
            { TerminalID = terminalID, UpdateStatus = UpdateParamsStatusEnum.Updated };
        }
    }
}
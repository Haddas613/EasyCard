using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Models.Integrations.Shva;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Integration.Models;
using Shva;
using Transactions.Api.Services;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/shva")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class ShvaApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly IProcessorResolver processorResolver;
        private readonly IMapper mapper;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IExternalSystemsService externalSystemsService;
        public ShvaApiController(ITerminalsService terminalsService, IProcessorResolver processorResolver,
             IMapper mapper, ISystemSettingsService systemSettingsService,
             IExternalSystemsService externalSystemsService)
        {
            this.mapper = mapper;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.systemSettingsService = systemSettingsService;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<ActionResult<OperationResponse>> ChangePassword(ChangePasswordRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var terminalProcessor = ValidateExists(
               terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
               Transactions.Shared.Messages.ProcessorNotDefined);

            //  var transaction = mapper.Map<PaymentTransaction>(request);// mapper.Map<PaymentTransaction>(model);
            var processor = processorResolver.GetProcessor(terminalProcessor);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);

            //mapper.Map(processorSettings, request.ExternalSystem);

            var processorRequest = mapper.Map<ProcessorChangePasswordRequest>(request); //TODO!!!
            processorRequest.ProcessorSettings = processorSettings;
            var processorResponse = await ((Shva.ShvaProcessor)processor).ChangePassword(processorRequest);

            var existProcessorSettings = terminal.Integrations.First(x => x.Type == Shared.Enums.ExternalSystemTypeEnum.Processor);

            //var externalSystems = externalSystemsService.GetExternalSystems().ToDictionary(d => d.ExternalSystemID);
            //var texternalSystem = new TerminalExternalSystem();
            //mapper.Map(request, externalSystems);//AutoMapperProfile:41

            if (!processorResponse.Success)
            {
                //texternalSystem.Settings
                // .UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                //processorFailedRsponse = BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
            }
            else
            {
                ShvaTerminalSettings terminalSettings = existProcessorSettings.Settings.ToObject<ShvaTerminalSettings>();
                terminalSettings.Password = request.NewPassword;
                existProcessorSettings.Settings = JObject.FromObject(terminalSettings);
                await terminalsService.SaveTerminalExternalSystem(existProcessorSettings, terminal);
            }

            return Ok(new OperationResponse(ShvaMessagesResource.ChangePasswordSetSuccessfully, StatusEnum.Success));
        }

        [HttpPost]
        [Route("new-password")]
        public async Task<ActionResult<OperationResponse>> NewPassword(NewPasswordRequest request)
        {
            if (request.TerminalID.HasValue)
            {
                //TODO process terminal
            }
            else if (request.TerminalTemplateID.HasValue)
            {
                //TODO process terminal template
            }
            else
            {
                return BadRequest(ShvaMessagesResource.EitherTerminalOrTerminalTemplateIDMustBeSpecified);
            }

            return Ok(new OperationResponse(ShvaMessagesResource.NewPasswordSetSuccessfully, StatusEnum.Success));
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            return Ok(new OperationResponse(ShvaMessagesResource.ConnectionSuccess, StatusEnum.Success));
        }
    }
}
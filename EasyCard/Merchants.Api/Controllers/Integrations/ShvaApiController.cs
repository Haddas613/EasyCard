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
using Shared.Integration;
using Shared.Integration.Models;
using Shva;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/shva")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class ShvaApiController : ApiControllerBase
    {
        private readonly ITerminalsService terminalsService;
        private readonly ShvaProcessor shvaProcessor;
        private readonly IMapper mapper;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IExternalSystemsService externalSystemsService;
        private readonly ITerminalTemplatesService terminalTemplatesService;

        public ShvaApiController(
            ITerminalsService terminalsService,
            ShvaProcessor shvaProcessor,
            IMapper mapper,
            ISystemSettingsService systemSettingsService,
            IExternalSystemsService externalSystemsService,
            ITerminalTemplatesService terminalTemplatesService)
        {
            this.mapper = mapper;
            this.shvaProcessor = shvaProcessor;
            this.terminalsService = terminalsService;
            this.systemSettingsService = systemSettingsService;
            this.externalSystemsService = externalSystemsService;
            this.terminalTemplatesService = terminalTemplatesService;
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            //TODO: implement
            return Ok(new OperationResponse(ShvaMessagesResource.ConnectionSuccess, StatusEnum.Success));
        }

        [HttpPost]
        [Route("new-password")]
        public async Task<ActionResult<OperationResponse>> SetNewPassword(ChangePasswordRequest request)
        {
            if (request.TerminalTemplateID.HasValue)
            {
                return await SetNewPasswordForTerminalTemplate(request.TerminalTemplateID.Value, request.NewPassword);
            }
            else
            {
                return await SetNewPasswordForTerminal(request.TerminalID.Value, request.NewPassword);
            }
        }

        private async Task<OperationResponse> SetNewPasswordForTerminal(Guid terminalID, string newPassword)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            var terminalProcessor = EnsureExists(
               terminal.Integrations.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID));

            var processorResponse = await ShvaChangePassword(terminalProcessor, newPassword);

            if (!processorResponse.Success)
            {
                return new OperationResponse(ShvaMessagesResource.CouldNotSetNewPassword, StatusEnum.Error);
            }
            else
            {
                ShvaTerminalSettings terminalSettings = terminalProcessor.Settings.ToObject<ShvaTerminalSettings>();
                terminalSettings.Password = newPassword;
                terminalProcessor.Settings = JObject.FromObject(terminalSettings);
                await terminalsService.SaveTerminalExternalSystem(terminalProcessor, terminal);
            }

            return new OperationResponse(ShvaMessagesResource.NewPasswordSetSuccessfully, StatusEnum.Success);
        }

        private async Task<OperationResponse> SetNewPasswordForTerminalTemplate(long terminalTemplate, string newPassword)
        {
            var terminal = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplate));
            var terminalTemplateExternalSystem = EnsureExists(
               terminal.Integrations.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.ShvaExternalSystemID));

            var terminalProcessor = mapper.Map<TerminalExternalSystem>(terminalTemplateExternalSystem);

            var processorResponse = await ShvaChangePassword(terminalProcessor, newPassword);

            if (!processorResponse.Success)
            {
                return new OperationResponse(ShvaMessagesResource.CouldNotSetNewPassword, StatusEnum.Error);
            }
            else
            {
                ShvaTerminalSettings terminalSettings = terminalTemplateExternalSystem.Settings.ToObject<ShvaTerminalSettings>();
                terminalSettings.Password = newPassword;
                terminalTemplateExternalSystem.Settings = JObject.FromObject(terminalSettings);
                await terminalTemplatesService.SaveTerminalTemplateExternalSystem(terminalTemplateExternalSystem);
            }

            return new OperationResponse(ShvaMessagesResource.NewPasswordSetSuccessfully, StatusEnum.Success);
        }

        private async Task<ProcessorChangePasswordResponse> ShvaChangePassword(TerminalExternalSystem externalSystem, string newPassword)
        {
            var processorSettings = externalSystem.Settings.ToObject<ShvaTerminalSettings>();
            var processorRequest = new ProcessorChangePasswordRequest
            {
                NewPassword = newPassword,
                CorrelationId = GetCorrelationID(),
                ProcessorSettings = processorSettings
            };

            return await shvaProcessor.ChangePassword(processorRequest);
        }
    }
}
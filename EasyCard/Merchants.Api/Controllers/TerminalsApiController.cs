﻿using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Audit;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Merchants.Shared;
using Merchants.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using SharedBusiness = Shared.Business;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/terminals")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)] // TODO: bearer
    [ApiController]
    public class TerminalsApiController : ApiControllerBase
    {
        private readonly IMerchantsService merchantsService;
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;
        private readonly IExternalSystemsService externalSystemsService;
        private readonly IUserManagementClient userManagementClient;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly ITerminalTemplatesService terminalTemplatesService;
        private readonly IFeaturesService featuresService;

        public TerminalsApiController(
            IMerchantsService merchantsService,
            ITerminalsService terminalsService,
            IMapper mapper,
            IExternalSystemsService externalSystemsService,
            IUserManagementClient userManagementClient,
            ISystemSettingsService systemSettingsService,
            ITerminalTemplatesService terminalTemplatesService,
            IFeaturesService featuresService)
        {
            this.merchantsService = merchantsService;
            this.terminalsService = terminalsService;
            this.mapper = mapper;
            this.externalSystemsService = externalSystemsService;
            this.userManagementClient = userManagementClient;
            this.systemSettingsService = systemSettingsService;
            this.terminalTemplatesService = terminalTemplatesService;
            this.featuresService = featuresService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(TerminalSummary)
                    .GetObjectMeta(TerminalSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        [HttpGet]
        [Route("available-integrations")]
        public async Task<ActionResult<Dictionary<string, IEnumerable<ExternalSystem>>>> GetAvailableIntegrations(bool showForTemplatesOnly = false)
        {
            //TODO: translations for keys
            var externalSystems = externalSystemsService.GetExternalSystems().Where(s => s.Active);

            if (showForTemplatesOnly)
            {
                externalSystems = externalSystems.Where(s => s.CanBeUsedInTerminalTemplate);
            }

            return Ok(externalSystems.GroupBy(k => k.Type).ToDictionary(k => k.Key, v => v));
        }

        [HttpGet]
        [Route("available-features")]
        public async Task<ActionResult<FeatureSummary>> GetAvailableFeatures()
        {
            var features = await mapper.ProjectTo<FeatureSummary>(featuresService.GetQuery()).ToListAsync();

            return Ok(features);
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalSummary>>> GetTerminals([FromQuery] TerminalsFilter filter)
        {
            // TODO: validate filters (see transactions list)

            var query = terminalsService.GetTerminals().Filter(filter);

            using (var dbTransaction = terminalsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var numberOfRecords = query.DeferredCount().FutureValue();

                var response = new SummariesResponse<TerminalSummary>();

                query = query.OrderByDynamic(filter.SortBy ?? nameof(Terminal.TerminalID), filter.SortDesc).ApplyPagination(filter);

                response.Data = await mapper.ProjectTo<TerminalSummary>(query).Future().ToListAsync();

                response.NumberOfRecords = numberOfRecords.Value;

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<TerminalResponse>> GetTerminal([FromRoute]Guid terminalID)
        {
            var dbTerminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            var terminal = mapper.Map<TerminalResponse>(dbTerminal);

            // TODO: enable it when user-terminal mappings will be enabled
            // terminal.Users = await mapper.ProjectTo<UserSummary>(terminalsService.GetTerminalUsers(terminal.TerminalID)).ToListAsync();

            var systemSettings = await systemSettingsService.GetSystemSettings();

            mapper.Map(systemSettings, terminal);

            var externalSystems = externalSystemsService.GetExternalSystems().ToDictionary(d => d.ExternalSystemID);

            foreach (var integration in terminal.Integrations)
            {
                if (externalSystems.ContainsKey(integration.ExternalSystemID))
                {
                    integration.ExternalSystem = mapper.Map<ExternalSystemSummary>(externalSystems[integration.ExternalSystemID]);
                }
            }

            return Ok(terminal);
        }

        /// <summary>
        /// Add terminal basic information
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateTerminal([FromBody]TerminalRequest model)
        {
            var merchant = EnsureExists(await merchantsService.GetMerchants().FirstOrDefaultAsync(d => d.MerchantID == model.MerchantID));

            var newTerminal = mapper.Map<Terminal>(model);

            var template = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(model.TerminalTemplateID));

            mapper.Map(template, newTerminal);

            newTerminal.Status = Shared.Enums.TerminalStatusEnum.Approved;

            await terminalsService.CreateEntity(newTerminal);

            return CreatedAtAction(nameof(GetTerminal), new { terminalID = newTerminal.TerminalID }, new OperationResponse(Messages.TerminalCreated, StatusEnum.Success, newTerminal.TerminalID));
        }

        // TODO: concurrency check

        /// <summary>
        /// Ypdates basic terminal information and settings
        /// </summary>
        /// <param name="terminalID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> UpdateTerminal([FromRoute]Guid terminalID, [FromBody]UpdateTerminalRequest model)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            mapper.Map(model, terminal);
            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalID));
        }

        [HttpPut]
        [Route("{terminalID}/externalsystem")]
        public async Task<ActionResult<OperationResponse>> SaveTerminalExternalSystem([FromRoute]Guid terminalID, [FromBody]ExternalSystemRequest model)
        {
            using (var dbTransaction = terminalsService.BeginDbTransaction())
            {
                var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

                var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(model.ExternalSystemID), nameof(ExternalSystem));

                TerminalExternalSystem texternalSystem = (await terminalsService.GetTerminalExternalSystems(terminalID)).FirstOrDefault(es => es.ExternalSystemID == model.ExternalSystemID);

                if (texternalSystem == null)
                {
                    texternalSystem = new TerminalExternalSystem();
                }

                mapper.Map(model, texternalSystem);
                texternalSystem.TerminalID = terminalID;
                texternalSystem.Type = externalSystem.Type;

                if (externalSystem.SettingsTypeFullName != null)
                {
                    var settingsType = Type.GetType(externalSystem.SettingsTypeFullName);

                    if (settingsType == null)
                    {
                        throw new ApplicationException($"Could not create instance of {externalSystem.SettingsTypeFullName}");
                    }

                    var settings = texternalSystem.Settings.ToObject(settingsType);
                    mapper.Map(settings, terminal);
                    await terminalsService.UpdateEntity(terminal, dbTransaction);
                }

                await terminalsService.SaveTerminalExternalSystem(texternalSystem, terminal, dbTransaction);

                await dbTransaction.CommitAsync();
            }

            return Ok(new OperationResponse(Messages.ExternalSystemSaved, StatusEnum.Success, terminalID));
        }

        [HttpDelete]
        [Route("{terminalID}/externalsystem/{externalSystemID}")]
        public async Task<ActionResult<OperationResponse>> DeleteTerminalExternalSystem([FromRoute]Guid terminalID, long externalSystemID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(externalSystemID), nameof(ExternalSystem));

            if (externalSystem.Type == Shared.Enums.ExternalSystemTypeEnum.Aggregator)
            {
                terminal.AggregatorTerminalReference = null;
            }
            else if (externalSystem.Type == Shared.Enums.ExternalSystemTypeEnum.Processor)
            {
                terminal.ProcessorTerminalReference = null;
            }

            await terminalsService.RemoveTerminalExternalSystem(terminalID, externalSystemID);
            await terminalsService.UpdateEntity(terminal);
            return Ok(new OperationResponse(Messages.ExternalSystemRemoved, StatusEnum.Success, terminalID));
        }

        [HttpPut]
        [Route("{terminalID}/switchfeature/{featureID}")]
        public async Task<ActionResult<OperationResponse>> SwitchTerminalFeature([FromRoute]Guid terminalID, [FromRoute]FeatureEnum featureID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));
            var feature = EnsureExists(await featuresService.GetQuery().FirstOrDefaultAsync(f => f.FeatureID == featureID), "Feature");

            await ProcessFeatureInternal(terminal, feature);

            await terminalsService.UpdateEntity(terminal);
            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalID));
        }

        [HttpPost]
        [Route("{terminalID}/resetApiKey")]
        public async Task<ActionResult<OperationResponse>> CreateTerminalApiKey([FromRoute] Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            var opResult = await userManagementClient.CreateTerminalApiKey(new CreateTerminalApiKeyRequest { TerminalID = terminal.TerminalID, MerchantID = terminal.MerchantID });

            // TODO: failed case
            return Ok(new OperationResponse { EntityReference = opResult.ApiKey });
        }

        [HttpPut]
        [Route("{terminalID}/disable")]
        public async Task<ActionResult<OperationResponse>> DisableTerminal([FromRoute]Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            terminal.Status = Shared.Enums.TerminalStatusEnum.Disabled;
            terminal.SharedApiKey = null;

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalDisabled, StatusEnum.Success, terminalID));
        }

        [HttpPut]
        [Route("{terminalID}/enable")]
        public async Task<ActionResult<OperationResponse>> EnableTerminal([FromRoute]Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            terminal.Status = Shared.Enums.TerminalStatusEnum.Approved;

            await terminalsService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalEnabled, StatusEnum.Success, terminalID));
        }

        [HttpPost]
        [Route("{terminalID}/auditResetApiKey/{merchantID}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<OperationResponse>> AuditResetApiKey([FromRoute] Guid terminalID, [FromRoute] Guid merchantID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminals().FirstOrDefaultAsync(m => m.TerminalID == terminalID));

            var auditEntry = new AuditEntryData
            {
                MerchantID = merchantID,
                TerminalID = terminalID,
                OperationCode = SharedBusiness.Audit.OperationCodesEnum.TerminalApiKeyChanged
            };

            await terminalsService.AddAuditEntry(auditEntry);

            return Ok(new OperationResponse { Status = StatusEnum.Success });
        }

        private async Task ProcessFeatureInternal(Terminal terminal, Feature feature)
        {
            if (terminal.EnabledFeatures != null && terminal.EnabledFeatures.Any(f => f == feature.FeatureID))
            {
                terminal.EnabledFeatures.Remove(feature.FeatureID);

                if (feature.FeatureID == FeatureEnum.Api)
                {
                    var opResult = await userManagementClient.DeleteTerminalApiKey(terminal.TerminalID);
                }
            }
            else
            {
                if (terminal.EnabledFeatures == null)
                {
                    terminal.EnabledFeatures = new List<FeatureEnum>();
                }

                if (feature.FeatureID == FeatureEnum.Checkout)
                {
                    //Api must be automatically enabled for checkout
                    if (!terminal.EnabledFeatures.Any(f => f == FeatureEnum.Api))
                    {
                        terminal.EnabledFeatures.Add(FeatureEnum.Api);
                    }
                }

                terminal.EnabledFeatures.Add(feature.FeatureID);
            }
        }
    }
}
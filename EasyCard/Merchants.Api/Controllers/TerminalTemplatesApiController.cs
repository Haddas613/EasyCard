using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Business.Entities.Terminal;
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
using Shared.Helpers.Security;
using Z.EntityFramework.Plus;

namespace Merchants.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/terminal-templates")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)] // TODO: bearer
    [ApiController]
    public class TerminalTemplatesApiController : ApiControllerBase
    {
        private readonly ITerminalTemplatesService terminalTemplatesService;
        private readonly IMapper mapper;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IExternalSystemsService externalSystemsService;
        private readonly IFeaturesService featuresService;

        public TerminalTemplatesApiController(
            ITerminalTemplatesService terminalTemplatesService,
            IMapper mapper,
            ISystemSettingsService systemSettingsService,
            IExternalSystemsService externalSystemsService,
            IFeaturesService featuresService)
        {
            this.terminalTemplatesService = terminalTemplatesService;
            this.mapper = mapper;
            this.systemSettingsService = systemSettingsService;
            this.externalSystemsService = externalSystemsService;
            this.featuresService = featuresService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(TerminalTemplateSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(TerminalTemplatesSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TerminalTemplateSummary>>> GetTerminalTemplates([FromQuery]TerminalTemplatesFilter filter)
        {
            var query = terminalTemplatesService.GetQuery().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var response = new SummariesResponse<TerminalTemplateSummary>();

            response.Data = await mapper.ProjectTo<TerminalTemplateSummary>(query.ApplyPagination(filter)).Future().ToListAsync();
            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }

        [HttpGet]
        [Route("{terminalTemplateID}")]
        public async Task<ActionResult<TerminalTemplateResponse>> GetTerminalTemplate([FromRoute]long terminalTemplateID)
        {
            var dbTemplate = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplateID));

            var template = mapper.Map<TerminalTemplateResponse>(dbTemplate);

            var systemSettings = await systemSettingsService.GetSystemSettings();

            mapper.Map(systemSettings, template);

            var externalSystems = externalSystemsService.GetExternalSystems().ToDictionary(d => d.ExternalSystemID);

            foreach (var integration in template.Integrations)
            {
                if (externalSystems.ContainsKey(integration.ExternalSystemID))
                {
                    integration.ExternalSystem = mapper.Map<ExternalSystemSummary>(externalSystems[integration.ExternalSystemID]);
                }
            }

            return Ok(template);
        }

        /// <summary>
        /// Add terminal template basic information
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateTerminalTemplate([FromBody]TerminalTemplateRequest model)
        {
            var newTemplate = mapper.Map<TerminalTemplate>(model);

            await terminalTemplatesService.CreateEntity(newTemplate);

            return CreatedAtAction(nameof(GetTerminalTemplate), new { terminalTemplateID = newTemplate.TerminalTemplateID }, new OperationResponse(Messages.TerminalTemplateCreated, StatusEnum.Success, newTemplate.TerminalTemplateID.ToString()));
        }

        /// <summary>
        /// Ypdates basic terminal information and settings
        /// </summary>
        /// <param name="terminalTemplateID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{terminalTemplateID}")]
        public async Task<ActionResult<OperationResponse>> UpdateTerminal([FromRoute]long terminalTemplateID, [FromBody]TerminalTemplateRequest model)
        {
            var terminal = EnsureExists(await terminalTemplatesService.GetQuery().FirstOrDefaultAsync(d => d.TerminalTemplateID == terminalTemplateID));

            mapper.Map(model, terminal);

            await terminalTemplatesService.UpdateEntity(terminal);

            return Ok(new OperationResponse(Messages.TerminalUpdated, StatusEnum.Success, terminalTemplateID.ToString()));
        }

        [HttpPut]
        [Route("{terminalTemplateID}/externalsystem")]
        public async Task<ActionResult<OperationResponse>> SaveTerminalExternalSystem([FromRoute]long terminalTemplateID, [FromBody]ExternalSystemRequest model)
        {
            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(model.ExternalSystemID), nameof(ExternalSystem));

            var templateExternalSystem = new TerminalTemplateExternalSystem();

            mapper.Map(model, templateExternalSystem);
            templateExternalSystem.TerminalTemplateID = terminalTemplateID;
            templateExternalSystem.Type = externalSystem.Type;

            await terminalTemplatesService.SaveTerminalTemplateExternalSystem(templateExternalSystem);

            return Ok(new OperationResponse(Messages.ExternalSystemSaved, StatusEnum.Success, terminalTemplateID.ToString()));
        }

        [HttpDelete]
        [Route("{terminalTemplateID}/externalsystem/{externalSystemID}")]
        public async Task<ActionResult<OperationResponse>> DeleteTerminalExternalSystem([FromRoute]long terminalTemplateID, long externalSystemID)
        {
            EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplateID));
            await terminalTemplatesService.RemoveTerminalTemplateExternalSystem(terminalTemplateID, externalSystemID);

            return Ok(new OperationResponse(Messages.ExternalSystemRemoved, StatusEnum.Success, terminalTemplateID.ToString()));
        }

        [HttpPut]
        [Route("{terminalTemplateID}/switchfeature/{featureID}")]
        public async Task<ActionResult<OperationResponse>> SwitchTerminalFeature([FromRoute]long terminalTemplateID, [FromRoute]FeatureEnum featureID)
        {
            var template = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplateID));
            var feature = EnsureExists(await featuresService.GetQuery().FirstOrDefaultAsync(f => f.FeatureID == featureID), "Feature");

            if (template.EnabledFeatures != null && template.EnabledFeatures.Any(f => f == feature.FeatureID))
            {
                template.EnabledFeatures.Remove(featureID);
            }
            else
            {
                if (template.EnabledFeatures == null)
                {
                    template.EnabledFeatures = new List<FeatureEnum>();
                }

                template.EnabledFeatures.Add(featureID);
            }

            await terminalTemplatesService.UpdateEntity(template);
            return Ok(new OperationResponse(Messages.TerminalTemplateUpdated, StatusEnum.Success) { EntityID = terminalTemplateID });
        }

        [HttpPost]
        [Route("{terminalTemplateID}/approve")]
        public async Task<ActionResult<OperationResponse>> ApproveTerminalTemplate([FromRoute]long terminalTemplateID)
        {
            if (!User.IsBillingAdmin())
            {
                throw new SecurityException();
            }

            var terminalTemplate = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplateID));

            terminalTemplate.Active = true;
            await terminalTemplatesService.UpdateEntity(terminalTemplate);

            return Ok(new OperationResponse(Messages.TerminalTemplateApproved, StatusEnum.Success, terminalTemplateID.ToString()));
        }

        [HttpPost]
        [Route("{terminalTemplateID}/disapprove")]
        public async Task<ActionResult<OperationResponse>> DisapproveTerminalTemplate([FromRoute]long terminalTemplateID)
        {
            if (!User.IsBillingAdmin())
            {
                throw new SecurityException();
            }

            var terminalTemplate = EnsureExists(await terminalTemplatesService.GetTerminalTemplate(terminalTemplateID));

            terminalTemplate.Active = false;
            await terminalTemplatesService.UpdateEntity(terminalTemplate);

            return Ok(new OperationResponse(Messages.TerminalTemplateDisabled, StatusEnum.Success, terminalTemplateID.ToString()));
        }
    }
}
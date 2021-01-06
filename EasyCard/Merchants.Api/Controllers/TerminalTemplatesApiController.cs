using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Merchants.Shared;
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

        public TerminalTemplatesApiController(ITerminalTemplatesService terminalTemplatesService, IMapper mapper, ISystemSettingsService systemSettingsService, IExternalSystemsService externalSystemsService)
        {
            this.terminalTemplatesService = terminalTemplatesService;
            this.mapper = mapper;
            this.systemSettingsService = systemSettingsService;
            this.externalSystemsService = externalSystemsService;
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

            var response = new SummariesResponse<TerminalTemplateSummary>
            {
                NumberOfRecords = await query.CountAsync(),
                Data = await mapper.ProjectTo<TerminalTemplateSummary>(query.ApplyPagination(filter)).ToListAsync()
            };

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
            // TODO: validation if it exists
            await terminalTemplatesService.RemoveTerminalTemplateExternalSystem(terminalTemplateID, externalSystemID);

            return Ok(new OperationResponse(Messages.ExternalSystemRemoved, StatusEnum.Success, terminalTemplateID.ToString()));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Extensions.Filtering;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Business.Entities.Terminal;
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
            var dbTemplate = EnsureExists(await terminalTemplatesService.GetQuery().FirstOrDefaultAsync(t => t.TerminalTemplateID == terminalTemplateID));

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

    }
}
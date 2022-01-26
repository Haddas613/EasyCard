using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.RapidOne;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RapidOne;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Integration;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/rapidone")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RapidOneApiController : ApiControllerBase
    {
        private readonly RapidOneInvoicing rapidOneInvoicing;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IExternalSystemsService externalSystemsService;

        public RapidOneApiController(
            RapidOneInvoicing rapidOneInvoicing,
            IMapper mapper,
            ITerminalsService terminalsService,
            IExternalSystemsService externalSystemsService)
        {
            this.rapidOneInvoicing = rapidOneInvoicing;
            this.mapper = mapper;
            this.terminalsService = terminalsService;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID);

            var rapidOneIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.RapidOneInvoicingExternalSystemID));

            if (rapidOneIntegration == null)
            {
                return BadRequest("Rapid One is not connected to this terminal");
            }

            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(rapidOneIntegration.ExternalSystemID), nameof(ExternalSystem));
            var settingsType = Type.GetType(externalSystem.SettingsTypeFullName);
            var settings = request.Settings.ToObject(settingsType);

            if (settings == null)
            {
                throw new ApplicationException($"Could not create instance of {externalSystem.SettingsTypeFullName}");
            }

            //TODO: temporary implementation. Make a request to rapidOne as well
            if (settings is IExternalSystemSettings externalSystemSettings)
            {
                rapidOneIntegration.Valid = await externalSystemSettings.Valid();
            }
            else
            {
                rapidOneIntegration.Valid = true;
            }

            //TODO: save on success?
            //mapper.Map(request, rapidOneIntegration);
            //await terminalsService.SaveTerminalExternalSystem(rapidOneIntegration, terminal);
            var response = new OperationResponse(Resources.MessagesResource.ConnectionSuccess, StatusEnum.Success);

            if (!rapidOneIntegration.Valid)
            {
                response.Status = StatusEnum.Error;
                response.Message = Resources.MessagesResource.ConnectionFailed;
            }

            return response;
        }

        [HttpGet]
        [Route("companies")]
        public async Task<ActionResult<IEnumerable<CompanySummary>>> GetCompanies(string baseurl, string token)
        {
            var result = await rapidOneInvoicing.GetCompanies(baseurl, token);

            var response = result.Select(r => new CompanySummary { DbName = r.DbName, Name = r.Name });

            return Ok(response);
        }

        [HttpGet]
        [Route("branches")]
        public async Task<ActionResult<IEnumerable<BranchSummary>>> GetBranches(string baseurl, string token)
        {
            var result = await rapidOneInvoicing.GetBranches(baseurl, token);

            var response = result.Where(r => r.Active)
                .Select(r => new BranchSummary { BranchID = r.BranchID, Name = r.Name });

            return Ok(response);
        }

        [HttpGet]
        [Route("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentSummary>>> GetDepartments(string baseurl, string token, int branchId)
        {
            var result = await rapidOneInvoicing.GetDepartments(baseurl, token, branchId);

            var response = result.Where(r => r.Active)
                .Select(r => new DepartmentSummary { BranchID = r.BranchID, DepartmentID = r.DepartmentID, Name = r.Name, Active = r.Active });

            return Ok(response);
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute]string entityID)
        {
            if (string.IsNullOrWhiteSpace(entityID))
            {
                return NotFound();
            }

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await rapidOneInvoicing.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EasyInvoice;
using EasyInvoice.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.EasyInvoice;
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

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/easy-invoice")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)]
    public class EasyInvoiceApiController : ApiControllerBase
    {
        private readonly ECInvoiceInvoicing eCInvoicing;
        private readonly ITerminalsService terminalsService;
        private readonly ITerminalTemplatesService terminalTemplatesService;
        private readonly IMapper mapper;

        public EasyInvoiceApiController(
            ECInvoiceInvoicing eCInvoicing,
            ITerminalsService terminalsService,
            ITerminalTemplatesService terminalTemplatesService,
            IMapper mapper)
        {
            this.eCInvoicing = eCInvoicing;
            this.terminalsService = terminalsService;
            this.terminalTemplatesService = terminalTemplatesService;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("create-customer")]
        public async Task<ActionResult<OperationResponse>> CreateCustomer(CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            var createUserResult = await eCInvoicing.CreateCustomer(
                new EasyInvoice.Models.ECCreateCustomerRequest
                {
                    Email = request.UserName,
                    Password = request.Password,
                    TaxID = request.BusinessID
                },
                GetCorrelationID());

            var response = new OperationResponse(EasyInvoiceMessagesResource.CustomerCreatedSuccessfully, StatusEnum.Success);

            // Currently only possible if 409 (user already exists)
            if (createUserResult.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.CustomerAlreadyExists;

                return BadRequest(response);
            }

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            terminalSettings.Password = request.Password;
            terminalSettings.UserName = request.UserName;
            easyInvoiceIntegration.Settings = JObject.FromObject(terminalSettings);
            await terminalsService.SaveTerminalExternalSystem(easyInvoiceIntegration, terminal);

            var generateCertificateResult = await eCInvoicing.GenerateCertificate(
                terminalSettings,
                GetCorrelationID());

            if (generateCertificateResult.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = generateCertificateResult.Message;

                return BadRequest(response);
            }

            terminalSettings.KeyStorePassword = generateCertificateResult.EntityReference;

            easyInvoiceIntegration.Settings = JObject.FromObject(terminalSettings);
            await terminalsService.SaveTerminalExternalSystem(easyInvoiceIntegration, terminal);

            response.AdditionalData = easyInvoiceIntegration.Settings;
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

            var data = mapper.Map<IEnumerable<IntegrationRequestLog>>(await eCInvoicing.GetStorageLogs(entityID));

            var response = new SummariesResponse<IntegrationRequestLog>
            {
                Data = data,
                NumberOfRecords = data.Count()
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("set-document-number")]
        public async Task<ActionResult<OperationResponse>> SetDocumentNumber(SetDocumentNumberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();

            //terminalSettings.Password = request.Password;
            var changeDocumentNumberResult = await eCInvoicing.SetDocumentNumber(
                new EasyInvoice.Models.ECInvoiceSetDocumentNumberRequest
                {
                    CurrentNum = request.CurrentNum,
                    DocType = (ECInvoiceDocumentType)Enum.Parse(typeof(ECInvoiceDocumentType), request.DocType),
                    Email = terminalSettings.UserName,
                    Terminal = terminalSettings
                },
                GetCorrelationID());

            var response = new OperationResponse(EasyInvoiceMessagesResource.DocumentNumberChangedSuccessfully, StatusEnum.Success);

            if (changeDocumentNumberResult.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.DocumentNumberChangedFailed;

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("get-document-number")]
        public async Task<ActionResult<OperationResponse>> GetDocumentNumber(GetDocumentNumberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();

            //terminalSettings.Password = request.Password;
            var getDocumentNumberResult = await eCInvoicing.GetDocumentNumber(
                new EasyInvoice.Models.ECInvoiceGetDocumentNumberRequest
                {
                    DocType = (ECInvoiceDocumentType)Enum.Parse(typeof(ECInvoiceDocumentType), request.DocType),
                    Terminal = terminalSettings
                },
                GetCorrelationID());

            var response = new OperationResponse(EasyInvoiceMessagesResource.DocumentNumberGetSuccessfully, StatusEnum.Success, getDocumentNumberResult.NextDocumentNumber.ToString());

            if (response.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.DocumentNumberGetFailed;

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("get-document-types")]
        public async Task<ActionResult<OperationResponse>> GetDocumentTypes(GetDocumentNumberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();

            //terminalSettings.Password = request.Password;
            var getDocumentNumberResult = await eCInvoicing.GetDocumentTypes(
                new EasyInvoice.Models.ECInvoiceGetDocumentNumberRequest
                {
                    Terminal = terminalSettings
                },
                GetCorrelationID());

            var response = new OperationResponse(EasyInvoiceMessagesResource.DocumentTypesGetSuccessfully, StatusEnum.Success);

            if (response.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.DocumentTypesGetFailed;

                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
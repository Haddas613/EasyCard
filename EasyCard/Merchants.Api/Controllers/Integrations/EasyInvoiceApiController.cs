using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EasyInvoice;
using EasyInvoice.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.EasyInvoice;
using Merchants.Api.Models.Terminal;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Models;
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
        private readonly IExternalSystemsService externalSystemsService;

        public EasyInvoiceApiController(
            ECInvoiceInvoicing eCInvoicing,
            ITerminalsService terminalsService,
            ITerminalTemplatesService terminalTemplatesService,
            IMapper mapper,
            IExternalSystemsService externalSystemsService)
        {
            this.eCInvoicing = eCInvoicing;
            this.terminalsService = terminalsService;
            this.terminalTemplatesService = terminalTemplatesService;
            this.mapper = mapper;
            this.externalSystemsService = externalSystemsService;
        }

        [HttpPost]
        [Route("test-connection")]
        public async Task<ActionResult<OperationResponse>> TestConnection(ExternalSystemRequest request)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var externalSystems = await terminalsService.GetTerminalExternalSystems(request.TerminalID);

            var easyInvoiceIntegration = EnsureExists(externalSystems.FirstOrDefault(t => t.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            if (easyInvoiceIntegration == null)
            {
                return BadRequest("easyInvoice is not connected to this terminal");
            }

            var externalSystem = EnsureExists(externalSystemsService.GetExternalSystem(easyInvoiceIntegration.ExternalSystemID), nameof(ExternalSystem));
            var settingsType = Type.GetType(externalSystem.SettingsTypeFullName);
            var settings = request.Settings.ToObject(settingsType);

            if (settings == null)
            {
                throw new ApplicationException($"Could not create instance of {externalSystem.SettingsTypeFullName}");
            }

            //TODO: temporary implementation. Make a request to easyInvoice as well
            if (settings is IExternalSystemSettings externalSystemSettings)
            {
                easyInvoiceIntegration.Valid = await externalSystemSettings.Valid();
            }
            else
            {
                easyInvoiceIntegration.Valid = true;
            }

            //TODO: save on success?
            //mapper.Map(request, easyInvoiceIntegration);
            //await terminalsService.SaveTerminalExternalSystem(easyInvoiceIntegration, terminal);
            var response = new OperationResponse(Resources.MessagesResource.ConnectionSuccess, StatusEnum.Success);

            if (!easyInvoiceIntegration.Valid)
            {
                response.Status = StatusEnum.Error;
                response.Message = Resources.MessagesResource.ConnectionFailed;
            }

            return response;
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
            var easyInvoiceIntegration = (await terminalsService.GetTerminalExternalSystems(terminal.TerminalID)).FirstOrDefault(es => es.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID);

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


        [HttpPost]
        [Route("update-customer")]
        public async Task<ActionResult<OperationResponse>> UpdateCustomer(ECInvoiceUpdateUserDetailsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));
            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();

            var updateUserResult = await eCInvoicing.UpdateCustomer(
                new UpdateUserDetailsRequest()
                {
                    Terminal = terminalSettings,
                    city = request.city,
                    country = request.country,
                    countryCode = request.countryCode,
                    email = request.email,
                    generalClientCode = request.generalClientCode,
                    hashExportConfiguration = request.hashExportConfiguration,
                    incomeCode = request.incomeCode,
                    name = request.name,
                    password = request.password,
                    phoneNumber = request.phoneNumber,
                    postalCode = request.postalCode,
                    street = request.street,
                    streetNumber = request.streetNumber,
                    taxId = request.taxId
                }, GetCorrelationID());

            var response = new OperationResponse(EasyInvoiceMessagesResource.CustomerUpdatedSuccessfully, StatusEnum.Success);

            // Currently only possible if 409 (user already exists)
            if (updateUserResult.Status != StatusEnum.Success)
            {
                response.Status = StatusEnum.Error;
                response.Message = EasyInvoiceMessagesResource.CustomerUpdatedFailed;

                return BadRequest(response);
            }

            //EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            //terminalSettings.Password = request.Password;
            //terminalSettings.UserName = request.UserName;
            //easyInvoiceIntegration.Settings = JObject.FromObject(terminalSettings);
            // await terminalsService.SaveTerminalExternalSystem(easyInvoiceIntegration, terminal);

            //var generateCertificateResult = await eCInvoicing.GenerateCertificate(
            //   terminalSettings,
            // GetCorrelationID());

            // if (generateCertificateResult.Status != StatusEnum.Success)
            // {
            //     response.Status = StatusEnum.Error;
            //     response.Message = generateCertificateResult.Message;
            //
            //     return BadRequest(response);
            // }

            //terminalSettings.KeyStorePassword = generateCertificateResult.EntityReference;

            easyInvoiceIntegration.Settings = JObject.FromObject(terminalSettings);
            await terminalsService.SaveTerminalExternalSystem(easyInvoiceIntegration, terminal);

            response.AdditionalData = easyInvoiceIntegration.Settings;
            return Ok(response);
        }

        [HttpGet]
        [Route("request-logs/{entityID}")]
        public async Task<ActionResult<SummariesResponse<IntegrationRequestLog>>> GetRequestLogs([FromRoute] string entityID)
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

        [HttpGet]
        [Route("get-document-types")]
        public async Task<ActionResult<IEnumerable<DictionaryDetails>>> GetDocumentTypes()
        {
            var response = Enum.GetNames(typeof(ECInvoiceDocumentType))
                .Select(e => new DictionaryDetails
                {
                    Code = e,
                    Description = ECInvoiceDocumentTypeResource.ResourceManager.GetString(e),
                });

            return Ok(response);
        }

        [HttpPost]
        [Route("set-document-number")]
        public async Task<ActionResult<OperationResponse>> SetDocumentNumber([FromBody] SetDocumentNumberRequest request)
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
        public async Task<ActionResult<OperationResponse>> GetDocumentNumber([FromQuery] GetDocumentNumberRequest request)
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
        [Route("get-document-report")]
        public async Task<ActionResult<IEnumerable<ECInvoiceGetReportItem>>> GetDocumentReport(GetDocumentReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            var getDocumentReportResult = await eCInvoicing.GetReport(
                new EasyInvoice.Models.ECInvoiceGetDocumentReportRequest
                {
                    Terminal = terminalSettings,
                    OnlyCancelled = request.OnlyCancelled,
                    IncludeCancelled = request.IncludeCancelled,
                    StartDate = request.StartAt.ToString("yyyy-MM-dd"),
                    EndDate = request.EndAt.ToString("yyyy-MM-dd")
                },
                GetCorrelationID());
            return Ok(getDocumentReportResult);
            // var response = new OperationResponse(EasyInvoiceMessagesResource.DocumentNumberGetSuccessfully, StatusEnum.Success, getDocumentNumberResult.ToString());

            // if (response.Status != StatusEnum.Success)
            // {
            //     response.Status = StatusEnum.Error;
            //     response.Message = EasyInvoiceMessagesResource.DocumentNumberGetFailed;
            //
            //     return BadRequest(response);
            // }
            //
            // return Ok(response);
        }
    }
}
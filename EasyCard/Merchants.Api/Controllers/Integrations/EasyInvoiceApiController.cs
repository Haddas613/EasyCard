using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyInvoice;
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

        public EasyInvoiceApiController(
            ECInvoiceInvoicing eCInvoicing,
            ITerminalsService terminalsService,
            ITerminalTemplatesService terminalTemplatesService)
        {
            this.eCInvoicing = eCInvoicing;
            this.terminalsService = terminalsService;
            this.terminalTemplatesService = terminalTemplatesService;
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

            return Ok(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices.BlobStorage;
using EasyInvoice;
using EasyInvoice.Models;
using Merchants.Api.Models.Integrations;
using Merchants.Api.Models.Integrations.EasyInvoice;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Models.Integration;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.Models;
using Shared.Integration;
using Transactions.Api;

namespace Merchants.Api.Controllers.Integrations
{
    [Route("api/integrations/easy-invoice")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.MerchantFrontend)]
    public class EasyInvoiceApiController : ApiControllerBase
    {
        private readonly ECInvoiceInvoicing eCInvoicing;
        private readonly ITerminalsService terminalsService;
        private readonly ITerminalTemplatesService terminalTemplatesService;
        private readonly IMapper mapper;
        private readonly IExternalSystemsService externalSystemsService;
        private readonly IBlobStorageService blobStorageService;
        public EasyInvoiceApiController(
            ECInvoiceInvoicing eCInvoicing,
            IBlobStorageService blobStorageService,
            ITerminalsService terminalsService,
            ITerminalTemplatesService terminalTemplatesService,
            IMapper mapper,
            IExternalSystemsService externalSystemsService)
        {
            this.blobStorageService = blobStorageService;
            this.eCInvoicing = eCInvoicing;
            this.terminalsService = terminalsService;
            this.terminalTemplatesService = terminalTemplatesService;
            this.mapper = mapper;
            this.externalSystemsService = externalSystemsService;
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

        [HttpPost]
        [Route("get-document-tax-report")]
        public async Task<string> GetDocumentTaxReport([FromBody]GetDocumentTaxReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState).ToString();
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            var getTaxReportResult = await eCInvoicing.GetTaxReport(
                new EasyInvoice.Models.ECInvoiceGetDocumentTaxReportRequest
                {
                    Terminal = terminalSettings,
                    StartDate = request.StartAt.ToString("yyyy-MM-dd"),
                    EndDate = request.EndAt.ToString("yyyy-MM-dd")
                },
                GetCorrelationID());
            Stream stream = new MemoryStream(getTaxReportResult.FileContents);
            return await blobStorageService.Upload("TaxReport.zip", stream, "application/zip");
        }

        [HttpGet]
        [Route("get-document-hash-report")]
        public async Task<Object> GetDocumentHashReport(GetDocumentTaxReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            var getDocumentReportResult = await eCInvoicing.GetHashReport(
                new EasyInvoice.Models.ECInvoiceGetDocumentTaxReportRequest
                {
                    Terminal = terminalSettings,
                    StartDate = request.StartAt.ToString("yyyy-MM-dd"),
                    EndDate = request.EndAt.ToString("yyyy-MM-dd")
                },
                GetCorrelationID());

            //RestClient clientRestSharop;
            //var lk = clientRestSharop.DownloadData(getDocumentReportResult);
            //var file =  File(getDocumentReportResult.ToString(), "application/zip", "taxReport.zip");
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
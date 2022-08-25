using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices.BlobStorage;
using EasyInvoice;
using EasyInvoice.Models;
using Shared.Api;
using Shared.Integration;
using Merchants.Api.Models.Integrations.EasyInvoice;
using Merchants.Business.Services;
using Microsoft.Extensions.Options;

namespace Transactions.Api.Controllers.Integrations
{
    [Route("api/integrations/easy-invoice")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.MerchantFrontend)]
    public class EasyInvoiceApiController : ApiControllerBase
    {
        private readonly ECInvoiceInvoicing eCInvoicing;
        private readonly ITerminalsService terminalsService;
        private readonly IMapper mapper;
        private readonly IBlobStorageService blobStorageService;
        private readonly Shared.ApplicationSettings appSettings;
        public EasyInvoiceApiController(
            ECInvoiceInvoicing eCInvoicing,
            IBlobStorageService blobStorageService,
            ITerminalsService terminalsService,
            IMapper mapper,
             IOptions<Shared.ApplicationSettings> appSettings)
        {
            this.blobStorageService = blobStorageService;
            this.eCInvoicing = eCInvoicing;
            this.terminalsService = terminalsService;
            this.appSettings = appSettings.Value;
            this.mapper = mapper;
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
            var content = await eCInvoicing.GetTaxReport(
                new EasyInvoice.Models.ECInvoiceGetDocumentTaxReportRequest
                {
                    Terminal = terminalSettings,
                    StartDate = request.StartAt.ToString("yyyy-MM-dd"),
                    EndDate = request.EndAt.ToString("yyyy-MM-dd")
                },
                GetCorrelationID());
            var fileNameFull = string.Format("{0}.{1}", "TaxReport", "zip");
            var mimeType = string.Format("application/{0}", "zip");
            FileContentResult getTaxReportResult = new FileContentResult(content, mimeType)
            {
                FileDownloadName = fileNameFull
            };

            Stream stream = new MemoryStream(getTaxReportResult.FileContents);
            var fileName = await blobStorageService.Upload("TaxReport.zip", stream, "application/zip");
            return blobStorageService.GetDownloadUrl(fileName);
        }

        [HttpPost]
        [Route("get-document-hash-report")]
        public async Task<string> GetDocumentHashReport([FromBody]GetDocumentTaxReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState).ToString();
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(request.TerminalID));
            var easyInvoiceIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.ECInvoiceExternalSystemID));

            EasyInvoiceTerminalSettings terminalSettings = easyInvoiceIntegration.Settings.ToObject<EasyInvoiceTerminalSettings>();
            var content = await eCInvoicing.GetHashReport(
                new EasyInvoice.Models.ECInvoiceGetDocumentTaxReportRequest
                {
                    Terminal = terminalSettings,
                    StartDate = request.StartAt.ToString("yyyy-MM-dd"),
                    EndDate = request.EndAt.ToString("yyyy-MM-dd")
                },
                GetCorrelationID());

            var fileNameFull = string.Format("{0}.{1}", "HashReport", "zip");
            var mimeType = string.Format("application/{0}", "zip");
            FileContentResult getHasReportResult = new FileContentResult(content, mimeType)
            {
                FileDownloadName = fileNameFull
            };

            Stream stream = new MemoryStream(getHasReportResult.FileContents);
            var fileName = await blobStorageService.Upload("HashReport.zip", stream, "application/zip");
            return blobStorageService.GetDownloadUrl(fileName);
        }
    }
}
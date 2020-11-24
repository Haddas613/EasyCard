using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Security;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Invoicing;
using Shared.Api.Extensions;
using Shared.Helpers.Security;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Transactions.Shared;
using SharedIntegration = Shared.Integration;
using Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [Route("api/invoicing")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class InvoicingController : ApiControllerBase
    {
        private readonly IInvoiceService invoiceService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ISystemSettingsService systemSettingsService;

        public InvoicingController(
                    IInvoiceService invoiceService,
                    IMapper mapper,
                    ITerminalsService terminalsService,
                    ILogger<CardTokenController> logger,
                    IHttpContextAccessorWrapper httpContextAccessor,
                    IConsumersService consumersService,
                    ISystemSettingsService systemSettingsService)
        {
            this.invoiceService = invoiceService;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.systemSettingsService = systemSettingsService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(InvoiceSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(InvoiceSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<InvoiceSummary>>> GetInvoices([FromQuery] InvoicesFilter filter)
        {
            var query = invoiceService.GetInvoices().Filter(filter);

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<InvoiceSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<InvoiceSummary>(query.OrderByDescending(i => i.InvoiceID).ApplyPagination(filter)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{invoiceID}")]
        public async Task<ActionResult<InvoiceResponse>> GetInvoice([FromRoute] Guid invoiceID)
        {
            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbInvoice = EnsureExists(await invoiceService.GetInvoices().FirstOrDefaultAsync(m => m.InvoiceID == invoiceID));

                var invoice = mapper.Map<InvoiceResponse>(dbInvoice);

                return Ok(invoice);
            }
        }

        // NOTE: this creates only db record - Invoicing system integration should be processed in a queue
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateInvoice([FromBody] InvoiceRequest model)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            var newInvoice = mapper.Map<Invoice>(model);

            newInvoice.MerchantID = terminal.MerchantID;

            newInvoice.ApplyAuditInfo(httpContextAccessor);

            if (newInvoice.DealDetails == null)
            {
                newInvoice.DealDetails = new Business.Entities.DealDetails();
            }

            if (newInvoice.InvoiceDetails == null)
            {
                newInvoice.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Check consumer
            var consumer = newInvoice.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == newInvoice.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            if (consumer != null)
            {
                if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(newInvoice.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(newInvoice.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new EntityConflictException(Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                }
            }

            newInvoice.Calculate();

            await invoiceService.CreateEntity(newInvoice);

            return new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, newInvoice.InvoiceID);
        }

        [HttpPost]
        [Route("resend")]
        public async Task<ActionResult<OperationResponse>> Resend(ResendInvoiceRequest request)
        {
            if (request.InvoicesID == null || request.InvoicesID.Count() == 0)
            {
                return BadRequest(new OperationResponse(Messages.InvoicesForResendRequired, null, httpContextAccessor.TraceIdentifier, nameof(request.InvoicesID), Messages.InvoicesForResendRequired));
            }

            var response = new OperationResponse
            {
                Status = StatusEnum.Success,
                Message = Messages.InvoicesQueuedForResend
            };

            return Ok(response);
        }
    }
}
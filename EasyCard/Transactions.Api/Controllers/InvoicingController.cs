﻿using System;
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

        public InvoicingController()
        {

        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<InvoiceSummary>>> GetInvoices([FromQuery] InvoicesFilter filter)
        {
            var query = invoiceService.GetInvoices().Filter(filter);

            using (var dbTransaction = invoiceService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<InvoiceSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<InvoiceSummary>(query.ApplyPagination(filter)).ToListAsync();

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

                var billingDeal = mapper.Map<InvoiceResponse>(dbInvoice);

                return Ok(billingDeal);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateInvoice([FromBody] InvoiceRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            var newInvoice = mapper.Map<Invoice>(model);
            newInvoice.Active = true;

            newInvoice.MerchantID = terminal.MerchantID;

            newInvoice.ApplyAuditInfo(httpContextAccessor);

            await invoiceService.CreateEntity(newInvoice);

            return CreatedAtAction(nameof(GetInvoice), new { BillingDealID = newInvoice.InvoiceID }, new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, newInvoice.InvoiceID.ToString()));
        }
    }
}
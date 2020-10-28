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
using Transactions.Api.Models.PaymentRequests;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;

namespace Transactions.Api.Controllers
{
    [Route("api/paymentRequests")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class PaymentRequestsController : ApiControllerBase
    {
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public PaymentRequestsController(
                    IPaymentRequestsService paymentRequestsService,
                    IMapper mapper,
                    ITerminalsService terminalsService,
                    ILogger<CardTokenController> logger,
                    IHttpContextAccessorWrapper httpContextAccessor,
                    IConsumersService consumersService)
        {
            this.paymentRequestsService = paymentRequestsService;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(PaymentRequestSummary)
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Select(d => d.GetColMeta(InvoiceSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture))
                    .ToDictionary(d => d.Key)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<PaymentRequestSummary>>> GetPaymentRequests([FromQuery] PaymentRequestsFilter filter)
        {
            var query = paymentRequestsService.GetPaymentRequests().Filter(filter);

            using (var dbTransaction = paymentRequestsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<PaymentRequestSummary> { NumberOfRecords = await query.CountAsync() };

                response.Data = await mapper.ProjectTo<PaymentRequestSummary>(query.ApplyPagination(filter)).ToListAsync();

                return Ok(response);
            }
        }

        [HttpGet]
        [Route("{paymentRequestID}")]
        public async Task<ActionResult<PaymentRequestResponse>> GetPaymentRequest([FromRoute] Guid paymentRequestID)
        {
            using (var dbTransaction = paymentRequestsService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbPaymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().FirstOrDefaultAsync(m => m.PaymentRequestID == paymentRequestID));

                var paymentRequest = mapper.Map<PaymentRequestResponse>(dbPaymentRequest);

                return Ok(paymentRequest);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreatePaymentRequest([FromBody] PaymentRequestCreate model)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");

            var newPaymentRequest = mapper.Map<PaymentRequest>(model);

            newPaymentRequest.MerchantID = terminal.MerchantID;

            newPaymentRequest.ApplyAuditInfo(httpContextAccessor);

            await paymentRequestsService.CreateEntity(newPaymentRequest);

            return CreatedAtAction(nameof(GetPaymentRequest), new { paymentRequestID = newPaymentRequest.PaymentRequestID }, new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, newPaymentRequest.PaymentRequestID));
        }
    }
}
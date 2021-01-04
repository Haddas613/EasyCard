using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Services;

namespace Transactions.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/checkout")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.AnyAdmin)] // TODO: checkout portal identity
    [ApiController]
    public class CheckoutController : ApiControllerBase
    {
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly ITerminalsService terminalsService;
        private readonly IPaymentRequestsService paymentRequestsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ISystemSettingsService systemSettingsService;

        public CheckoutController(
            IMapper mapper,
            ITerminalsService terminalsService,
            ILogger<TransactionsApiController> logger,
            IPaymentRequestsService paymentRequestsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService)
        {
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
        }

        [HttpGet]
        public async Task<ActionResult<CheckoutData>> GetCheckoutData(Guid? paymentRequestID, string apiKey)
        {
            Debug.WriteLine(User);

            var response = new CheckoutData();

            var apiKeyB = Convert.FromBase64String(apiKey);

            var terminal = EnsureExists(await terminalsService.GetTerminals().Where(d => d.SharedApiKey == apiKeyB).FirstOrDefaultAsync());

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            response.Settings = new TerminalCheckoutCombinedSettings();

            mapper.Map(terminal.PaymentRequestSettings, response.Settings);
            mapper.Map(terminal.CheckoutSettings, response.Settings);
            mapper.Map(terminal.Settings, response.Settings);
            mapper.Map(terminal.Merchant, response.Settings);
            mapper.Map(terminal, response.Settings);

            if (paymentRequestID.HasValue)
            {
                var paymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().Where(d => d.PaymentRequestID == paymentRequestID && d.TerminalID == terminal.TerminalID).FirstOrDefaultAsync());

                response.PaymentRequest = mapper.Map<PaymentRequestInfo>(paymentRequest);
            }

            return response;
        }
    }
}

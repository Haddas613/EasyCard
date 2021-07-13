using AutoMapper;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Extensions;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Api;
using Shared.Business.Security;
using Shared.Integration;
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
        private readonly IConsumersService consumersService;
        private readonly ICreditCardTokenService creditCardTokenService;

        public CheckoutController(
            IMapper mapper,
            ITerminalsService terminalsService,
            ILogger<TransactionsApiController> logger,
            IPaymentRequestsService paymentRequestsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService,
            IConsumersService consumersService,
            ICreditCardTokenService creditCardTokenService)
        {
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.consumersService = consumersService;
            this.creditCardTokenService = creditCardTokenService;
        }

        [HttpGet]
        public async Task<ActionResult<CheckoutData>> GetCheckoutData(Guid? paymentRequestID, string apiKey, Guid? consumerID)
        {
            Debug.WriteLine(User);

            var response = new CheckoutData();

            var apiKeyB = Convert.FromBase64String(apiKey);

            var tid = EnsureExists(await terminalsService.GetTerminals().Where(d => d.SharedApiKey == apiKeyB).Select(t => t.TerminalID).FirstOrDefaultAsync(), nameof(Terminal));
            var terminal = await terminalsService.GetTerminal(tid);

            if (terminal.EnabledFeatures == null || !terminal.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.Checkout))
            {
                return Unauthorized($"Checkout feature is not enabled.");
            }

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

            response.Settings.AllowPinPad = terminal.IntegrationEnabled(ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID);

            if (response.Settings.AllowPinPad == true)
            {
                var nayaxIntegration = terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID);
                if (nayaxIntegration != null)
                {
                    var devices = nayaxIntegration.Settings.ToObject<Nayax.NayaxTerminalCollection>();
                    response.Settings.AllowPinPad = devices?.devices?.Any() == true;
                    if (response.Settings.AllowPinPad == true)
                    {
                        response.Settings.PinPadDevices = devices.devices.Select(d => new PinPadDevice { DeviceID = d.TerminalID, DeviceName = d.PosName });
                    }
                }
            }

            if (paymentRequestID.HasValue)
            {
                var paymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().Where(d => d.PaymentRequestID == paymentRequestID && d.TerminalID == terminal.TerminalID).FirstOrDefaultAsync());
                response.Settings.AllowPinPad = paymentRequest.AllowPinPad && response.Settings.AllowPinPad.GetValueOrDefault();

                if (consumerID.HasValue)
                {
                    if (consumerID.Value != paymentRequest.DealDetails.ConsumerID)
                    {
                        return Unauthorized($"{consumerID} does not have access to payment request {paymentRequest.PaymentRequestID}");
                    }
                }

                if (paymentRequest.Status == Shared.Enums.PaymentRequestStatusEnum.Sent || paymentRequest.Status == Shared.Enums.PaymentRequestStatusEnum.Initial)
                {
                    await paymentRequestsService.UpdateEntityWithStatus(paymentRequest, Shared.Enums.PaymentRequestStatusEnum.Viewed);
                }

                response.PaymentRequest = mapper.Map<PaymentRequestInfo>(paymentRequest);
            }

            if (consumerID.HasValue)
            {
                var consumer = await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == consumerID);

                if (consumer != null)
                {
                    response.Consumer = new ConsumerInfo();
                    mapper.Map(consumer, response.Consumer);

                    var tokensRaw = await creditCardTokenService.GetTokens()
                        .Where(d => d.ConsumerID == consumer.ConsumerID)
                        .ToListAsync();

                    //TODO: no in memory filtering
                    var tokens = tokensRaw.Where(t => t.CardExpiration.Expired == false)
                        .Select(d => new TokenInfo { CardNumber = d.CardNumber, CardExpiration = d.CardExpiration.ToString(), CardVendor = d.CardVendor, CreditCardTokenID = d.CreditCardTokenID })
                        .ToList();

                    if (tokens.Count > 0)
                    {
                        response.Consumer.Tokens = tokens;
                    }
                }
            }

            return response;
        }
    }
}

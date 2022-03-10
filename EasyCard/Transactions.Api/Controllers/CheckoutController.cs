using AutoMapper;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Extensions;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Business.Security;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Entities;
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
        private readonly IPaymentIntentService paymentIntentService;

        public CheckoutController(
            IMapper mapper,
            ITerminalsService terminalsService,
            ILogger<TransactionsApiController> logger,
            IPaymentRequestsService paymentRequestsService,
            IHttpContextAccessorWrapper httpContextAccessor,
            ISystemSettingsService systemSettingsService,
            IConsumersService consumersService,
            ICreditCardTokenService creditCardTokenService,
            IPaymentIntentService paymentIntentService)
        {
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.paymentRequestsService = paymentRequestsService;
            this.httpContextAccessor = httpContextAccessor;
            this.systemSettingsService = systemSettingsService;
            this.consumersService = consumersService;
            this.creditCardTokenService = creditCardTokenService;
            this.paymentIntentService = paymentIntentService;
        }

        [HttpGet]
        public async Task<ActionResult<CheckoutData>> GetCheckoutData(Guid? paymentRequestID, Guid? paymentIntentID, string apiKey)
        {
            Debug.WriteLine(User);

            var response = new CheckoutData();

            Guid? terminalID = null;
            Terminal terminal = null;
            PaymentRequest paymentRequest = null;

            //Api key is not required and ignored for payment request
            if (paymentRequestID.HasValue)
            {
                paymentRequest = EnsureExists(await paymentRequestsService.GetPaymentRequests().Where(d => d.PaymentRequestID == paymentRequestID).FirstOrDefaultAsync());
                terminalID = EnsureExists(paymentRequest.TerminalID);
            }
            else if (paymentIntentID.HasValue)
            {
                paymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(paymentIntentID.Value));
                terminalID = EnsureExists(paymentRequest.TerminalID);
            }
            else if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var apiKeyB = Convert.FromBase64String(apiKey);
                terminalID = EnsureExists(await terminalsService.GetTerminals().Where(d => d.SharedApiKey == apiKeyB).Select(t => t.TerminalID).FirstOrDefaultAsync(), nameof(Terminal));
            }
            else
            {
                return Unauthorized($"Terminal is not specified");
            }

            terminal = EnsureExists(await terminalsService.GetTerminal(terminalID.Value));

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
                    var devices = nayaxIntegration.Settings.ToObject<Nayax.Models.NayaxTerminalCollection>();
                    response.Settings.AllowPinPad = devices?.devices?.Any() == true;
                    if (response.Settings.AllowPinPad == true)
                    {
                        response.Settings.PinPadDevices = devices.devices.Select(d => new PinPadDevice { DeviceID = d.TerminalID, DeviceName = d.PosName });
                    }
                }
            }

            Guid? consumerID = null;

            if (paymentRequestID.HasValue)
            {
                response.Settings.AllowPinPad = paymentRequest.AllowPinPad && response.Settings.AllowPinPad.GetValueOrDefault();
                consumerID = paymentRequest.DealDetails.ConsumerID;

                if (paymentRequest.Status == Shared.Enums.PaymentRequestStatusEnum.Sent || paymentRequest.Status == Shared.Enums.PaymentRequestStatusEnum.Initial)
                {
                    await paymentRequestsService.UpdateEntityWithStatus(paymentRequest, Shared.Enums.PaymentRequestStatusEnum.Viewed);
                }

                response.PaymentRequest = mapper.Map<PaymentRequestInfo>(paymentRequest);
            }
            else if (paymentIntentID.HasValue)
            {
                response.Settings.AllowPinPad = paymentRequest.AllowPinPad && response.Settings.AllowPinPad.GetValueOrDefault();
                consumerID = paymentRequest.DealDetails.ConsumerID;

                response.PaymentRequest = mapper.Map<PaymentRequestInfo>(paymentRequest);
                response.PaymentIntentID = paymentIntentID;
            }

            if (consumerID.HasValue)
            {
                var consumer = await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == consumerID);

                if (consumer != null)
                {
                    response.Consumer = new ConsumerInfo();
                    mapper.Map(consumer, response.Consumer);

                    List<CreditCardTokenDetails> tokensRaw = null;

                    if (terminal.Settings.SharedCreditCardTokens == true)
                    {
                        tokensRaw = await creditCardTokenService.GetTokensSharedAdmin(terminal.MerchantID, terminal.TerminalID)
                            .Where(d => d.ConsumerID == consumer.ConsumerID)
                            .ToListAsync();
                    }
                    else
                    {
                        tokensRaw = await creditCardTokenService.GetTokens()
                            .Where(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == consumer.ConsumerID)
                            .ToListAsync();
                    }

                    //TODO: no in memory filtering
                    var tokens = tokensRaw.Where(t => t.CardExpiration.Expired == false)
                        .Select(d => new TokenInfo {
                            CardNumber = d.CardNumber,
                            CardExpiration = d.CardExpiration.ToString(),
                            CardVendor = d.CardVendor,
                            CreditCardTokenID = d.CreditCardTokenID,
                            Created = d.Created,
                        })
                        .ToList();

                    if (tokens.Count > 0)
                    {
                        response.Consumer.Tokens = tokens;
                    }
                }
            }

            response.Settings.AllowBit = terminal.IntegrationEnabled(ExternalSystemHelpers.BitVirtualWalletProcessorExternalSystemID);
            response.Settings.EnableThreeDS = terminal.CheckoutSettings.Support3DSecure;
            return response;
        }
    }
}

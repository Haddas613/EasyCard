﻿using AutoMapper;
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
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using SharedHelpers = Shared.Helpers;

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

                    tokensRaw = await creditCardTokenService.GetTokens(terminal, consumer);

                    //TODO: no in memory filtering
                    var tokens = tokensRaw.Where(t => t.CardExpiration.Expired == false)
                        .Select(d => new TokenInfo
                        {
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
                else
                {
                    response.Consumer = new ConsumerInfo();
                    mapper.Map(paymentRequest.DealDetails, response.Consumer);
                }
            }
            else
            {
                response.Consumer = new ConsumerInfo();
                mapper.Map(paymentRequest.DealDetails, response.Consumer);
            }

            response.Settings.AllowBit =
                terminal.IntegrationEnabled(ExternalSystemHelpers.BitVirtualWalletProcessorExternalSystemID)
                && response.PaymentRequest.Currency == SharedHelpers.CurrencyEnum.ILS
                && !response.PaymentRequest.IsRefund
                && (response.PaymentRequest.PaymentRequestAmount > 0 == true || response.PaymentRequest.UserAmount);

            response.Settings.EnableThreeDS = terminal.Support3DSecure;
            response.Settings.Language = paymentRequest?.Language ?? terminal.CheckoutSettings.DefaultLanguage;

            if (paymentRequest != null)
            {
                response.Settings.IssueInvoice = paymentRequest.IssueInvoice;

                if (paymentRequest.HidePhone.HasValue)
                {
                    response.Settings.HidePhone = paymentRequest.HidePhone;
                }

                if (paymentRequest.HideEmail.HasValue)
                {
                    response.Settings.HideEmail = paymentRequest.HideEmail;
                }

                if (paymentRequest.HideNationalID.HasValue)
                {
                    response.Settings.HideNationalID = paymentRequest.HideNationalID;
                }

                if (paymentRequest.ConsumerDataReadonly.HasValue)
                {
                    response.Settings.ConsumerDataReadonly = paymentRequest.ConsumerDataReadonly;
                }

                if (paymentRequest.SaveCreditCardByDefault.HasValue)
                {
                    response.Settings.SaveCreditCardByDefault = paymentRequest.SaveCreditCardByDefault;
                }

                if (paymentRequest.DisableCancelPayment.HasValue)
                {
                    response.Settings.DisableCancelPayment = paymentRequest.DisableCancelPayment;
                }

                if (paymentRequest.EmailRequired.HasValue)
                {
                    response.Settings.EmailRequired = paymentRequest.EmailRequired;
                }

                if (paymentRequest.PhoneRequired.HasValue)
                {
                    response.Settings.PhoneRequired = paymentRequest.EmailRequired;
                }

                if (paymentRequest.NationalIDRequired == true)
                {
                    response.Settings.NationalIDRequired = true;
                }
            }

            var transactionTypes = new List<TransactionTypeEnum>();

            if (paymentRequest != null)
            {
                if ((response.Settings.AllowImmediate == true && paymentRequest.AllowImmediate == null) || paymentRequest.AllowImmediate == true)
                {
                    transactionTypes.Add(TransactionTypeEnum.Immediate);
                }

                if ((response.Settings.MaxInstallments > 1 && response.Settings.AllowInstallments == true && paymentRequest.AllowInstallments == null) || paymentRequest.AllowInstallments == true)
                {
                    transactionTypes.Add(TransactionTypeEnum.Installments);
                }

                if ((response.Settings.MaxCreditInstallments > 1 && response.Settings.AllowCredit == true && paymentRequest.AllowCredit == null) || paymentRequest.AllowCredit == true)
                {
                    transactionTypes.Add(TransactionTypeEnum.Credit);
                }

                if (!(transactionTypes.Count > 0) || paymentRequest.AllowRegular != false)
                {
                    transactionTypes.Add(TransactionTypeEnum.RegularDeal);
                }
            }
            else
            {
                transactionTypes.Add(TransactionTypeEnum.RegularDeal);
                transactionTypes.Add(TransactionTypeEnum.Installments);
                transactionTypes.Add(TransactionTypeEnum.Credit);
                transactionTypes.Add(TransactionTypeEnum.Immediate);
            }

            response.Settings.TransactionTypes = transactionTypes;

            return response;
        }
    }
}

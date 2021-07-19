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
using Shared.Helpers.Security;
using Transactions.Api.Models.PaymentRequests;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Transactions.Shared;
using Microsoft.Extensions.Options;
using Transactions.Api.Extensions;
using Shared.Helpers.Email;
using Merchants.Business.Entities.Terminal;
using Shared.Helpers.Templating;
using Shared.Helpers;
using Z.EntityFramework.Plus;
using Shared.Api.Configuration;
using Transactions.Shared.Enums;
using Shared.Integration;
using Newtonsoft.Json.Linq;
using System.Security;

namespace Transactions.Api.Controllers
{
    [Route("api/paymentIntent")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class PaymentIntentController : ApiControllerBase
    {
        private readonly IPaymentIntentService paymentIntentService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ApplicationSettings appSettings;
        private readonly ApiSettings apiSettings;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly ISystemSettingsService systemSettingsService;
        private readonly IEmailSender emailSender;

        public PaymentIntentController(
                    IPaymentIntentService paymentIntentService,
                    IMapper mapper,
                    ITerminalsService terminalsService,
                    ILogger<CardTokenController> logger,
                    IHttpContextAccessorWrapper httpContextAccessor,
                    IConsumersService consumersService,
                    IOptions<ApplicationSettings> appSettings,
                    ICryptoServiceCompact cryptoServiceCompact,
                    ISystemSettingsService systemSettingsService,
                    IEmailSender emailSender,
                    IOptions<ApiSettings> apiSettings)
        {
            this.paymentIntentService = paymentIntentService;
            this.mapper = mapper;
            this.apiSettings = apiSettings.Value;
            this.terminalsService = terminalsService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.appSettings = appSettings.Value;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.systemSettingsService = systemSettingsService;
            this.emailSender = emailSender;
        }

        [HttpGet]
        [Route("{paymentIntentID}")]
        public async Task<ActionResult<PaymentRequestResponse>> GetPaymentIntent([FromRoute] Guid paymentIntentID, Guid? terminalID = null)
        {
            if (terminalID == null && User.IsTerminal())
            {
                terminalID = User.GetTerminalID();
            }

            if (terminalID == null)
            {
                throw new SecurityException("Cannot determine TerminalID");
            }

            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID.GetValueOrDefault()));

            var dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(terminal.TerminalID, paymentIntentID));

            var paymentRequest = mapper.Map<PaymentRequestResponse>(dbPaymentRequest);

            paymentRequest.PaymentRequestUrl = GetPaymentIntentUrl(dbPaymentRequest, terminal.SharedApiKey, dbPaymentRequest.RedirectUrl);

            paymentRequest.TerminalName = terminal.Label;

            return Ok(paymentRequest);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreatePaymentIntent([FromBody] PaymentRequestCreate model)
        {
            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            if (terminal.EnabledFeatures == null || !terminal.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.Checkout))
            {
                return BadRequest(new OperationResponse(Messages.CheckoutFeatureMustBeEnabled, StatusEnum.Error));
            }

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            if (terminal.SharedApiKey == null)
            {
                return BadRequest(new OperationResponse("Please add Shared Api Key first", StatusEnum.Error, correlationId: httpContextAccessor.TraceIdentifier));
            }

            // Check consumer
            var consumer = model.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            if (model.IssueInvoice.GetValueOrDefault())
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings);
            }

            if (model.AllowPinPad.GetValueOrDefault())
            {
                model.PinPadDetails = model.PinPadDetails.UpdatePinPadDetails(terminal.Integrations.FirstOrDefault(i => i.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
            }

            var newPaymentRequest = mapper.Map<PaymentRequest>(model);

            // Update details if needed
            newPaymentRequest.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newPaymentRequest);
            if (consumer != null)
            {
                newPaymentRequest.CardOwnerName = consumer.ConsumerName;
                newPaymentRequest.CardOwnerNationalID = consumer.ConsumerNationalID;
            }

            newPaymentRequest.Calculate();

            newPaymentRequest.MerchantID = terminal.MerchantID;

            if (!string.IsNullOrWhiteSpace(model.RedirectUrl))
            {
                RedirectUrlHelpers.CheckRedirectUrls(terminal.CheckoutSettings.RedirectUrls, model.RedirectUrl);
            }

            string url = GetPaymentIntentUrl(newPaymentRequest, terminal.SharedApiKey, model.RedirectUrl);

            await paymentIntentService.SavePaymentIntent(newPaymentRequest);

            var respObject = new OperationResponse(Transactions.Shared.Messages.PaymentRequestCreated, StatusEnum.Success, newPaymentRequest.PaymentRequestID) { AdditionalData = JObject.FromObject(new { url }) };

            var response = CreatedAtAction(nameof(GetPaymentIntent), new { paymentIntentID = newPaymentRequest.PaymentRequestID }, respObject);

            return response;
        }

        private string GetPaymentIntentUrl(PaymentRequest dbPaymentRequest, byte[] sharedTerminalApiKey, string redirectUrl)
        {
            if (sharedTerminalApiKey == null)
            {
                return null;
            }

            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["paymentIntent"] = Convert.ToBase64String(dbPaymentRequest.PaymentRequestID.ToByteArray());
            query["apiKey"] = Convert.ToBase64String(sharedTerminalApiKey);

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                query["redirectUrl"] = redirectUrl;
            }

            if (dbPaymentRequest.DealDetails?.ConsumerID.HasValue == true)
            {
                query["consumerID"] = dbPaymentRequest.DealDetails.ConsumerID.Value.ToString();
            }

            uriBuilder.Query = query.ToString();
            var url = uriBuilder.ToString();

            return url;
        }
    }
}
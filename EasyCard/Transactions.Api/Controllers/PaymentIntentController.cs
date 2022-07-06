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
using Shared.Api.Validation;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Swagger;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    /// <summary>
    /// Payment link API
    /// </summary>
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

        /// <summary>
        /// Get payment intent details
        /// </summary>
        /// <param name="paymentIntentID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{paymentIntentID}")]
        public async Task<ActionResult<PaymentRequestResponse>> GetPaymentIntent([FromRoute] Guid paymentIntentID)
        {
            var dbPaymentRequest = EnsureExists(await paymentIntentService.GetPaymentIntent(paymentIntentID));

            var terminal = EnsureExists(await terminalsService.GetTerminal(dbPaymentRequest.TerminalID.Value));

            var paymentRequest = mapper.Map<PaymentRequestResponse>(dbPaymentRequest);

            paymentRequest.TerminalName = terminal.Label;

            return Ok(paymentRequest);
        }

        /// <summary>
        /// Create payment link to Checkout Page
        /// </summary>
        /// <param name="model">Payment parameters</param>
        /// <returns>Redirect url</returns>
        [HttpPost]
        [ValidateModelState]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(PaymentRequestCreate), typeof(CreatePaymentIntentExample))]
        [SwaggerResponseExample(201, typeof(PRCreatedOperationResponseExample))]
        [SwaggerResponseExample(404, typeof(EntityNotFoundOperationResponseExample))]
        [SwaggerResponseExample(400, typeof(ValidationErrorsOperationResponseExample))]
        public async Task<ActionResult<OperationResponse>> CreatePaymentIntent([FromBody] PaymentRequestCreate model)
        {
            // TODO: validate values

            var merchantID = User.GetMerchantID();
            var userIsTerminal = User.IsTerminal();

            if (!userIsTerminal && model.TerminalID == null)
            {
                return BadRequest(new OperationResponse("TerminalID required", StatusEnum.Error, correlationId: httpContextAccessor.TraceIdentifier));
            }

            var terminalID = model.TerminalID ?? User.GetTerminalID()?.FirstOrDefault();

            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID.GetValueOrDefault()));

            if (terminal.EnabledFeatures == null || !terminal.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.Checkout))
            {
                return BadRequest(new OperationResponse(Messages.CheckoutFeatureMustBeEnabled, StatusEnum.Error));
            }

            // TODO: validation procedure
            //if (model.AllowPinPad == true && !(model.PaymentRequestAmount > 0))
            //{
            //    return BadRequest(new OperationResponse(Messages.AmountRequiredForPinpadDeal, StatusEnum.Error));
            //}

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            // Check consumer
            var consumer = model.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID), "Consumer") : null;

            model.UpdatePaymentRequest(terminal);

            var newPaymentRequest = mapper.Map<PaymentRequest>(model);
            newPaymentRequest.TerminalID = terminal.TerminalID;

            newPaymentRequest.Calculate();

            // Update details if needed
            newPaymentRequest.DealDetails.UpdateDealDetails(consumer, terminal.Settings, newPaymentRequest, null, false);
            if (consumer != null)
            {
                newPaymentRequest.CardOwnerName = consumer.ConsumerName;
                newPaymentRequest.CardOwnerNationalID = consumer.ConsumerNationalID;
            }
            else
            {
                var consumerID = await CreateConsumer(model, merchantID.Value);
                if (consumerID.HasValue)
                {
                    newPaymentRequest.DealDetails.ConsumerID = consumerID;
                }
            }

            newPaymentRequest.MerchantID = terminal.MerchantID;

            if (!string.IsNullOrWhiteSpace(model.RedirectUrl))
            {
                RedirectUrlHelpers.CheckRedirectUrls(terminal.CheckoutSettings.RedirectUrls, model.RedirectUrl);
            }

            string url = GetPaymentIntentShortUrl(newPaymentRequest, terminal.CheckoutSettings.DefaultLanguage);

            newPaymentRequest.PaymentRequestUrl = url;

            await paymentIntentService.SavePaymentIntent(newPaymentRequest);

            var respObject = new OperationResponse(Transactions.Shared.Messages.PaymentRequestCreated, StatusEnum.Success, newPaymentRequest.PaymentRequestID) { AdditionalData = JObject.FromObject(new { url }) };

            var response = CreatedAtAction(nameof(GetPaymentIntent), new { paymentIntentID = newPaymentRequest.PaymentRequestID }, respObject);

            return response;
        }

        /// <summary>
        /// Delete payment intent
        /// </summary>
        /// <param name="paymentIntentID"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{paymentIntentID}")]
        public async Task<ActionResult<OperationResponse>> DeletePaymentIntent([FromRoute] Guid paymentIntentID)
        {
            var paymentIntent = EnsureExists(await paymentIntentService.GetPaymentIntent(paymentIntentID));

            if (paymentIntent.Status == PaymentRequestStatusEnum.Payed || (int)paymentIntent.Status < 0 || paymentIntent.PaymentTransactionID != null)
            {
                return BadRequest(new OperationResponse($"{Messages.PaymentRequestStatusIsClosed}", StatusEnum.Error, paymentIntent.PaymentRequestID, httpContextAccessor.TraceIdentifier));
            }

            await paymentIntentService.DeletePaymentIntent(paymentIntentID);

            return Ok(new OperationResponse { EntityUID = paymentIntentID, Status = StatusEnum.Success, Message = Messages.PaymentRequestCanceled });
        }

        private async Task<Guid?> CreateConsumer(PaymentRequestCreate transaction, Guid merchantID)
        {
            try
            {
                var consumer = new Merchants.Business.Entities.Billing.Consumer();

                mapper.Map(transaction.DealDetails, consumer);
                consumer.ConsumerName = transaction.DealDetails?.ConsumerName;
                consumer.ConsumerEmail = transaction.DealDetails?.ConsumerEmail;
                consumer.ConsumerNationalID = transaction.CardOwnerNationalID;
                consumer.MerchantID = merchantID;
                consumer.ApplyAuditInfo(httpContextAccessor);

                if (!(!string.IsNullOrWhiteSpace(consumer.ConsumerName) && !string.IsNullOrWhiteSpace(consumer.ConsumerEmail)))
                {
                    return null;
                }

                await consumersService.CreateEntity(consumer);

                return consumer.ConsumerID;
            }
            catch (Exception ex)
            {
                logger.LogError($"Cannot create consumer: {ex.Message}", ex);
                return null;
            }
        }

        private string GetPaymentIntentShortUrl(PaymentRequest dbPaymentRequest, string defaultLanguage)
        {
            var uriBuilder = new UriBuilder(apiSettings.CheckoutPortalUrl);
            uriBuilder.Path = "/i";
            var encrypted = cryptoServiceCompact.EncryptCompact(dbPaymentRequest.PaymentRequestID.ToByteArray());

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["r"] = encrypted;

            if (!string.IsNullOrWhiteSpace(dbPaymentRequest.Language))
            {
                query["l"] = dbPaymentRequest.Language;
            }
            else if (!string.IsNullOrWhiteSpace(defaultLanguage))
            {
                query["l"] = defaultLanguage;
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
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
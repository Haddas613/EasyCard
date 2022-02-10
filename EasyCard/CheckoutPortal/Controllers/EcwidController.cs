using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CheckoutPortal.Models;
using Transactions.Api.Client;
using Shared.Helpers.Security;
using Shared.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Transactions.Api.Models.Checkout;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Shared.Integration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Shared.Api.Configuration;
using CheckoutPortal.Models.Ecwid;
using Ecwid.Configuration;
using Ecwid;
using Ecwid.Models;
using MerchantProfileApi.Client;
using MerchantProfileApi.Models.Billing;
using CheckoutPortal.Services;

namespace CheckoutPortal.Controllers
{
    [AllowAnonymous]
    [Route("ecwid")]
    public class EcwidController : Controller
    {
        private readonly ILogger<EcwidController> logger;
        private readonly IApiClientsFactory apiClientsFactory; 
        private readonly ITerminalApiKeyTokenServiceFactory terminalApiKeyTokenServiceFactory;
        private readonly IMapper mapper;
        private readonly ApiSettings apiSettings;
        private readonly EcwidGlobalSettings ecwidSettings;

        private readonly EcwidConvertor ecwidConvertor;

        public EcwidController(
            ILogger<EcwidController> logger,
            IApiClientsFactory apiClientsFactory,
            ITerminalApiKeyTokenServiceFactory terminalApiKeyTokenServiceFactory,
            IMapper mapper,
            IOptions<ApiSettings> apiSettings,
            IOptions<EcwidGlobalSettings> ecwidSettings)
        {
            this.logger = logger;
            this.apiClientsFactory = apiClientsFactory;
            this.mapper = mapper;
            this.apiSettings = apiSettings.Value;
            this.ecwidSettings = ecwidSettings.Value;
            this.terminalApiKeyTokenServiceFactory = terminalApiKeyTokenServiceFactory;

            ecwidConvertor = new EcwidConvertor(this.ecwidSettings);
        }

        [HttpPost]
        public async Task<IActionResult> Index(EcwidRequestPayload request)
        {
            EcwidPayload ecwidPayload = null;

            try
            {
                ecwidPayload = ecwidConvertor.DecryptEcwidPayload(request.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(EcwidController)} index error: Could not decrypt given request: {request.Data}");
                throw new ApplicationException("Invalid request");
            }

            if (ecwidPayload.MerchantAppSettings == null || string.IsNullOrEmpty(ecwidPayload.MerchantAppSettings?.ApiKey))
            {
                logger.LogError($"{nameof(EcwidController)} index error: Could not retrieve merchant settings: {request.Data}");
                throw new ApplicationException("Invalid request. Merchant settings are not present");
            }

            var tokensService = terminalApiKeyTokenServiceFactory.CreateTokenService(ecwidPayload.MerchantAppSettings.ApiKey);
            var merchantMetadataApiClient = apiClientsFactory.GetMerchantMetadataApiClient(tokensService);

            var customerPayload = ecwidConvertor.GetConsumerRequest(ecwidPayload.Cart.Order);

            var exisingConsumers = await merchantMetadataApiClient.GetConsumers(new ConsumersFilter 
            {
                ExternalReference = customerPayload.ExternalReference,
                Origin = customerPayload.Origin,
            });

            Guid? consumerID = null;

            if (exisingConsumers.NumberOfRecords == 1)
            {
                consumerID = exisingConsumers.Data.First().ConsumerID;
            }
            else if (exisingConsumers.NumberOfRecords > 1)
            {
                throw new ApplicationException("There are several consumers with same card code in ECNG");
            }

            if (consumerID == null)
            {
                var createConsumerResponse = await merchantMetadataApiClient.CreateConsumer(customerPayload);
                consumerID = createConsumerResponse.EntityUID;
            }

            var paymentRequestPayload = ecwidConvertor.GetCreatePaymentRequest(ecwidPayload);
            paymentRequestPayload.DealDetails.ConsumerID = consumerID;

            var transactionsApiClient = apiClientsFactory.GetTransactionsApiClient(tokensService);

            var paymentIntentResponse = await transactionsApiClient.CreatePaymentIntent(paymentRequestPayload);

            var url = paymentIntentResponse.AdditionalData.Value<string>("url");

            return Redirect(url);
        }

        [Route("settings")]
        [HttpGet]
        public async Task<IActionResult> Settings(EcwidSettingsQuery request)
        {
            return View(request);
        }
    }
}

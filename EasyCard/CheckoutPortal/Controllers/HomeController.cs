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
using Shared.Api.Security;
using Microsoft.AspNetCore.Diagnostics;
using System.IO;

namespace CheckoutPortal.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ITransactionsApiClient transactionsApiClient;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IMapper mapper;

        public HomeController(ILogger<HomeController> logger, ITransactionsApiClient transactionsApiClient, ICryptoServiceCompact cryptoServiceCompact, IMapper mapper)
        {
            this.logger = logger;
            this.transactionsApiClient = transactionsApiClient;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index([FromQuery]CardRequest request)
        {
            // redirect url is required if it is not payment request
            if (string.IsNullOrWhiteSpace(request.PaymentRequest) && string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                throw new ApplicationException("redirectUrl is empty");
            }

            Guid? paymentRequestID = null;

            try
            {
                var apiKeyd = cryptoServiceCompact.DecryptCompact(request.ApiKey);
                paymentRequestID = !string.IsNullOrWhiteSpace(request.PaymentRequest) ? new Guid(Convert.FromBase64String(request.PaymentRequest)) : (Guid?)null;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Failed to decrypt request keys - apiKey: {request.ApiKey}, paymentRequestReference: {request.PaymentRequest}");

                throw;
            }

            Transactions.Api.Models.Checkout.CheckoutData checkoutConfig = null;

            try
            {
                checkoutConfig = await transactionsApiClient.GetCheckout(paymentRequestID, request.ApiKey);
            }
            catch(Exception ex)
            {
                // TODO: show error description to customer
                logger.LogError(ex, $"Failed to get payment request data");

                throw;
            }

            // TODO: check if terminal is not active

            // TODO: check payment request state

            if (!string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                checkoutConfig.Settings.RedirectUrls.CheckRedirectUrls(request.RedirectUrl);
            }

            var model = new ChargeViewModel();

            // TODO: default deal description, consumer detals by consumerID
            mapper.Map(request, model);
            mapper.Map(checkoutConfig.PaymentRequest, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Charge(ChargeViewModel chargeViewModel)
        {
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // TODO: show only business exceptions
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, ExceptionMessage = exceptionHandlerPathFeature?.Error?.Message });
        }
    }
}

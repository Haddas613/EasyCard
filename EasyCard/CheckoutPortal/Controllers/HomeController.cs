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

        public async Task<IActionResult> Index([FromQuery]string redirectUrl, [FromQuery]string paymentRequestReference, [FromQuery]string apiKey, [FromQuery]CardRequest request)
        {
            // redirect url is required if it is not payment request
            if (string.IsNullOrWhiteSpace(paymentRequestReference) && string.IsNullOrWhiteSpace(redirectUrl))
            {
                throw new ApplicationException("redirectUrl is empty");
            }

            string apiKeyd = null;
            Guid? paymentRequestID = null;

            try
            {
                apiKeyd = cryptoServiceCompact.DecryptCompact(apiKey);
                paymentRequestID = Guid.Parse(cryptoServiceCompact.DecryptCompact(paymentRequestReference));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Failed to decrypt request keys - apiKey: {apiKey}, paymentRequestReference: {paymentRequestReference}");

                throw;
            }

            var checkoutConfig = await transactionsApiClient.GetCheckout(paymentRequestID, apiKey);

            // TODO: check if terminal is not active

            // TODO: check payment request state

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                checkoutConfig.Settings.RedirectUrls.CheckRedirectUrls(redirectUrl);
            }

            var model = new ChargeViewModel();

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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

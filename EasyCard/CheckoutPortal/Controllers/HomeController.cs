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

        // TODO: preffered language parameter
        // TODO: issueInvoice flag
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index([FromQuery]CardRequest request)
        {
            var checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.RedirectUrl, request.ConsumerID);

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            var model = new ChargeViewModel();

            // TODO: consumer detals by consumerID
            mapper.Map(request, model);
            mapper.Map(checkoutConfig.PaymentRequest, model);
            mapper.Map(checkoutConfig.Settings, model);

            if (checkoutConfig.Consumer != null)
            {
                mapper.Map(checkoutConfig.Consumer, model);

                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    model.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new KeyValuePair<Guid, string>(d.CreditCardTokenID, $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}"));
                }
            }



            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Charge(ChargeViewModel request)
        {
            var checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.RedirectUrl, request.ConsumerID);

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            if (!ModelState.IsValid)
            {
                return View("Index", request);
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            Shared.Api.Models.OperationResponse result = null;

            if (checkoutConfig.PaymentRequest != null)
            {
                var mdel = new Transactions.Api.Models.Transactions.PRCreateTransactionRequest() { CreditCardSecureDetails = new Shared.Integration.Models.CreditCardSecureDetails() };

                // TODO: consumer IP
                mapper.Map(request, mdel);
                mapper.Map(request, mdel.CreditCardSecureDetails);
                mapper.Map(checkoutConfig.PaymentRequest, mdel);
                mapper.Map(checkoutConfig.Settings, mdel);

                result = await transactionsApiClient.CreateTransactionPR(mdel);
                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    return View("PaymentError", new PaymentErrorViewModel { ErrorMessage = result.Message });
                }
            }
            else
            {
                var mdel = new Transactions.Api.Models.Transactions.CreateTransactionRequest()
                {
                    CreditCardSecureDetails = new Shared.Integration.Models.CreditCardSecureDetails(),
                    DealDetails = new Shared.Integration.Models.DealDetails()
                }; 
                mapper.Map(request, mdel);
                mapper.Map(request, mdel.CreditCardSecureDetails);
                mapper.Map(request, mdel.DealDetails);
                mapper.Map(checkoutConfig.Settings, mdel);

                mdel.Calculate();

                result = await transactionsApiClient.CreateTransaction(mdel);
                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    ModelState.AddModelError("Charge", result.Message);
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError(err.Code, err.Description);
                    }

                    return View("Index", request);

                    //return View("PaymentError", new PaymentErrorViewModel { ErrorMessage = result.Message, RequestId = HttpContext.TraceIdentifier });
                }
            }

            if (string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                return RedirectToAction("PaymentResult");
            }
            else
            {
                var redirectUrl = UrlHelper.BuildUrl(request.RedirectUrl, null, new { transactionID = result.EntityUID });

                return Redirect(redirectUrl);
            }
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> PaymentResult()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> PaymentError()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // TODO: show only business exceptions
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, ExceptionMessage = exceptionHandlerPathFeature?.Error?.Message });
        }

        private async Task<Transactions.Api.Models.Checkout.CheckoutData> GetCheckoutData(string apiKey, string paymentRequest, string redirectUrl, Guid? consumerID)
        {
            // redirect url is required if it is not payment request
            if (string.IsNullOrWhiteSpace(paymentRequest) && string.IsNullOrWhiteSpace(redirectUrl))
            {
                logger.LogError($"Checkout data provided by merchant is not valid. RedirectUrl: {redirectUrl}, PaymentRequest: {paymentRequest}");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            Guid? paymentRequestID;
            try
            {
                var apiKeyd = cryptoServiceCompact.DecryptCompact(apiKey);
                paymentRequestID = !string.IsNullOrWhiteSpace(paymentRequest) ? new Guid(Convert.FromBase64String(paymentRequest)) : (Guid?)null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to decrypt request keys. ApiKey: {apiKey}, PaymentRequest: {paymentRequest}");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            Transactions.Api.Models.Checkout.CheckoutData checkoutConfig;
            try
            {
                checkoutConfig = await transactionsApiClient.GetCheckout(paymentRequestID, apiKey, consumerID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get payment request data");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            // TODO: check if terminal is not active

            // Check payment request state. TODO: we can try to pay failed requests again
            if (checkoutConfig.PaymentRequest?.QuickStatus == Transactions.Api.Models.PaymentRequests.Enums.PayReqQuickStatusFilterTypeEnum.Completed)
            {
                throw new BusinessException(Messages.PaymentRequestAlreadyPayed);
            }
            else if (checkoutConfig.PaymentRequest?.QuickStatus == Transactions.Api.Models.PaymentRequests.Enums.PayReqQuickStatusFilterTypeEnum.Canceled)
            {
                throw new BusinessException(Messages.PaymentRequestClosed);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    checkoutConfig.Settings.RedirectUrls.CheckRedirectUrls(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Invalid redirect url");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            return checkoutConfig;
        }
    }
}

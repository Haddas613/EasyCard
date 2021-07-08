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

namespace CheckoutPortal.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ITransactionsApiClient transactionsApiClient;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IMapper mapper;
        private readonly RequestLocalizationOptions localizationOptions;

        public HomeController(
            ILogger<HomeController> logger,
            ITransactionsApiClient transactionsApiClient,
            ICryptoServiceCompact cryptoServiceCompact,
            IMapper mapper,
            IOptions<RequestLocalizationOptions> localizationOptions)
        {
            this.logger = logger;
            this.transactionsApiClient = transactionsApiClient;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.mapper = mapper;
            this.localizationOptions = localizationOptions.Value;
        }

        // TODO: preffered language parameter
        // TODO: issueInvoice flag
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index([FromQuery]CardRequest request)
        {

            if (request == null || !Request.QueryString.HasValue || (Request.Query.Keys.Count == 1 && Request.Query.ContainsKey("culture")))
            {
                return View("PaymentImpossible");
            }

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

            if (checkoutConfig.Consumer != null)
            {
                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    request.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new KeyValuePair<Guid, string>(d.CreditCardTokenID, $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}"));
                }
            }

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            // If token is present and correct, credit card validation is removed from model state
            if (request.CreditCardToken.HasValue || request.PinPad)
            {
                if(!request.PinPad && !request.SavedTokens.Any(t => t.Key == request.CreditCardToken))
                {
                    ModelState.AddModelError(nameof(request.CreditCardToken), "Token is not recognized");
                    logger.LogWarning($"{nameof(Charge)}: unrecognized token from user. Token: {request.CreditCardToken.Value}; PaymentRequestId: {(checkoutConfig.PaymentRequest?.PaymentRequestID.ToString() ?? "-")}");
                    return View("Index", request);
                }

                ModelState[nameof(request.Cvv)].Errors.Clear();
                ModelState[nameof(request.Cvv)].ValidationState = ModelValidationState.Skipped;
                ModelState[nameof(request.CardNumber)].Errors.Clear();
                ModelState[nameof(request.CardNumber)].ValidationState = ModelValidationState.Skipped;
                ModelState[nameof(request.CardExpiration)].Errors.Clear();
                ModelState[nameof(request.CardExpiration)].ValidationState = ModelValidationState.Skipped;
            }

            InstallmentDetails installmentDetails = null;

            if (!checkoutConfig.Settings.TransactionTypes.Any(t => t == request.TransactionType))
            {
                ModelState.AddModelError(nameof(request.TransactionType), $"{request.TransactionType} is not allowed for this transaction");
                return View("Index", request);
            }

            if (request.TransactionType != TransactionTypeEnum.RegularDeal && request.NumberOfPayments.HasValue 
                && request.NumberOfPayments.Value != checkoutConfig.PaymentRequest?.NumberOfPayments)
            {
                if (request.NumberOfPayments > (request.TransactionType == TransactionTypeEnum.Credit ? checkoutConfig.Settings.MaxCreditInstallments : checkoutConfig.Settings.MaxInstallments))
                {
                    ModelState.AddModelError(nameof(request.NumberOfPayments),
                       Resources.CommonResources.NumberOfPaymentsMustBeLessThan.Replace("@min", checkoutConfig.Settings.MaxCreditInstallments.Value.ToString()));
                } 
                else
                {
                    if (checkoutConfig.PaymentRequest != null)
                    {
                        checkoutConfig.PaymentRequest.NumberOfPayments = request.NumberOfPayments.Value;
                        checkoutConfig.PaymentRequest.InitialPaymentAmount =
                            checkoutConfig.PaymentRequest.InstallmentPaymentAmount = checkoutConfig.PaymentRequest.TotalAmount / checkoutConfig.PaymentRequest.NumberOfPayments;

                        installmentDetails = new InstallmentDetails
                        {
                            NumberOfPayments = checkoutConfig.PaymentRequest.NumberOfPayments,
                            InitialPaymentAmount = checkoutConfig.PaymentRequest.InitialPaymentAmount,
                            InstallmentPaymentAmount = checkoutConfig.PaymentRequest.InstallmentPaymentAmount,
                            TotalAmount = checkoutConfig.PaymentRequest.TotalAmount
                        };
                    }
                    else
                    {
                        installmentDetails = new InstallmentDetails
                        {
                            NumberOfPayments = request.NumberOfPayments.Value,
                            InitialPaymentAmount = request.Amount.Value / request.NumberOfPayments.Value,
                            InstallmentPaymentAmount = request.Amount.Value / request.NumberOfPayments.Value,
                            TotalAmount = request.Amount.Value
                        };
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", request);
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            Shared.Api.Models.OperationResponse result = null;

            if (!request.IssueInvoice.HasValue && checkoutConfig.Settings.IssueInvoice != null)
            {
                request.IssueInvoice = checkoutConfig.Settings.IssueInvoice;
            }

            request.AllowPinPad = request.AllowPinPad == true && checkoutConfig.Settings.AllowPinPad == true;

            if (checkoutConfig.PaymentRequest != null)
            {
                var mdel = new Transactions.Api.Models.Transactions.PRCreateTransactionRequest()
                {
                    CreditCardSecureDetails = new CreditCardSecureDetails(),
                    InstallmentDetails = installmentDetails,
                    TransactionType = request.TransactionType
                };

                // TODO: consumer IP
                mapper.Map(request, mdel);
                mapper.Map(request, mdel.CreditCardSecureDetails);
                mapper.Map(checkoutConfig.PaymentRequest, mdel);
                mapper.Map(checkoutConfig.Settings, mdel);

                if (request.CreditCardToken.HasValue || request.PinPad)
                {
                    mdel.CreditCardSecureDetails = null;
                }


                result = await transactionsApiClient.CreateTransactionPR(mdel);

                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    logger.LogError($"{nameof(Charge)}.{nameof(transactionsApiClient.CreateTransactionPR)}: {result.Message}");
                    return View("PaymentError", new PaymentErrorViewModel { ErrorMessage = result.Message });
                }
            }
            else
            {
                var mdel = new Transactions.Api.Models.Transactions.CreateTransactionRequest()
                {
                    CreditCardSecureDetails = new CreditCardSecureDetails(),
                    DealDetails = new DealDetails(),
                    CreditCardToken = request.CreditCardToken,
                    InstallmentDetails = installmentDetails,
                    TransactionType = request.TransactionType
                }; 
                mapper.Map(request, mdel);
                mapper.Map(request, mdel.CreditCardSecureDetails);
                mapper.Map(request, mdel.DealDetails);
                mapper.Map(checkoutConfig.Settings, mdel);

                if (request.CreditCardToken.HasValue || request.PinPad)
                {
                    mdel.CreditCardSecureDetails = null;
                }

                mdel.Calculate();

                result = await transactionsApiClient.CreateTransaction(mdel);
                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    logger.LogError($"{nameof(Charge)}.{nameof(transactionsApiClient.CreateTransaction)}: {result.Message}");

                    ModelState.AddModelError("Charge", result.Message);
                    if (result.Errors?.Count() > 0)
                    {
                        foreach (var err in result.Errors)
                        {
                            ModelState.AddModelError(err.Code, err.Description);
                        }
                    }

                    mapper.Map(checkoutConfig.Settings, request);

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

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CancelPayment(ChargeViewModel request)
        {
            var checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.RedirectUrl, request.ConsumerID);

            if (checkoutConfig.PaymentRequest != null)
            {
                var result = await transactionsApiClient.CancelPaymentRequest(checkoutConfig.PaymentRequest.PaymentRequestID);

                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    logger.LogError($"{nameof(CancelPayment)}: Could not cancel payment request {checkoutConfig.PaymentRequest.PaymentRequestID}. Reason: {result.Message}");
                    return RedirectToAction("PaymentError");
                }
            }

            if (string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                return RedirectToAction("PaymentCanceled");
            }
            else
            {
                var redirectUrl = UrlHelper.BuildUrl(request.RedirectUrl, null, new { rejectionReason = "Canceled by customer" });

                return Redirect(redirectUrl);
            }

        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PaymentResult()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PaymentCanceled()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult VueTest()
        {
            return View(new ChargeViewModel { CardNumber = "1234" });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PaymentError()
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

        public IActionResult ChangeLocalization(string culture)
        {
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

            if (!string.IsNullOrWhiteSpace(culture))
            {
                var allowed = localizationOptions.SupportedCultures.Any(c => c.Name == culture);

                if (allowed)
                {
                    var c = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
                    HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, c);
                }
            }
            else
            {
                return BadRequest();
            }

            return Ok();
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
                if(ex is WebApiClientErrorException webEx)
                {
                    logger.LogError(ex, $"Failed to get payment request data. Reason: {webEx.Response}");
                }
                else
                {
                    logger.LogError(ex, $"Failed to get payment request data. Reason: {ex.Message}");
                }
                
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

            var transactionTypes = new List<TransactionTypeEnum> { TransactionTypeEnum.RegularDeal };

            if (paymentRequestID.HasValue)
            {
                if (checkoutConfig.Settings.MaxInstallments > 1)
                {
                    transactionTypes.Add(TransactionTypeEnum.Installments);
                }
            }
            else
            {
                transactionTypes.Add(TransactionTypeEnum.Installments);
                transactionTypes.Add(TransactionTypeEnum.Credit);
            }

            checkoutConfig.Settings.TransactionTypes = transactionTypes;

            return checkoutConfig;
        }
    }
}

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
        private readonly IHubContext<Hubs.TransactionsHub, Transactions.Shared.Hubs.ITransactionsHub> transactionsHubContext;
        private readonly ApiSettings apiSettings;

        public HomeController(
            ILogger<HomeController> logger,
            ITransactionsApiClient transactionsApiClient,
            ICryptoServiceCompact cryptoServiceCompact,
            IMapper mapper,
            IOptions<RequestLocalizationOptions> localizationOptions,
            IHubContext<Hubs.TransactionsHub, Transactions.Shared.Hubs.ITransactionsHub> transactionsHubContext,
            IOptions<ApiSettings> apiSettings)
        {
            this.logger = logger;
            this.transactionsApiClient = transactionsApiClient;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.mapper = mapper;
            this.localizationOptions = localizationOptions.Value;
            this.transactionsHubContext = transactionsHubContext;
            this.apiSettings = apiSettings.Value;
        }

        /// <summary>
        /// Payment Request short url
        /// </summary>
        /// <param name="r">Payment request</param>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("p")]
        public async Task<IActionResult> ShortUrlPaymentRequest([FromQuery]string r)
        {
            if (string.IsNullOrWhiteSpace(r))
            {
                return RedirectToAction(nameof(Index));
            }

            var paymentRequestDecrypted = cryptoServiceCompact.DecryptCompactToBytes(r);

            if (paymentRequestDecrypted == null || paymentRequestDecrypted.Length != 16)
            {
                return RedirectToAction(nameof(Index));
            }

            Guid paymentRequestId = new Guid(paymentRequestDecrypted);

            var checkoutConfig = await GetCheckoutData(paymentRequestId);

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            return await IndexViewResult(checkoutConfig);
        }

        /// <summary>
        /// Payment Intent short url
        /// </summary>
        /// <param name="r">Payment intent</param>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("i")]
        public async Task<IActionResult> ShortUrlPaymentIntent([FromQuery]string r)
        {
            if (string.IsNullOrWhiteSpace(r))
            {
                return RedirectToAction(nameof(Index));
            }

            var paymentIntentDecrypted = cryptoServiceCompact.DecryptCompactToBytes(r);

            if (paymentIntentDecrypted == null || paymentIntentDecrypted.Length != 16)
            {
                return RedirectToAction(nameof(Index));
            }

            Guid paymentIntentID = new Guid(paymentIntentDecrypted);

            var checkoutConfig = await GetCheckoutData(paymentIntentID, null, true);
            checkoutConfig.PaymentIntentID = paymentIntentID;

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            return await IndexViewResult(checkoutConfig);
        }

        // TODO: preffered language parameter
        // TODO: issueInvoice flag
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index([FromQuery] CardRequest request)
        {

            if (request == null || !Request.QueryString.HasValue || (Request.Query.Keys.Count == 1 && Request.Query.ContainsKey("culture")))
            {
                return View("PaymentImpossible");
            }

            var checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);

            return await IndexViewResult(checkoutConfig, request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Charge(ChargeViewModel request)
        {
            CheckoutData checkoutConfig;
            bool isPaymentIntent = request.PaymentIntent != null;

            if (request.ApiKey != null)
            {
                checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);
            }
            else
            {
                if(!Guid.TryParse(isPaymentIntent ? request.PaymentIntent : request.PaymentRequest, out var id))
                {
                    throw new BusinessException(Messages.InvalidCheckoutData);
                }

                checkoutConfig = await GetCheckoutData(id, request.RedirectUrl, isPaymentIntent);
            }

            if (checkoutConfig.Consumer != null)
            {
                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    request.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new KeyValuePair<Guid, string>(d.CreditCardTokenID, $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}"));
                }
            }

            if (checkoutConfig.PaymentRequest != null)
            {
                if (checkoutConfig.PaymentRequest.OnlyAddCard)
                {
                    request.SaveCreditCard = true;
                    request.Amount = null;
                }
                else
                {
                    if (!checkoutConfig.PaymentRequest.UserAmount && checkoutConfig.PaymentRequest.TotalAmount > 0)
                    {
                        request.Amount = checkoutConfig.PaymentRequest.TotalAmount;
                        ModelState[nameof(request.Amount)].Errors.Clear();
                        ModelState[nameof(request.Amount)].ValidationState = ModelValidationState.Skipped;
                    }
                }
            }

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            if (checkoutConfig?.Settings.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.CreditCardTokens) == false
                && (request.SaveCreditCard))
            {
                ModelState.AddModelError(nameof(request.SaveCreditCard), "Saving credit cards is not allowed for this terminal");
                return await IndexViewResult(checkoutConfig, request);
            }

            // If token is present and correct, credit card validation is removed from model state
            if (request.CreditCardToken.HasValue || request.PinPad)
            {
                if (!request.PinPad && !request.SavedTokens.Any(t => t.Key == request.CreditCardToken))
                {
                    ModelState.AddModelError(nameof(request.CreditCardToken), "Token is not recognized");

                    logger.LogWarning($"{nameof(Charge)}: unrecognized token from user. Token: {request.CreditCardToken.Value}; PaymentRequestId: {(checkoutConfig.PaymentRequest?.PaymentRequestID.ToString() ?? "-")}");
                    return await IndexViewResult(checkoutConfig, request);
                }

                if (request.PinPad)
                {
                    if (string.IsNullOrWhiteSpace(request.PinPadDeviceID))
                    {
                        ModelState.AddModelError(nameof(request.PinPadDeviceID), "PinPad device is not supplied");
                    }
                    else
                    {
                        if (!checkoutConfig.Settings.PinPadDevices.Any(d => d.DeviceID == request.PinPadDeviceID))
                        {
                            ModelState.AddModelError(nameof(request.PinPadDeviceID), "PinPad device is not recognized");
                        }
                    }
                }

                if (ModelState[nameof(request.Cvv)] != null)
                {
                    ModelState[nameof(request.Cvv)]?.Errors?.Clear();
                    ModelState[nameof(request.Cvv)].ValidationState = ModelValidationState.Skipped;
                }
                if (ModelState[nameof(request.CardNumber)] != null)
                {
                    ModelState[nameof(request.CardNumber)]?.Errors?.Clear();
                    ModelState[nameof(request.CardNumber)].ValidationState = ModelValidationState.Skipped;
                }
                if (ModelState[nameof(request.CardExpiration)] != null)
                {
                    ModelState[nameof(request.CardExpiration)]?.Errors?.Clear();
                    ModelState[nameof(request.CardExpiration)].ValidationState = ModelValidationState.Skipped;
                }
            }

            if (string.IsNullOrWhiteSpace(request.Cvv) && checkoutConfig.Settings.CvvRequired == true)
            {
                ModelState.AddModelError(nameof(request.Cvv), "CVV is required");
            }

            if (string.IsNullOrWhiteSpace(request.NationalID) && checkoutConfig.Settings.NationalIDRequired == true)
            {
                ModelState.AddModelError(nameof(request.NationalID), Resources.CommonResources.NationalIDInvalid);
            } 
            else if (request.NationalID != null && !IsraelNationalIdHelpers.Valid(request.NationalID))
            {
                ModelState.AddModelError(nameof(request.NationalID), Resources.CommonResources.NationalIDInvalid);
            }

            InstallmentDetails installmentDetails = null;

            if (!checkoutConfig.Settings.TransactionTypes.Any(t => t == request.TransactionType))
            {
                ModelState.AddModelError(nameof(request.TransactionType), $"{request.TransactionType} is not allowed for this transaction");

                return await IndexViewResult(checkoutConfig, request);
            }

            if (request.TransactionType == TransactionTypeEnum.Installments || request.TransactionType == TransactionTypeEnum.Credit)
            {
                var maxNumberOfPayments = (request.TransactionType == TransactionTypeEnum.Credit ? checkoutConfig.Settings.MaxCreditInstallments : checkoutConfig.Settings.MaxInstallments);
                if (request.NumberOfPayments > maxNumberOfPayments)
                {
                    ModelState.AddModelError(nameof(request.NumberOfPayments), 
                        Resources.CommonResources.NumberOfPaymentsMustBeLessThan.Replace("@max", maxNumberOfPayments.GetValueOrDefault(0).ToString()));
                }
                else
                {
                    installmentDetails = new InstallmentDetails
                    {
                        NumberOfPayments = request.NumberOfPayments.Value,
                        TotalAmount = request.Amount.Value
                    };

                    if (request.InitialPaymentAmount.HasValue)
                    {
                        var installmentPaymentAmount = Math.Floor(request.Amount.Value / request.NumberOfPayments.Value);

                        installmentDetails.InitialPaymentAmount = request.InitialPaymentAmount.Value;
                        installmentDetails.InstallmentPaymentAmount = request.InstallmentPaymentAmount.HasValue ? 
                            request.InstallmentPaymentAmount.Value : (request.Amount.Value - request.InitialPaymentAmount.Value) / (request.NumberOfPayments.Value - 1);

                        if (installmentDetails.InitialPaymentAmount + installmentDetails.InstallmentPaymentAmount * (request.NumberOfPayments.Value - 1) != request.Amount.Value 
                            || installmentDetails.InitialPaymentAmount < 0.1m || installmentDetails.InstallmentPaymentAmount < 0.1m)
                        {
                            ModelState.AddModelError(nameof(InstallmentDetails), Messages.TotalAmountIsInvalid);
                        }
                    }
                    else
                    {
                        var installmentPaymentAmount = Math.Floor(request.Amount.Value / request.NumberOfPayments.Value);

                        installmentDetails.InitialPaymentAmount = request.Amount.Value - installmentPaymentAmount * (request.NumberOfPayments.Value - 1);
                        installmentDetails.InstallmentPaymentAmount = installmentPaymentAmount;
                    }
                }
            }

            // only add card case
            if (request.Amount.GetValueOrDefault() == 0 && request.SaveCreditCard)
            {
                ModelState[nameof(request.Amount)].Errors.Clear();
                ModelState[nameof(request.Amount)].ValidationState = ModelValidationState.Skipped;
            }

            if (!ModelState.IsValid)
            {
                return await IndexViewResult(checkoutConfig, request);
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
                mapper.Map(checkoutConfig.Settings, mdel);
                mapper.Map(checkoutConfig.PaymentRequest, mdel);

                if (checkoutConfig.PaymentIntentID != null)
                {
                    mdel.PaymentIntentID = checkoutConfig.PaymentIntentID;
                }
                else
                {
                    mdel.PaymentRequestID = checkoutConfig.PaymentRequest.PaymentRequestID;
                }

                if (request.CreditCardToken.HasValue || request.PinPad)
                {
                    mdel.CreditCardSecureDetails = null;
                }

                if (checkoutConfig.PaymentRequest.UserAmount)
                {
                    mdel.PaymentRequestAmount = request.Amount ?? checkoutConfig.PaymentRequest.PaymentRequestAmount;
                    mdel.NetTotal = Math.Round(mdel.PaymentRequestAmount.GetValueOrDefault() / (1m + mdel.VATRate.GetValueOrDefault()), 2, MidpointRounding.AwayFromZero);
                    mdel.VATTotal = mdel.PaymentRequestAmount.GetValueOrDefault() - mdel.NetTotal;
                }

                result = await transactionsApiClient.CreateTransactionPR(mdel);

                if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                {
                    logger.LogError($"{nameof(Charge)}.{nameof(transactionsApiClient.CreateTransactionPR)}: {result.Message}");

                    ModelState.AddModelError("Charge", result.Message);
                    if (result.Errors?.Count() > 0)
                    {
                        foreach (var err in result.Errors)
                        {
                            ModelState.AddModelError(err.Code, err.Description);
                        }
                    }

                    return await IndexViewResult(checkoutConfig, request);
                }
            }
            else//PR IS NULL
            {
                var mdel = new Transactions.Api.Models.Transactions.CreateTransactionRequest()
                {
                    CreditCardSecureDetails = new CreditCardSecureDetails(),
                    DealDetails = new DealDetails(),
                    CreditCardToken = request.CreditCardToken,
                    InstallmentDetails = installmentDetails,
                    TransactionType = request.TransactionType,
                    PaymentIntentID = isPaymentIntent ? new Guid(request.PaymentIntent) : (Guid?)null
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

                    return await IndexViewResult(checkoutConfig, request);
                }
            }

            var redirectUrl = request.RedirectUrl ?? checkoutConfig.PaymentRequest.RedirectUrl;

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return RedirectToAction("PaymentResult");
            }
            else
            {
                if (checkoutConfig.Settings?.LegacyRedirectResponse == true)
                {
                    if (checkoutConfig.PaymentRequest?.OnlyAddCard == true)
                    {
                        return Redirect(UrlHelper.BuildUrl(redirectUrl, null, new { tokenID = result.EntityUID }));
                    }
                    else
                    {
                        var paymentTransaction = await transactionsApiClient.GetTransaction(result.EntityUID);

                        redirectUrl = UrlHelper.BuildUrl(redirectUrl, null, LegacyQueryStringConvertor.GetLegacyQueryString(request, paymentTransaction));

                        return Redirect(redirectUrl);
                    }
                }
                else
                {
                    if (checkoutConfig.PaymentRequest?.OnlyAddCard == true)
                    {
                        return Redirect(UrlHelper.BuildUrl(redirectUrl, null, new { tokenID = result.EntityUID }));
                    }
                    else
                    {
                        return Redirect(UrlHelper.BuildUrl(redirectUrl, null, new { transactionID = result.EntityUID }));
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CancelPayment(ChargeViewModel request)
        {
            CheckoutData checkoutConfig;

            if (request.ApiKey != null)
            {
                checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);
            }
            else
            {
                //TODO
                //bool isPaymentIntent = request.PaymentIntent != null;

                //if (!Guid.TryParse(isPaymentIntent ? request.PaymentIntent : request.PaymentRequest, out var id))
                //{
                //    throw new BusinessException(Messages.InvalidCheckoutData);
                //}

                if (!Guid.TryParse(request.PaymentRequest, out var id))
                {
                    throw new BusinessException(Messages.InvalidCheckoutData);
                }
                checkoutConfig = await GetCheckoutData(id, request.RedirectUrl, false);
            }

            if (checkoutConfig.PaymentRequest != null)
            {
                // TODO: cancel payment intent
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
                    HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, c, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        SameSite = SameSiteMode.None,
                        Secure = true
                    });
                }
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        private async Task<IActionResult> IndexViewResult(CheckoutData checkoutConfig, CardRequest request = null)
        {
            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            var model = new ChargeViewModel();

            // TODO: consumer detals by consumerID
            if (request != null)
            {
                mapper.Map(request, model);

                if (!request.Amount.HasValue && request.UserAmount)
                {
                    request.Amount = 0;
                    model.Amount = 0;
                    model.TotalAmount = 0;
                }
                else if(request.Amount.GetValueOrDefault(0) == 0 && !request.UserAmount)
                {
                    throw new BusinessException(Messages.InvalidCheckoutData);
                }
            }

            mapper.Map(checkoutConfig.PaymentRequest, model);
            mapper.Map(checkoutConfig.Settings, model);
            model.PaymentIntent = checkoutConfig.PaymentIntentID?.ToString();

            if (checkoutConfig.Consumer != null)
            {
                mapper.Map(checkoutConfig.Consumer, model);

                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    model.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new KeyValuePair<Guid, string>(d.CreditCardTokenID, $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}"));
                }
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            return await Task.FromResult(View(nameof(Index), model));
        }

        private async Task<IActionResult> IndexViewResult(CheckoutData checkoutConfig, ChargeViewModel model)
        {
            mapper.Map(checkoutConfig.PaymentRequest, model);
            mapper.Map(checkoutConfig.Settings, model);
            model.PaymentIntent = checkoutConfig.PaymentIntentID?.ToString();

            if (checkoutConfig.Consumer != null)
            {
                mapper.Map(checkoutConfig.Consumer, model);

                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    model.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new KeyValuePair<Guid, string>(d.CreditCardTokenID, $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}"));
                }
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            return await Task.FromResult(View(nameof(Index), model));
        }

        /// <summary>
        /// Preferred method. To be used when payment request id or payment intent id are available.
        /// </summary>
        /// <param name="id">Payment request or payment intent id (depends on <paramref name="isPaymentIntent"/>)</param>
        /// <param name="redirectUrl">Redirect url (optional)</param>
        /// <param name="isPaymentIntent">Type flag. If set to true, Id will be treated as if it's payment intent</param>
        /// <returns></returns>
        private async Task<CheckoutData> GetCheckoutData(Guid id, string redirectUrl = null, bool isPaymentIntent = false)
        {
            Guid? paymentRequestID = !isPaymentIntent ? id : default(Guid?);
            Guid? paymentIntentID = isPaymentIntent ? id : default(Guid?);

            CheckoutData checkoutConfig;
            try
            {
                checkoutConfig = await transactionsApiClient.GetCheckout(paymentRequestID, paymentIntentID, null);
            }
            catch (Exception ex)
            {
                if (ex is WebApiClientErrorException webEx)
                {
                    logger.LogError(ex, $"Failed to get checkout data. Reason: {webEx.Response}");
                }
                else
                {
                    logger.LogError(ex, $"Failed to get checkout data. Reason: {ex.Message}");
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

            var transactionTypes = new List<TransactionTypeEnum> { TransactionTypeEnum.RegularDeal, TransactionTypeEnum.Immediate };

            if (checkoutConfig.PaymentRequest != null)
            {
                if (checkoutConfig.Settings.MaxInstallments > 1)
                {
                    transactionTypes.Add(TransactionTypeEnum.Installments);
                }

                if (checkoutConfig.Settings.MaxCreditInstallments > 1)
                {
                    transactionTypes.Add(TransactionTypeEnum.Credit);
                }
            }
            else
            {
                transactionTypes.Add(TransactionTypeEnum.Installments);
                transactionTypes.Add(TransactionTypeEnum.Credit);
            }

            checkoutConfig.Settings.TransactionTypes = transactionTypes;
            checkoutConfig.Settings.BlobBaseAddress = apiSettings.BlobBaseAddress;
            return checkoutConfig;
        }

        /// <summary>
        /// Legacy method with ApiKey. To be used only when payment request id or payment intent id are not available.
        /// </summary>
        /// <param name="apiKey">Api key</param>
        /// <param name="paymentRequestB64">Base64 encoded payment request id</param>
        /// <param name="paymentIntentB64">Base64 encoded payment intent id</param>
        /// <param name="redirectUrl">Redirect url (required)</param>
        /// <returns></returns>
        private async Task<CheckoutData> GetCheckoutData(string apiKey, string paymentRequestB64, string paymentIntentB64, string redirectUrl)
        {
            // redirect url is required if it is not payment request
            if (string.IsNullOrWhiteSpace(paymentRequestB64) && string.IsNullOrWhiteSpace(redirectUrl) && string.IsNullOrWhiteSpace(paymentIntentB64))
            {
                logger.LogError($"Checkout data provided by merchant is not valid. RedirectUrl: {redirectUrl}, PaymentRequest: {paymentRequestB64}, PaymentIntent: {paymentIntentB64}");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            Guid? paymentRequestID;
            Guid? paymentIntentID;
            try
            {
                var apiKeyd = cryptoServiceCompact.DecryptCompact(apiKey);
                paymentRequestID = !string.IsNullOrWhiteSpace(paymentRequestB64) ? new Guid(Convert.FromBase64String(paymentRequestB64)) : (Guid?)null;
                paymentIntentID = !string.IsNullOrWhiteSpace(paymentIntentB64) ? new Guid(Convert.FromBase64String(paymentIntentB64)) : (Guid?)null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to decrypt request keys. ApiKey: {apiKey}, PaymentRequest: {paymentRequestB64}, PaymentIntent: {paymentIntentB64}");
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            CheckoutData checkoutConfig;
            try
            {
                checkoutConfig = await transactionsApiClient.GetCheckout(paymentRequestID, paymentIntentID, apiKey);
            }
            catch (Exception ex)
            {
                if (ex is WebApiClientErrorException webEx)
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

            var transactionTypes = new List<TransactionTypeEnum> { TransactionTypeEnum.RegularDeal, TransactionTypeEnum.Immediate };

            if (checkoutConfig.PaymentRequest != null)
            {
                if (checkoutConfig.Settings.MaxInstallments > 1)
                {
                    transactionTypes.Add(TransactionTypeEnum.Installments);
                }

                if (checkoutConfig.Settings.MaxCreditInstallments > 1)
                {
                    transactionTypes.Add(TransactionTypeEnum.Credit);
                }
            }
            else
            {
                transactionTypes.Add(TransactionTypeEnum.Installments);
                transactionTypes.Add(TransactionTypeEnum.Credit);
            }

            checkoutConfig.Settings.TransactionTypes = transactionTypes;
            checkoutConfig.Settings.BlobBaseAddress = apiSettings.BlobBaseAddress;

            return checkoutConfig;
        }
    }
}

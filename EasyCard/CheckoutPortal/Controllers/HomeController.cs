﻿using AutoMapper;
using CheckoutPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api.Configuration;
using Shared.Api.Utilities;
using Shared.Helpers;
using Shared.Helpers.Security;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Transactions.Api.Client;
using Transactions.Api.Models.Checkout;
using Transactions.Api.Models.External.Bit;
using CheckoutPortal.Services;
using CheckoutPortal.Models.Bit;
using System.Threading;
using System.Globalization;
using CheckoutPortal.Resources;
using System.IO;
using System.Text;
using Transactions.Api.Models.Transactions;

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
        public async Task<IActionResult> ShortUrlPaymentRequest([FromQuery] string r, string l)
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

            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            return IndexViewResult(checkoutConfig, queryLang: l);
        }

        /// <summary>
        /// Payment Intent short url
        /// </summary>
        /// <param name="r">Payment intent</param>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("i")]
        public async Task<IActionResult> ShortUrlPaymentIntent([FromQuery] string r, string l)
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

            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            checkoutConfig.PaymentIntentID = paymentIntentID;

            // TODO: add merchant site origin instead of unsafe-inline
            //Response.Headers.Add("Content-Security-Policy", "default-src https:; script-src https: 'unsafe-inline'; style-src https: 'unsafe-inline'");

            return IndexViewResult(checkoutConfig, queryLang: l);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index([FromQuery] CardRequest request)
        {
            if (request == null || !Request.QueryString.HasValue || (Request.Query.Keys.Count == 1 && Request.Query.ContainsKey("culture")))
            {
                return View("PaymentImpossible");
            }

            var checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);

            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            return IndexViewResult(checkoutConfig, request);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Charge(ChargeViewModel request)
        {
            bool isPaymentIntent = request.PaymentIntent != null;
            CheckoutData checkoutConfig = await GetCheckoutConfigForCharge(request, isPaymentIntent);


            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            if (checkoutConfig.Consumer != null)
            {
                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    request.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new SavedTokenInfo
                    {
                        CreditCardTokenID = d.CreditCardTokenID,
                        Label = $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}",
                        Created = d.Created,
                    });
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

            //TODO: Bit payment validation when feature is added
            //if (checkoutConfig?.Settings.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.Bit) == false
            //    && (request.PayWithBit))
            //{
            //    ModelState.AddModelError(nameof(request.SaveCreditCard), "Bit payments are not allowed for this terminal");
            //    return await IndexViewResult(checkoutConfig, request);
            //}

            if (request.PayWithBit && request.TransactionType.HasValue && request.TransactionType != TransactionTypeEnum.RegularDeal)
            {
                ModelState.AddModelError(nameof(request.PayWithBit), "Only regular deals are allowed for Bit payments");
                return IndexViewResult(checkoutConfig, request);
            }

            if (checkoutConfig?.Settings.EnabledFeatures.Any(f => f == Merchants.Shared.Enums.FeatureEnum.CreditCardTokens) == false
                && (request.SaveCreditCard))
            {
                ModelState.AddModelError(nameof(request.SaveCreditCard), "Saving credit cards is not allowed for this terminal");
                return IndexViewResult(checkoutConfig, request);
            }

            if (checkoutConfig.Settings.ConsumerDataReadonly == true)
            {
                if (ModelState[nameof(request.Name)] != null)
                {
                    ModelState[nameof(request.Name)]?.Errors?.Clear();
                    ModelState[nameof(request.Name)].ValidationState = ModelValidationState.Skipped;
                }
            }

            if (request.PayWithBit)
            {
                request.PinPad = false;
                request.PinPadDeviceID = null;

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
            else if (request.CreditCardToken.HasValue || request.PinPad) // If token is present and correct, credit card validation is removed from model state
            {
                if (!request.PayWithBit && !request.PinPad && !request.SavedTokens.Any(t => t.CreditCardTokenID == request.CreditCardToken))
                {
                    ModelState.AddModelError(nameof(request.CreditCardToken), "Token is not recognized");

                    logger.LogWarning($"{nameof(Charge)}: unrecognized token from user. Token: {request.CreditCardToken.Value}; PaymentRequestId: {(checkoutConfig.PaymentRequest?.PaymentRequestID.ToString() ?? "-")}");
                    return IndexViewResult(checkoutConfig, request);
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
            else
            {
                if (string.IsNullOrWhiteSpace(request.Cvv) && checkoutConfig.Settings.CvvRequired == true)
                {
                    ModelState.AddModelError(nameof(request.Cvv), "CVV is required");
                }
            }

            if (checkoutConfig.Settings.ConsumerDataReadonly != true)
            {
                if (string.IsNullOrWhiteSpace(request.NationalID) && checkoutConfig.Settings.NationalIDRequired == true)
                {
                    ModelState.AddModelError(nameof(request.NationalID), Resources.CommonResources.NationalIDInvalid);
                }
                else if (request.NationalID != null && !IsraelNationalIdHelpers.Valid(request.NationalID))
                {
                    ModelState.AddModelError(nameof(request.NationalID), Resources.CommonResources.NationalIDInvalid);
                }

                if (string.IsNullOrWhiteSpace(request.Email) && checkoutConfig.Settings.EmailRequired == true)
                {
                    ModelState.AddModelError(nameof(request.Email), Resources.CommonResources.EmailRequired);
                }

                if (string.IsNullOrWhiteSpace(request.Phone) && checkoutConfig.Settings.PhoneRequired == true)
                {
                    ModelState.AddModelError(nameof(request.Phone), Resources.CommonResources.PhoneRequired);
                }
            }

            InstallmentDetails installmentDetails = null;

            if (request.TransactionType != null && !checkoutConfig.Settings.TransactionTypes.Any(t => t == request.TransactionType))
            {
                ModelState.AddModelError(nameof(request.TransactionType), $"{request.TransactionType} is not allowed for this transaction");

                return IndexViewResult(checkoutConfig, request);
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
                        var installmentPaymentAmount = Math.Floor(request.Amount.GetValueOrDefault() / request.NumberOfPayments.GetValueOrDefault());

                        installmentDetails.InitialPaymentAmount = request.Amount.GetValueOrDefault() - installmentPaymentAmount * (request.NumberOfPayments.GetValueOrDefault() - 1);
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
                return IndexViewResult(checkoutConfig, request);
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            Shared.Api.Models.OperationResponse result = null;

            if (!request.IssueInvoice.HasValue && checkoutConfig.Settings.IssueInvoice != null)
            {
                request.IssueInvoice = checkoutConfig.Settings.IssueInvoice;
            }

            request.AllowPinPad = request.AllowPinPad == true && checkoutConfig.Settings.AllowPinPad == true;

            if (request.PayWithBit)
            {
                var mdel = new Transactions.Api.Models.Transactions.CreateTransactionRequest()
                {
                    CreditCardSecureDetails = null,
                    DealDetails = new DealDetails(),
                    CreditCardToken = null,
                    InstallmentDetails = null,
                    TransactionType = TransactionTypeEnum.RegularDeal,
                };

                if (checkoutConfig.PaymentIntentID != null)
                {
                    mdel.PaymentIntentID = checkoutConfig.PaymentIntentID;
                }
                else
                {
                    mdel.PaymentRequestID = checkoutConfig.PaymentRequest?.PaymentRequestID;
                }

                mapper.Map(request, mdel);
                mapper.Map(request, mdel.DealDetails);
                mapper.Map(checkoutConfig.Settings, mdel);
                mapper.Map(checkoutConfig.Consumer, mdel);
                mapper.Map(checkoutConfig.Consumer, mdel.CreditCardSecureDetails);

                if (checkoutConfig.PaymentRequest.UserAmount)
                {
                    // TODO: move it to appropriate calculator
                    mdel.TransactionAmount = request.Amount ?? checkoutConfig.PaymentRequest.PaymentRequestAmount;
                    mdel.NetTotal = Math.Round(mdel.TransactionAmount / (1m + mdel.VATRate.GetValueOrDefault()), 2, MidpointRounding.AwayFromZero);
                    mdel.VATTotal = mdel.TransactionAmount - mdel.NetTotal;
                }
                //else
                //{
                //    mdel.Calculate();
                //}

                mdel.Origin = request.Origin ?? checkoutConfig.PaymentRequest.Origin ?? Request.GetTypedHeaders().Referer?.Host;

                result = await transactionsApiClient.InitiateBitTransaction(mdel);
            }
            else if (checkoutConfig.PaymentRequest != null)
            {
                var mdel = new Transactions.Api.Models.Transactions.PRCreateTransactionRequest()
                {
                    CreditCardSecureDetails = new CreditCardSecureDetails(),
                    InstallmentDetails = installmentDetails,
                    TransactionType = request.TransactionType.GetValueOrDefault()
                };

                // TODO: remove this block at all
                mapper.Map(checkoutConfig.Settings, mdel);
                mapper.Map(checkoutConfig.PaymentRequest, mdel);

                // TODO: consumer IP
                mapper.Map(request, mdel);
                mapper.Map(request, mdel.CreditCardSecureDetails); 
                mapper.Map(checkoutConfig.Consumer, mdel);
                mapper.Map(checkoutConfig.Consumer, mdel.CreditCardSecureDetails);

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
                    // TODO: move it to appropriate calculator
                    mdel.PaymentRequestAmount = request.Amount ?? checkoutConfig.PaymentRequest.PaymentRequestAmount;
                    mdel.NetTotal = Math.Round(mdel.PaymentRequestAmount.GetValueOrDefault() / (1m + mdel.VATRate.GetValueOrDefault()), 2, MidpointRounding.AwayFromZero);
                    mdel.VATTotal = mdel.PaymentRequestAmount.GetValueOrDefault() - mdel.NetTotal;
                }

                mdel.Origin = request.Origin ?? checkoutConfig.PaymentRequest.Origin ?? Request.GetTypedHeaders().Referer?.Host;

                result = await transactionsApiClient.CreateTransactionPR(mdel);
            }
            else //PR IS NULL
            {
                var mdel = new Transactions.Api.Models.Transactions.CreateTransactionRequest()
                {
                    CreditCardSecureDetails = new CreditCardSecureDetails(),
                    DealDetails = new DealDetails(),
                    CreditCardToken = request.CreditCardToken,
                    InstallmentDetails = installmentDetails,
                    TransactionType = request.TransactionType.GetValueOrDefault(),
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

                //mdel.Calculate();

                mdel.Origin = Request.GetTypedHeaders().Referer?.Host;

                result = await transactionsApiClient.CreateTransaction(mdel);
            }

            if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                logger.LogError($"{nameof(Charge)}.{nameof(transactionsApiClient.CreateTransactionPR)}: {result.Message}");

                if (request.PayWithBit)
                {
                    return Json(result);
                }
                else
                {

                    ModelState.AddModelError("Charge", result.Message);
                    if (result.Errors?.Count() > 0)
                    {
                        foreach (var err in result.Errors)
                        {
                            ModelState.AddModelError(err.Code, err.Description ?? "Unknown Error");
                        }
                    }

                    return IndexViewResult(checkoutConfig, request);
                }
            }

            if (request.PayWithBit)
            {
                var bitResult = result as InitialBitOperationResponse;

                if (DeviceDetectUtilities.IsMobileBrowser(Request))
                {
                    var bitTransaction = await transactionsApiClient.GetBitTransaction(new GetBitTransactionQuery
                    {
                        PaymentInitiationId = bitResult.BitPaymentInitiationId,
                        TransactionSerialId = bitResult.BitTransactionSerialId,
                        PaymentTransactionID = result.EntityUID.Value,
                    });
                    var bitCompletedUrl = HttpUtility.UrlEncode($"{apiSettings.CheckoutPortalUrl}/bit-completed" +
                       $"?PaymentInitiationId={bitTransaction.PaymentInitiationId}&TransactionSerialId={bitTransaction.TransactionSerialId}" +
                       $"&PaymentIntent={request.PaymentIntent}&PaymentRequest={request.PaymentRequest}&ApiKey={request.ApiKey}" +
                       $"&PaymentTransactionID={result.EntityUID.Value}&RedirectUrl={request.RedirectUrl}");

                    //URL needs to be double encoded
                    var scheme = HttpUtility.UrlEncode($"&return_scheme={bitCompletedUrl}");
                    string mobileRedirectUrl = null;

                    if (Request.IsIOS())
                    {
                        mobileRedirectUrl = bitTransaction.ApplicationSchemeIos + scheme;
                    }
                    else
                    {
                        mobileRedirectUrl = bitTransaction.ApplicationSchemeAndroid + scheme;
                    }

                    return Json(new BitPaymentMobileModel
                    {
                        MobileRedirectUrl = mobileRedirectUrl,
                    });
                }

                return Json(new BitPaymentViewModel
                {
                    PaymentInitiationId = bitResult.BitPaymentInitiationId,
                    TransactionSerialId = bitResult.BitTransactionSerialId,
                    RedirectUrl = request.RedirectUrl ?? checkoutConfig.PaymentRequest.RedirectUrl,
                    PaymentTransactionID = result.EntityUID.Value,
                    PaymentIntent = checkoutConfig.PaymentIntentID?.ToString(),
                    ApiKey = request.ApiKey,
                    PaymentRequest = checkoutConfig.PaymentRequest?.PaymentRequestID.ToString(),
                });
            }

            var redirectUrl = request.RedirectUrl ?? checkoutConfig.PaymentRequest?.RedirectUrl;

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                if (!string.IsNullOrWhiteSpace(request.QueryLang))
                {
                    return RedirectToAction("PaymentResult", new { l = request.QueryLang });
                }

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

        private async Task<CheckoutData> GetCheckoutConfigForCharge(ChargeViewModel request, bool isPaymentIntent)
        {
            CheckoutData checkoutConfig;

            if (request.ApiKey != null)
            {
                checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);
            }
            else
            {
                if (!Guid.TryParse(isPaymentIntent ? request.PaymentIntent : request.PaymentRequest, out var id))
                {
                    throw new BusinessException(Messages.InvalidCheckoutData);
                }

                checkoutConfig = await GetCheckoutData(id, request.RedirectUrl, isPaymentIntent);
            }

            return checkoutConfig;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CancelPayment(ChargeViewModel request)
        {
            CheckoutData checkoutConfig;

            bool isPaymentIntent = request.PaymentIntent != null;

            if (request.ApiKey != null)
            {
                checkoutConfig = await GetCheckoutData(request.ApiKey, request.PaymentRequest, request.PaymentIntent, request.RedirectUrl);
            }
            else
            {
                if (!Guid.TryParse(isPaymentIntent ? request.PaymentIntent : request.PaymentRequest, out var id))
                {
                    throw new BusinessException(Messages.InvalidCheckoutData);
                }

                checkoutConfig = await GetCheckoutData(id, request.RedirectUrl, isPaymentIntent);
            }

            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            var result = isPaymentIntent ? await transactionsApiClient.CancelPaymentIntent(Guid.Parse(request.PaymentIntent))
                : await transactionsApiClient.CancelPaymentRequest(checkoutConfig.PaymentRequest.PaymentRequestID);

            if (result.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                if (isPaymentIntent)
                {
                    logger.LogError($"{nameof(CancelPayment)}: Could not cancel payment intent {request.PaymentIntent}. Reason: {result.Message}");
                }
                else
                {
                    logger.LogError($"{nameof(CancelPayment)}: Could not cancel payment request {checkoutConfig.PaymentRequest.PaymentRequestID}. Reason: {result.Message}");
                }
                return RedirectToAction("PaymentError");
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
        [Route("bit-completed")]
        public async Task<IActionResult> BitPaymentCompleted([FromQuery] BitPaymentViewModel request)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionSerialId) || string.IsNullOrWhiteSpace(request.PaymentInitiationId))
            {
                logger.LogError($"{nameof(BitPaymentCompleted)}: Incorrect request");
                return PaymentError("Incorrect request");
            }

            CheckoutData checkoutConfig;

            if (request.ApiKey != null)
            {
                checkoutConfig = await GetCheckoutData(request.ApiKey, null, null, request.RedirectUrl);
            }
            else if (request.PaymentIntent != null)
            {
                checkoutConfig = await GetCheckoutData(Guid.Parse(request.PaymentIntent), request.RedirectUrl, true);
            }
            else if (request.PaymentRequest != null)
            {
                checkoutConfig = await GetCheckoutData(Guid.Parse(request.PaymentRequest), request.RedirectUrl, false);
            }
            else
            {
                throw new BusinessException(Messages.InvalidCheckoutData);
            }

            if (checkoutConfig == null)
            {
                return RedirectToAction("PaymentLinkNoLongerAvailable");
            }

            var bitRequest = new CaptureBitTransactionRequest
            {
                PaymentInitiationId = request.PaymentInitiationId,
                PaymentTransactionID = request.PaymentTransactionID,
                PaymentIntentID = string.IsNullOrEmpty(request.PaymentIntent) ? default : Guid.Parse(request.PaymentIntent),
                PaymentRequestID = string.IsNullOrEmpty(request.PaymentRequest) ? default : Guid.Parse(request.PaymentRequest),
            };

            var captureResult = await transactionsApiClient.CaptureBitTransaction(bitRequest);

            if (captureResult.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                logger.LogError($"{nameof(BitPaymentCompleted)}.{nameof(transactionsApiClient.CaptureBitTransaction)}: {captureResult.Message}");

                return View(new BitPaymentCompletedViewModel
                {
                    Message = captureResult.Message,
                    ReturnURL = checkoutConfig?.PaymentRequest.PaymentRequestUrl,
                });
            }

            var redirectUrl = request.RedirectUrl ?? checkoutConfig.PaymentRequest.RedirectUrl;

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return View(new BitPaymentCompletedViewModel
                {
                    Message = captureResult.Message,
                });
            }
            else
            {
                if (checkoutConfig.Settings?.LegacyRedirectResponse == true)
                {
                    var paymentTransaction = await transactionsApiClient.GetTransaction(request.PaymentTransactionID);
                    try
                    {
                        redirectUrl = UrlHelper.BuildUrl(
                            redirectUrl,
                            null,
                            LegacyQueryStringConvertor.GetLegacyQueryString(request, paymentTransaction));

                        return Redirect(redirectUrl);
                    }
                    catch { }

                    return View(new BitPaymentCompletedViewModel
                    {
                        Message = captureResult.Message,
                    });
                }
                else
                {
                    return Redirect(UrlHelper.BuildUrl(redirectUrl, null, new { transactionID = request.PaymentTransactionID }));
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Versioning3Ds(ChargeViewModel request)
        {
            try
            {
                bool isPaymentIntent = request.PaymentIntent != null;
                CheckoutData checkoutConfig = await GetCheckoutConfigForCharge(request, isPaymentIntent);

                if (checkoutConfig == null)
                {
                    return Json(new { errorMessage = CommonResources.PaymentLinkNoLongerAvailable });
                }

                var validationResponse = await transactionsApiClient.Versioning3Ds(
                    new Transactions.Api.Models.External.ThreeDS.Versioning3DsRequest
                    {
                        CardNumber = request.CardNumber,
                        TerminalID = checkoutConfig.Settings.TerminalID
                    }
                );

                return Json(validationResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Not possible to process 3DS Versioning request: {ex.Message}");
                return Json(new { errorMessage = CommonResources.ErrorGeneral });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Authenticate3Ds(ChargeViewModel request)
        {
            try
            {
                bool isPaymentIntent = request.PaymentIntent != null;
                CheckoutData checkoutConfig = await GetCheckoutConfigForCharge(request, isPaymentIntent);

                if (checkoutConfig == null)
                {
                    return Json(new { errorMessage = CommonResources.PaymentLinkNoLongerAvailable });
                }

                // TODO: get from real browser
                var browserDetails = new BrowserDetails
                {
                    BrowserAcceptHeader = "text/html,application/xhtml+xml,application/xml;",
                    BrowserLanguage = "en",
                    BrowserColorDepth = "8",
                    BrowserScreenHeight = "1050",
                    BrowserScreenWidth = "1680",
                    BrowserTZ = "1200",
                    BrowserUserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64;)"
                };

                var authResponse = await transactionsApiClient.Authenticate3Ds(
                    new Transactions.Api.Models.External.ThreeDS.Authenticate3DsRequest
                    {
                        Currency = request.Currency,
                        CardNumber = request.CardNumber,
                        TerminalID = checkoutConfig.Settings.TerminalID,
                        ThreeDSServerTransID = request.ThreeDSServerTransID,
                        Amount = request.Amount ?? checkoutConfig.PaymentRequest?.PaymentRequestAmount,
                        BrowserDetails = browserDetails,
                        CardExpiration = CreditCardHelpers.ParseCardExpiration(request.CardExpiration)
                    }
                );

                return Json(authResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Not possible to process 3DS Authenticate request: {ex.Message}");
                return Json(new { errorMessage = CommonResources.ErrorGeneral });
            }
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Notification3Ds(Models.ThreeDS.Notification request)
        {
            string cresDecoded = null;

            cresDecoded = request.Cres?.ConvertFromBase64() ?? request.Error?.ConvertFromBase64();

            if (!string.IsNullOrWhiteSpace(cresDecoded))
            {
                try
                {
                    var cresObj = JsonConvert.DeserializeObject<Transactions.Api.Models.External.ThreeDS.Capture3DsResponse>(cresDecoded);

                    if (string.IsNullOrWhiteSpace(cresObj?.ThreeDSServerTransID))
                    {
                        logger.LogError($"Notification3Ds ThreeDSServerTransID is empty: {cresDecoded}");
                        return View(new Models.ThreeDS.NotificationResult { Success = false });
                    }



                    return View(new Models.ThreeDS.NotificationResult { Success = cresObj.Success, ThreeDSServerTransID = cresObj.ThreeDSServerTransID, Error = cresObj.ErrorDescription });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Notification3Ds is not valid: {ex.Message}");
                    return View(new Models.ThreeDS.NotificationResult { Success = false });
                }
            }
            else
            {
                logger.LogError($"Notification3Ds is empty: {request.Cres}");
                return View(new Models.ThreeDS.NotificationResult { Success = false });
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
        public IActionResult PaymentLinkNoLongerAvailable()
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
        public IActionResult PaymentError(string message, string returnUrl = null)
        {
            return View(nameof(PaymentError), new PaymentErrorViewModel { ErrorMessage = message, ReturnURL = returnUrl });
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
            if (!string.IsNullOrWhiteSpace(culture))
            {
                ChangeLocalizationInternal(culture);
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// Changes current localization to specified one if required.
        /// </summary>
        /// <param name="culture"></param>
        private void ChangeLocalizationInternal(string culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return;
            }

            var allowed = localizationOptions.SupportedCultures.Any(c => c.Name == culture);
            var currentCulture = HttpContext.Features.Get<IRequestCultureFeature>();

            if (!allowed || currentCulture.RequestCulture.Culture.Name == culture)
            {
                return;
            }

            var c = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));

            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, c, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }

        private IActionResult IndexViewResult(CheckoutData checkoutConfig, CardRequest request = null, string queryLang = null)
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
                else if (request.Amount.GetValueOrDefault(0) == 0 && !request.UserAmount)
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
                    model.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new SavedTokenInfo
                    {
                        CreditCardTokenID = d.CreditCardTokenID,
                        Label = $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}",
                        Created = d.Created,
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(model.Origin))
            {
                model.Origin = checkoutConfig.PaymentRequest.Origin ?? Request.GetTypedHeaders().Referer?.Host;
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            model.QueryLang = queryLang;

            return View(nameof(Index), model);
        }

        private IActionResult IndexViewResult(CheckoutData checkoutConfig, ChargeViewModel model)
        {
            mapper.Map(checkoutConfig.PaymentRequest, model);
            mapper.Map(checkoutConfig.Settings, model);
            model.PaymentIntent = checkoutConfig.PaymentIntentID?.ToString();

            if (checkoutConfig.Consumer != null)
            {
                mapper.Map(checkoutConfig.Consumer, model);

                if (checkoutConfig.Consumer.Tokens?.Count() > 0)
                {
                    model.SavedTokens = checkoutConfig.Consumer.Tokens.Select(d => new SavedTokenInfo
                    {
                        CreditCardTokenID = d.CreditCardTokenID,
                        Label = $"{d.CardNumber} {d.CardExpiration} {d.CardVendor}",
                        Created = d.Created,
                    });
                }
            }

            ViewBag.MainLayoutViewModel = checkoutConfig.Settings;

            return View(nameof(Index), model);
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
                    if (webEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        logger.LogWarning(ex, $"Failed to get payment request data. Reason: {webEx.Response}");
                        return null;
                    }
                    else
                    {
                        logger.LogError(ex, $"Failed to get payment request data. Reason: {webEx.Response}");
                    }
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

            //var transactionTypes = new List<TransactionTypeEnum> { TransactionTypeEnum.RegularDeal, TransactionTypeEnum.Immediate };

            //if (checkoutConfig.PaymentRequest != null)
            //{
            //    if (checkoutConfig.Settings.MaxInstallments > 1)
            //    {
            //        transactionTypes.Add(TransactionTypeEnum.Installments);
            //    }

            //    if (checkoutConfig.Settings.MaxCreditInstallments > 1)
            //    {
            //        transactionTypes.Add(TransactionTypeEnum.Credit);
            //    }
            //}
            //else
            //{
            //    transactionTypes.Add(TransactionTypeEnum.Installments);
            //    transactionTypes.Add(TransactionTypeEnum.Credit);
            //}

            //checkoutConfig.Settings.TransactionTypes = transactionTypes;
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
                    if (webEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        logger.LogWarning(ex, $"Failed to get payment request data. Reason: {webEx.Response}");
                        return null;
                    }
                    else
                    {
                        logger.LogError(ex, $"Failed to get payment request data. Reason: {webEx.Response}");
                    }
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

            //var transactionTypes = new List<TransactionTypeEnum> { TransactionTypeEnum.RegularDeal, TransactionTypeEnum.Immediate };

            //if (checkoutConfig.PaymentRequest != null)
            //{
            //    if (checkoutConfig.Settings.MaxInstallments > 1)
            //    {
            //        transactionTypes.Add(TransactionTypeEnum.Installments);
            //    }

            //    if (checkoutConfig.Settings.MaxCreditInstallments > 1)
            //    {
            //        transactionTypes.Add(TransactionTypeEnum.Credit);
            //    }
            //}
            //else
            //{
            //    transactionTypes.Add(TransactionTypeEnum.Installments);
            //    transactionTypes.Add(TransactionTypeEnum.Credit);
            //}

            //checkoutConfig.Settings.TransactionTypes = transactionTypes;
            checkoutConfig.Settings.BlobBaseAddress = apiSettings.BlobBaseAddress;

            return checkoutConfig;
        }
    }
}

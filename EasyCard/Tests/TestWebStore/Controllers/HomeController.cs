﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Helpers;
using TestWebStore.Models;
using Transactions.Business.Services;

namespace TestWebStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IThreeDSIntermediateStorage threeDSIntermediateStorage;
        private readonly WebHookStorage webHookStorage;

        public HomeController(ILogger<HomeController> logger, IThreeDSIntermediateStorage threeDSIntermediateStorage, WebHookStorage webHookStorage)
        {
            this.logger = logger;
            this.threeDSIntermediateStorage = threeDSIntermediateStorage;
            this.webHookStorage = webHookStorage;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("PaymentResult")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PaymentResult(PaymentResultViewModel model)
        {
            return View(model);
        }

        public IActionResult PayWithEasyCard(BasketViewModel model)
        {
            var redirectUrl = BuildRedirectUrl(model);

            return Redirect(redirectUrl);
        }

        private string BuildRedirectUrl(BasketViewModel model)
        {
            var ecUrl = model.CheckoutBaseUrl ?? "https://ecng-checkout.azurewebsites.net";
            var webStoreUrl = "https://ecng-testwebstore.azurewebsites.net";

            var redirectUrl = UrlHelper.BuildUrl(webStoreUrl, "PaymentResult", new { model.InternalOrderID });

            var request = new CardRequest
            {
                Amount = (model.Price * model.Quantity).GetValueOrDefault(),
                ApiKey = model.ApiKey,
                ConsumerID = model.ConsumerID,
                Currency = "ILS",
                Email = model.Email,
                Name = model.Name,
                NationalID = model.NationalID,
                Phone = model.Phone,
                RedirectUrl = redirectUrl,
                Description = $"Goods and services from Test Wen Store: {model.ProductName}",
                IssueInvoice = model.IssueInvoice,
                AllowPinPad = model.AllowPinPad
            };

            var res = UrlHelper.BuildUrl(ecUrl, null, request);

            return res;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Notification3Ds()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var res = await reader.ReadToEndAsync();

                    var authRes = JsonConvert.DeserializeObject<Transactions.Api.Models.External.ThreeDS.Authenticate3DsCallback>(res);

                    if (authRes != null)
                    {
                        await threeDSIntermediateStorage.StoreIntermediateData(new Shared.Integration.Models.ThreeDSIntermediateData(authRes.ThreeDSServerTransID, authRes.AuthenticationValue, authRes.Eci, authRes.Xid)
                        {
                            TransStatus = authRes.TransStatus,
                            Request = res
                        });
                    }
                    else
                    {
                        logger.LogError($"ThreeDS AuthenticateCallback data is empty: {res}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ThreeDS AuthenticateCallback error: {ex.Message}");
            }

            return Ok();
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var res = await reader.ReadToEndAsync();

                    var wh = JsonConvert.DeserializeObject<WebHookData>(res);

                    if (wh != null)
                    {
                        await webHookStorage.StoreData(wh);
                    }
                    else
                    {
                        logger.LogError($"Webhook data is empty: {res}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Webhook error: {ex.Message}");
            }

            return Ok();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Webhooks()
        {
            var res = await webHookStorage.GetData();

            return View(res);
        }
    }
}

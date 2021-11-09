using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Helpers;
using TestWebStore.Models;

namespace TestWebStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
                AllowPinPad = model.AllowPinPad,
                UserAmount = model.UserAmount
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IdentityServer.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace IdentityServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly RequestLocalizationOptions localizationOptions;

        public HomeController(ILogger<HomeController> logger, IOptions<RequestLocalizationOptions> localizationOptions)
        {
            this.logger = logger;
            this.localizationOptions = localizationOptions.Value;
        }

        public IActionResult Index()
        {
            var model = new HomeIndexViewModel
            {
                IsAuthorized = User?.Identity.IsAuthenticated == true,
                UserName = User.Identity?.Name
            };
            return View(model);
        }

        public IActionResult ManageAccount()
        {
            return View();
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
    }
}

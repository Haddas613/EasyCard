using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shared.Api.Configuration;
using Shared.Helpers.Security;

namespace Merchants.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly ApiSettings apiSettings;
        private readonly IdentityServerClientSettings identityConfig;
        private readonly ApplicationInsightsSettings appInsightsSettings;

        public HomeController(IWebHostEnvironment environment, IOptions<ApiSettings> apiSettings, IOptions<IdentityServerClientSettings> identityConfig, IOptions<ApplicationInsightsSettings> appInsightsSettings)
        {
            this.hostingEnvironment = environment;
            this.apiSettings = apiSettings.Value;
            this.identityConfig = identityConfig.Value;
            this.appInsightsSettings = appInsightsSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var config = new UI_CONFIG
            {
                VUE_APP_TRANSACTIONS_API_BASE_ADDRESS = apiSettings.TransactionsApiAddress,
                VUE_APP_MERCHANT_API_BASE_ADDRESS = apiSettings.MerchantsManagementApiAddress,
                VUE_APP_REPORT_API_BASE_ADDRESS = apiSettings.ReportingApiAddress,
                VUE_APP_AUTHORITY = identityConfig.Authority,
                VUE_APP_APPLICATION_INSIGHTS_KEY = appInsightsSettings.InstrumentationKey
            };

            var str = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<script id=\"cfginject\">window.config = ");
            builder.AppendLine(str);
            builder.AppendLine("</script>");

            var path = Path.Combine(hostingEnvironment.WebRootPath, "index.html");
            string data = System.IO.File.ReadAllText(path)
                .Replace("<script id=\"cfginject\"></script>", builder.ToString());
            var response = File(Encoding.UTF8.GetBytes(data), "text/html"); // FileStreamResult
            return response;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shared.Api.Configuration;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantProfileApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IHostEnvironment hostingEnvironment;
        private readonly ApiSettings apiSettings;
        private readonly IdentityServerClientSettings identityConfig;
        private readonly ApplicationInsightsSettings appInsightsSettings;
        private readonly UISettings uISettings;

        public HomeController(
            IHostEnvironment environment,
            IOptions<ApiSettings> apiSettings,
            IOptions<IdentityServerClientSettings> identityConfig,
            IOptions<ApplicationInsightsSettings> appInsightsSettings,
            IOptions<UISettings> uISettings)
        {
            this.hostingEnvironment = environment;
            this.apiSettings = apiSettings.Value;
            this.identityConfig = identityConfig.Value;
            this.appInsightsSettings = appInsightsSettings.Value;
            this.uISettings = uISettings.Value;
        }

        //[HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    var config = new UI_CONFIG
        //    {
        //        VUE_APP_TRANSACTIONS_API_BASE_ADDRESS = apiSettings.TransactionsApiAddress,
        //        VUE_APP_PROFILE_API_BASE_ADDRESS = apiSettings.MerchantProfileURL,
        //        VUE_APP_REPORT_API_BASE_ADDRESS = apiSettings.ReportingApiAddress,
        //        VUE_APP_AUTHORITY = identityConfig.Authority,
        //        VUE_APP_APPLICATION_INSIGHTS_KEY = appInsightsSettings.InstrumentationKey,
        //        VUE_APP_SUPPORT_EMAIL = uISettings.SupportEmail
        //    };

        //    var str = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

        //    str = "window.config=" + str;
        //    var hash = str.ComputeSha256Hash();

        //    StringBuilder builder = new StringBuilder();
        //    builder.Append($"<script id=\"cfginject\" src=\"config\" integrity=\"sha256-{hash}\">");
        //    builder.AppendLine("</script>");

        //    var path = Path.Combine(hostingEnvironment.ContentRootPath, "wwwroot\\index.html");
        //    string data = System.IO.File.ReadAllText(path)
        //        .Replace("<script id=\"cfginject\"></script>", builder.ToString());
        //    var response = File(Encoding.UTF8.GetBytes(data), "text/html"); // FileStreamResult
        //    return response;
        //}

        [HttpGet("config")]
        public async Task<IActionResult> Config()
        {
            var config = new UI_CONFIG
            {
                VUE_APP_TRANSACTIONS_API_BASE_ADDRESS = apiSettings.TransactionsApiAddress,
                VUE_APP_PROFILE_API_BASE_ADDRESS = apiSettings.MerchantProfileURL,
                VUE_APP_REPORT_API_BASE_ADDRESS = apiSettings.ReportingApiAddress,
                VUE_APP_AUTHORITY = identityConfig.Authority,
                VUE_APP_APPLICATION_INSIGHTS_KEY = appInsightsSettings.InstrumentationKey,
                VUE_APP_SUPPORT_EMAIL = uISettings.SupportEmail,
                VUE_APP_BLOB_BASE_ADDRESS = apiSettings.BlobBaseAddress,
            };

            var str = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

            str = "window.config=" + str;

            var response = File(Encoding.UTF8.GetBytes(str), "application/javascript"); // FileStreamResult
            return response;
        }
    }
}

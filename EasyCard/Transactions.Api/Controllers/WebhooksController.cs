using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Models;
using Shared.Api.WebHooks;
using Shared.Business.Security;
using Shared.Helpers.WebHooks;
using System.Threading.Tasks;
using Transactions.Shared;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/webhooks")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class WebhooksController : ApiControllerBase
    {
        private readonly IWebHookService webHookService;
        private readonly IMapper mapper;

        private readonly ILogger logger;
        private readonly ApplicationSettings appSettings;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        private readonly ITerminalsService terminalsService;

        public WebhooksController(
            IWebHookService webHookService, IMapper mapper,
            ITerminalsService terminalsService,
            ILogger<TransactionsApiController> logger,
            IOptions<ApplicationSettings> appSettings,
            IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.webHookService = webHookService;

            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get transactions which are not transmitted yet
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesResponse<ExecutedWebhookSummary>>> GeExecutedWebhooks([FromQuery] WebhooksFilter filter)
        {
            var query = await webHookService.GetWebHooks(filter);

            var res = new SummariesResponse<ExecutedWebhookSummary> { Data = query };

            return res;
        }
    }
}

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
using CheckoutPortal.Models.Ecwid;

namespace CheckoutPortal.Controllers
{
    [AllowAnonymous]
    [Route("ecwid")]
    public class EcwidController : Controller
    {
        private readonly ILogger<EcwidController> logger;
        private readonly ITransactionsApiClient transactionsApiClient;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IMapper mapper;
        private readonly IHubContext<Hubs.TransactionsHub, Transactions.Shared.Hubs.ITransactionsHub> transactionsHubContext;
        private readonly ApiSettings apiSettings;

        public EcwidController(
            ILogger<EcwidController> logger,
            ITransactionsApiClient transactionsApiClient,
            ICryptoServiceCompact cryptoServiceCompact,
            IMapper mapper,
            IHubContext<Hubs.TransactionsHub, Transactions.Shared.Hubs.ITransactionsHub> transactionsHubContext,
            IOptions<ApiSettings> apiSettings)
        {
            this.logger = logger;
            this.transactionsApiClient = transactionsApiClient;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.mapper = mapper;
            this.transactionsHubContext = transactionsHubContext;
            this.apiSettings = apiSettings.Value;
        }

        [HttpPost]
        public IActionResult Index(EcwidRequestPayload request)
        {
            string decryptedJson = null;
            try
            {

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(EcwidController)} index error: Could not decrypt given request: {request.Data}");
                throw new ApplicationException("Invalid request");
            }

            return View();
        }

        [Route("settings")]
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }
    }
}

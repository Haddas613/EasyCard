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
using Ecwid.Configuration;
using Ecwid;
using Ecwid.Models;

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
        private readonly EcwidGlobalSettings ecwidSettings;

        private readonly EcwidConvertor ecwidConvertor;

        public EcwidController(
            ILogger<EcwidController> logger,
            ITransactionsApiClient transactionsApiClient,
            ICryptoServiceCompact cryptoServiceCompact,
            IMapper mapper,
            IHubContext<Hubs.TransactionsHub, Transactions.Shared.Hubs.ITransactionsHub> transactionsHubContext,
            IOptions<ApiSettings> apiSettings,
            IOptions<EcwidGlobalSettings> ecwidSettings)
        {
            this.logger = logger;
            this.transactionsApiClient = transactionsApiClient;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.mapper = mapper;
            this.transactionsHubContext = transactionsHubContext;
            this.apiSettings = apiSettings.Value;
            this.ecwidSettings = ecwidSettings.Value;

            this.ecwidConvertor = new EcwidConvertor(this.ecwidSettings);
        }

        [HttpPost]
        public IActionResult Index(EcwidRequestPayload request)
        {
            EcwidOrder ecwidOrder = null;
            try
            {
                ecwidOrder = ecwidConvertor.DecryptEcwidOrder(request.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(EcwidController)} index error: Could not decrypt given request: {request.Data}");
                throw new ApplicationException("Invalid request");
            }

            //TODO: get consumer from ecwid data by external reference. If not present create one and use it
            //when creating set Origin to "Ecwid"

            //TODO: use api key when initializing Transactions.Api.Client

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

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.External;
using Transactions.Business.Services;
using Upay;

namespace Transactions.Api.Controllers.External
{

    [AllowAnonymous]
    public class UPayApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly UpayGlobalSettings configuration;

        public UPayApiController(
             ITransactionsService transactionsService,
             IMapper mapper,
             ILogger<TransactionsApiController> logger,
             IOptions<UpayGlobalSettings> configuration)
        {
            this.transactionsService = transactionsService;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<string>> ValidateDeal(Guid? token)
        {
            if (Request.Headers.ContainsKey("API-key") && !string.IsNullOrEmpty(Request.Headers["API-key"]))
            {
                string key = Request.Headers["API-key"];
                if (!key.Equals(configuration.ApiKey))
                {
                    return Unauthorized($"API-Key value is not authorized");
                }
            }
            else
            {
                return Unauthorized($"Request is not authorized. There is no API-Key.");
            }

            var transaction = mapper.Map<UpayValidateDealResult>(EnsureExists(
                await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == transactionID)));
            return transaction.ToString();
        }
    }
}

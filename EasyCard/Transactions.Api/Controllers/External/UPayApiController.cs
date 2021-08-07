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
    [Authorize(AuthenticationSchemes = Extensions.Auth.ApiKeyAuthenticationScheme, Policy = Policy.UPayAPI)]
    [Route("external/upay")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        [Route("v1/validate-deal/{token:guid}")]
        public async Task<ActionResult<UpayValidateDealResult>> ValidateDeal([FromRoute] Guid token)
        {
            var transaction = mapper.Map<UpayValidateDealResult>(EnsureExists(
                await transactionsService.GetTransactions().FirstOrDefaultAsync(m => m.PaymentTransactionID == token)));
            return transaction;
        }
    }
}

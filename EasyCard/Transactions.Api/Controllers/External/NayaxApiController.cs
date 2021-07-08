using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nayax.Configuration;
using Shared.Api;
using Shared.Api.Models;
using Shared.Helpers.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.External;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Services;
using Transactions.Shared.Enums;
using SharedApi = Shared.Api;

namespace Transactions.Api.Controllers.External
{

    [Authorize(AuthenticationSchemes = Extensions.Auth.ApiKeyAuthenticationScheme, Policy = Policy.NayaxAPI)]
    [Route("api/external/nayax")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class NayaxApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly NayaxGlobalSettings configuration;

        public NayaxApiController(
             ITransactionsService transactionsService,
             IMapper mapper,
             ILogger<TransactionsApiController> logger,
             IOptions<NayaxGlobalSettings> configuration)
        {
            this.transactionsService = transactionsService;
            this.logger = logger;
            this.configuration = configuration.Value;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("v1/tranRecord")]
        public async Task<ActionResult<NayaxUpdateTranRecordResponse>> UpdateTranRecord([FromBody] NayaxUpdateTranRecordRequest model)
        {
            try
            {
                var transaction =
                EnsureExists(
                await transactionsService.GetTransactionsForUpdate().FirstOrDefaultAsync(m => m.PinPadTransactionID == model.Vuid));

                transaction.ShvaTransactionDetails.TranRecord = model.TranRecord;
                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission, transactionOperationCode: TransactionOperationCodesEnum.ProcessorPreTransmissionCommited);
                return new NayaxUpdateTranRecordResponse
                {
                    Status = "0"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to update TransactionRecord for PAX deal. Vuid: {model.Vuid} Uid: {model.Uid} ");

                return new NayaxUpdateTranRecordResponse
                {
                    StatusCode = 14,
                    ErrorMsg = string.Format("Failed to update TransactionRecord for PAX deal. Vuid: {0} Uid: {1} ", model.Vuid, model.Uid),
                    Status = "error"
                };
            }
        }
    }
}

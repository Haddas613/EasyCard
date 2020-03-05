using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Api.Models;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Services;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transactions")]
    [ApiController]
    public class TransactionsApiController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;

        public TransactionsApiController(ITransactionsService transactionsService)
        {
            this.transactionsService = transactionsService;
        }

        [HttpGet]
        [Route("{transactionID}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction([FromRoute] long transactionID)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetTransactions([FromQuery] TransactionsFilter filter)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateTransaction([FromBody] TransactionRequest model)
        {
            throw new NotImplementedException();
        }
    }
}
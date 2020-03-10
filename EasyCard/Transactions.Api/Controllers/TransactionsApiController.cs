using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Models;
using Shared.Integration.Models;
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
        private readonly SecretClient secretClient;
        private readonly IMapper mapper;

        public TransactionsApiController(ITransactionsService transactionsService, SecretClient secretClient, IMapper mapper)
        {
            this.transactionsService = transactionsService;
            this.secretClient = secretClient;
            this.mapper = mapper;
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
        [Route("token")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateToken([FromBody] TokenRequest model)
        {
            try
            {
                // todo: encrypt auth data
                var key = Guid.NewGuid().ToString();
                var data = mapper.Map<CreditCardToken>(model);

                // todo: implement
                //data.TerminalID = ...;
                //data.UserID = ...;

                var resp = await secretClient.SetSecretAsync(new KeyVaultSecret(key, JsonConvert.SerializeObject(data)));

                return CreatedAtAction(nameof(CreateToken), resp.Value.Name);

            }catch(Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateTransaction([FromBody] TransactionRequest model)
        {
            throw new NotImplementedException();
        }
    }
}
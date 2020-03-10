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
using Shared.Helpers.KeyValueStorage;
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
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardToken> keyValueStorage;

        public TransactionsApiController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardToken> keyValueStorage, IMapper mapper)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
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

                await keyValueStorage.Save(key, JsonConvert.SerializeObject(data));

                return CreatedAtAction(nameof(CreateToken), new { token = key});

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
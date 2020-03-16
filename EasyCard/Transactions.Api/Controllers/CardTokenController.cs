using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers.KeyValueStorage;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;

namespace Transactions.Api.Controllers
{
    [Route("api/cardtokens")]
    [ApiController]
    public class CardTokenController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardToken> keyValueStorage;

        public CardTokenController(ITransactionsService transactionsService, ICreditCardTokenService creditCardTokenService,
            IKeyValueStorage<CreditCardToken> keyValueStorage, IMapper mapper)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("token")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateToken([FromBody] TokenRequest model)
        {
            // todo: encrypt auth data
            var key = Guid.NewGuid().ToString();
            var storageData = mapper.Map<CreditCardToken>(model);

            // todo: implement
            storageData.TerminalID = 1;
            storageData.MerchantID = 1;

            await keyValueStorage.Save(key, JsonConvert.SerializeObject(storageData));

            var dbData = mapper.Map<CreditCardTokenDetails>(storageData);
            dbData.CardOwnerNationalID = "test";
            dbData.CardVendor = "test";
            dbData.PublicKey = key;
            await creditCardTokenService.CreateEntity(dbData);

            return CreatedAtAction(nameof(CreateToken), new OperationResponse("ok", StatusEnum.Success, key));
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<CreditCardTokenSummary>>> GetTokens([FromQuery] CreditCardTokenFilter filter)
        {
            var query = creditCardTokenService.GetTokens();

            var response = new SummariesResponse<CreditCardTokenSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<CreditCardTokenSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }

        [HttpDelete]
        [Route("{key}")]
        public async Task<ActionResult<OperationResponse>> DeleteToken(string key)
        {
            var token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(t => t.PublicKey == key));

            await keyValueStorage.Delete(key);

            token.Active = false;
            await creditCardTokenService.UpdateEntity(token);

            return Ok(new OperationResponse("ok", StatusEnum.Success, key));
        }
    }
}
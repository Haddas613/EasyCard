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
using Shared.Api.Models.Enums;
using Shared.Helpers.KeyValueStorage;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Business.Entities;
using Transactions.Business.Services;

namespace Transactions.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardTokenController : ControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardToken> keyValueStorage;

        public CardTokenController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardToken> keyValueStorage, IMapper mapper)
        {
            this.transactionsService = transactionsService;
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
            await transactionsService.CreateToken(dbData);

            return CreatedAtAction(nameof(CreateToken), new OperationResponse("ok", StatusEnum.Success, key));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
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
using Transactions.Shared;

namespace Transactions.Api.Controllers
{
    [Route("api/cardtokens")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class CardTokenController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;

        private readonly ITerminalsService terminalsService;

        public CardTokenController(ITransactionsService transactionsService, ICreditCardTokenService creditCardTokenService,
            IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper, ITerminalsService terminalsService)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
        }

        [HttpPost]
        [Route("token")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OperationResponse))]
        public async Task<ActionResult<OperationResponse>> CreateToken([FromBody] TokenRequest model)
        {
            var terminal = SecureExists(await terminalsService.GetTerminals().Where(d => d.TerminalID == model.TerminalID).FirstOrDefaultAsync());

            var storageData = mapper.Map<CreditCardTokenKeyVault>(model);
            var dbData = mapper.Map<CreditCardTokenDetails>(model);

            storageData.TerminalID = dbData.TerminalID = terminal.TerminalID;
            storageData.MerchantID = dbData.MerchantID = terminal.MerchantID;

            await keyValueStorage.Save(dbData.CreditCardTokenID.ToString(), JsonConvert.SerializeObject(storageData));

            await creditCardTokenService.CreateEntity(dbData);

            return CreatedAtAction(nameof(CreateToken), new OperationResponse(Messages.TokenCreated, StatusEnum.Success, dbData.CreditCardTokenID.ToString()));
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
            var guid = new Guid(key);
            var token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(t => t.CreditCardTokenID == guid));

            var terminal = SecureExists(await terminalsService.GetTerminals().Where(d => d.TerminalID == token.TerminalID).FirstOrDefaultAsync());

            await keyValueStorage.Delete(key);

            token.Active = false;
            await creditCardTokenService.UpdateEntity(token);

            return Ok(new OperationResponse(Messages.TokenDeleted, StatusEnum.Success, key));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Data;
using IdentityServer.Data.Entities;
using IdentityServer.Helpers;
using IdentityServer.Messages;
using IdentityServer.Models;
using IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServerClient;
using Merchants.Api.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api;
using Shared.Api.Models;
using Shared.Helpers.Email;
using Shared.Helpers.Security;

namespace IdentityServer.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/terminalapikeys")]
    [Authorize(Policy = Policy.ManagementApi, AuthenticationSchemes = "token")]
    public class TerminalApiKeyController : ApiControllerBase
    {
        private readonly ILogger logger;
        private readonly ICryptoService cryptoService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICryptoServiceCompact cryptoServiceCompact;
        private readonly IMerchantsApiClient merchantsApiClient;

        public TerminalApiKeyController(
            ILogger<UserManagementApiController> logger,
            ICryptoService cryptoService,
            UserManager<ApplicationUser> userManager,
            ICryptoServiceCompact cryptoServiceCompact,
            IMerchantsApiClient merchantsApiClient)
        {
            this.logger = logger;
            this.cryptoService = cryptoService;
            this.userManager = userManager;
            this.cryptoServiceCompact = cryptoServiceCompact;
            this.merchantsApiClient = merchantsApiClient;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiKeyOperationResponse>> Create([FromBody]CreateTerminalApiKeyRequest model)
        {
            var user = await userManager.FindByNameAsync($"terminal_{model.TerminalID}");

            if (user != null)
            {
                await userManager.DeleteAsync(user); // TODO: check success
            }

            user = new ApplicationUser { UserName = $"terminal_{model.TerminalID}" };
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                logger.LogError($"Failed to create terminal Api Key, TerminalID: {model.TerminalID}");
                return BadRequest("Failed to create terminal Api Key");
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            await userManager.AddClaim(allClaims, user, Claims.TerminalIDClaim, model.TerminalID.ToString());
            await userManager.AddClaim(allClaims, user, Claims.MerchantIDClaim, model.MerchantID.ToString());

            if (model.WoocommerceEnabled)
            {
                await userManager.AddClaim(allClaims, user, Claims.WoocommerceEnabled, "true");
            }

            if (model.EcwidEnabled)
            {
                await userManager.AddClaim(allClaims, user, Claims.EcwidEnabled, "true");
            }

            await merchantsApiClient.AuditResetApiKey(model.TerminalID, model.MerchantID);

            return Ok(GetApiKeyOperationResponse(user.Id, true, true));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiKeyOperationResponse>> Update([FromBody] CreateTerminalApiKeyRequest model)
        {
            var user = await userManager.FindByNameAsync($"terminal_{model.TerminalID}");

            if (user == null)
            {
                logger.LogError($"Failed to update terminal Api Key, TerminalID: {model.TerminalID}");
                return NotFound("Failed to update terminal Api Key");
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            await userManager.AddClaim(allClaims, user, Claims.WoocommerceEnabled, model.WoocommerceEnabled.ToString().ToLower());

            await userManager.AddClaim(allClaims, user, Claims.EcwidEnabled, model.EcwidEnabled.ToString().ToLower());

            await merchantsApiClient.AuditResetApiKey(model.TerminalID, model.MerchantID);

            return Ok(GetApiKeyOperationResponse(user.Id, true, true));
        }

        // admin only
        [HttpDelete]
        [Route("{terminalID}")]
        public async Task<ActionResult<ApiKeyOperationResponse>> Delete([FromRoute] Guid terminalID)
        {
            var user = await userManager.FindByNameAsync($"terminal_{terminalID}");

            if (user == null)
            {
                logger.LogError($"Terminal Api Key does not exist, TerminalID: {terminalID}");
                return NotFound("Terminal Api Key does not exist");
            }

            await userManager.DeleteAsync(user); // TODO: check success

            return Ok(new ApiKeyOperationResponse());
        }

        // admin only
        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<ApiKeyOperationResponse>> Get([FromRoute] Guid terminalID)
        {
            var user = await userManager.FindByNameAsync($"terminal_{terminalID}");

            if (user == null)
            {
                logger.LogError($"Terminal Api Key does not exist, TerminalID: {terminalID}");
                return NotFound("Terminal Api Key does not exist");
            }

            return Ok(GetApiKeyOperationResponse(user.Id, true, true));
        }

        private ApiKeyOperationResponse GetApiKeyOperationResponse(string userId, bool woocommerce, bool ecwid)
        {
            var key = cryptoServiceCompact.EncryptCompact(userId);

            var res = new ApiKeyOperationResponse { ApiKey = key };

            if (woocommerce)
            {
                res.WoocommerceApiKey = cryptoServiceCompact.EncryptCompact($"{userId}-wc");
            }

            return res;
        }
    }
}
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
        private readonly ITerminalApiKeyService terminalApiKeyService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICryptoServiceCompact cryptoServiceCompact;

        public TerminalApiKeyController(
            ILogger<UserManagementApiController> logger,
            ICryptoService cryptoService,
            ITerminalApiKeyService terminalApiKeyService,
            UserManager<ApplicationUser> userManager,
            ICryptoServiceCompact cryptoServiceCompact)
        {
            this.logger = logger;
            this.cryptoService = cryptoService;
            this.terminalApiKeyService = terminalApiKeyService;
            this.userManager = userManager;
            this.cryptoServiceCompact = cryptoServiceCompact;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> Create([FromBody]CreateTerminalApiKeyRequest model)
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
                return BadRequest("Failed to create terminal Api Key"); // TODO: log details
            }

            var allClaims = await userManager.GetClaimsAsync(user);

            await userManager.AddClaim(allClaims, user, Claims.TerminalIDClaim, model.TerminalID.ToString());
            await userManager.AddClaim(allClaims, user, Claims.MerchantIDClaim, model.MerchantID.ToString());

            return Ok(new OperationResponse(ApiMessages.TerminalApiKeyCreated, Shared.Api.Models.Enums.StatusEnum.Success, cryptoServiceCompact.EncryptCompact(user.Id)));
        }

        // admin only
        [HttpDelete]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> Delete([FromRoute] Guid terminalID)
        {
            var user = await userManager.FindByNameAsync($"terminal_{terminalID}");

            if (user == null)
            {
                return NotFound("Terminal Api Key does not exist"); // TODO: log details
            }

            await userManager.DeleteAsync(user); // TODO: check success

            return Ok(new OperationResponse(ApiMessages.TerminalApiKeyRemoved, Shared.Api.Models.Enums.StatusEnum.Success));
        }
    }
}
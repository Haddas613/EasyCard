using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Data;
using IdentityServer.Data.Entities;
using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServer.Models.TerminalApiKey;
using IdentityServer.Services;
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

        public TerminalApiKeyController(
            ILogger<UserManagementApiController> logger,
            ICryptoService cryptoService,
            ITerminalApiKeyService terminalApiKeyService,
            UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.cryptoService = cryptoService;
            this.terminalApiKeyService = terminalApiKeyService;
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> Get([FromRoute] long terminalID)
        {
            //TODO: check user access

            var key = EnsureExists(await terminalApiKeyService.GetAuthKeys().Where(k => k.TerminalID == terminalID).Select(k => k.AuthKey).FirstOrDefaultAsync(), "Key");

            return Ok(new OperationResponse("ok", Shared.Api.Models.Enums.StatusEnum.Success, key));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> Create([FromBody]CreateTerminalApiKeyRequest model)
        {
            var user = new ApplicationUser { UserName = $"terminal_{model.TerminalID:0000000000}" };
            var result = await userManager.CreateAsync(user);

            var newApiKey = new TerminalApiAuthKey
            {
                AuthKey = cryptoService.Encrypt(user.Id), //TODO: encrypt actual data
                TerminalID = model.TerminalID,
                Created = DateTime.UtcNow
            };

            await terminalApiKeyService.CreateEntity(newApiKey);

            return Ok(new OperationResponse("ok", Shared.Api.Models.Enums.StatusEnum.Success, newApiKey.AuthKey));
        }

        [HttpPut]
        [Route("{terminalID}/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OperationResponse>> Reset([FromRoute] long terminalID)
        {
            //TODO: check user access

            var key = EnsureExists(await terminalApiKeyService.GetAuthKeys().Where(k => k.TerminalID == terminalID).FirstOrDefaultAsync(), "Key");

            //TODO: encrypt actual data
            key.AuthKey = cryptoService.Encrypt(Guid.NewGuid().ToString());

            await terminalApiKeyService.UpdateEntity(key);

            return Ok(new OperationResponse("ok", Shared.Api.Models.Enums.StatusEnum.Success, key.AuthKey));
        }

        // admin only
        [HttpDelete]
        [Route("{terminalID}")]
        public async Task<ActionResult<OperationResponse>> Delete([FromRoute] long terminalID)
        {
            //TODO: check user access
            var key = EnsureExists(await terminalApiKeyService.GetAuthKeys().Where(k => k.TerminalID == terminalID).FirstOrDefaultAsync(), "Key");

            await terminalApiKeyService.Delete(key.TerminalApiAuthKeyID);

            return Ok(new OperationResponse("ok", Shared.Api.Models.Enums.StatusEnum.Success));
        }
    }
}
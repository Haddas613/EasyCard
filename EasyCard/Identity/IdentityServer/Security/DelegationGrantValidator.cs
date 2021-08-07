using IdentityServer.Models;
using IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public class DelegationGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator validator;
        private readonly ICryptoService cryptoService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICryptoServiceCompact cryptoServiceCompact;

        public DelegationGrantValidator(
            ITokenValidator validator,
            ICryptoService cryptoService, UserManager<ApplicationUser> userManager, ICryptoServiceCompact cryptoServiceCompact)
        {
            this.validator = validator;
            this.cryptoService = cryptoService;
            this.userManager = userManager;
            this.cryptoServiceCompact = cryptoServiceCompact;
        }

        public string GrantType => "terminal_rest_api";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get("authorizationKey");

            if (string.IsNullOrEmpty(userToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            //var result = await validator.ValidateAccessTokenAsync(userToken);
            //if (result.IsError)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            ////// get user's identity
            //var sub = result.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            //var apiKey = terminalApiKeyService.GetAuthKeys().FirstOrDefault(d => d.AuthKey == userToken);

            //if (apiKey == null)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            try
            {
                var userId = cryptoServiceCompact.DecryptCompact(userToken);

                //var claims = new List<Claim>() { new Claim("extension_TerminalID", apiKey.TerminalID.ToString()) };

                context.Result = new GrantValidationResult(userId, GrantType);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message); // TODO: log exception
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }
        }
    }
}

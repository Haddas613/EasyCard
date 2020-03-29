using IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public class DelegationGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator validator;
        private readonly ITerminalApiKeyService terminalApiKeyService;
        private readonly ICryptoService cryptoService;

        public DelegationGrantValidator(ITokenValidator validator, ITerminalApiKeyService terminalApiKeyService, ICryptoService cryptoService)
        {
            this.validator = validator;
            this.terminalApiKeyService = terminalApiKeyService;
            this.cryptoService = cryptoService;
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

            var apiKey = terminalApiKeyService.GetAuthKeys().FirstOrDefault(d => d.AuthKey == userToken);

            if (apiKey == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            try
            {
                var userId = cryptoService.Decrypt(apiKey.AuthKey);

                //var claims = new List<Claim>() { new Claim("extension_TerminalID", apiKey.TerminalID.ToString()) };

                context.Result = new GrantValidationResult(userId, GrantType);
                return;
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }
        }
    }
}

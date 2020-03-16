using IdentityServer4.Models;
using IdentityServer4.Validation;
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

        public DelegationGrantValidator(ITokenValidator validator)
        {
            this.validator = validator;
        }

        public string GrantType => "my_crap_grant";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get("token");

            //if (string.IsNullOrEmpty(userToken))
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            //var result = await validator.ValidateAccessTokenAsync(userToken);
            //if (result.IsError)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            ////// get user's identity
            //var sub = result.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            context.Result = new GrantValidationResult("2a0472fd-b1b2-4c1b-964d-b1cc8e5f44f7", GrantType);
            return;
        }
    }
}

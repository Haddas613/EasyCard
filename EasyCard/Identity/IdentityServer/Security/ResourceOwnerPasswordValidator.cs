using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Security
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        //private IUserRepository _userRepository;

        public ResourceOwnerPasswordValidator(/*IUserRepository userRepository*/)
        {
            //this._userRepository = userRepository;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //var isAuthenticated = _userRepository.ValidatePassword(context.UserName, context.Password);

            context.Result = new GrantValidationResult("2a0472fd-b1b2-4c1b-964d-b1cc8e5f44f7", "password", null, "local", null);
            return Task.FromResult(context.Result);
        }
    }
}

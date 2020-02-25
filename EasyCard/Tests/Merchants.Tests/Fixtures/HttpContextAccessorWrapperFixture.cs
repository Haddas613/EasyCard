using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Merchants.Tests.Fixtures
{
    // TODO: use constructor to add roles, merchantID etc.
    public class HttpContextAccessorWrapperFixture : IHttpContextAccessorWrapper
    {
        public ClaimsPrincipal GetUser()
        {
            return new ClaimsPrincipal(); // TODO: add claims
        }
    }
}

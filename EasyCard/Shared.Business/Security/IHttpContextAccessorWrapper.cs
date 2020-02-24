using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Business.Security
{
    public interface IHttpContextAccessorWrapper
    {
        ClaimsPrincipal GetUser();
    }
}

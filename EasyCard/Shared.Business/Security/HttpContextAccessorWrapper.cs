﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Business.Security
{
    public class HttpContextAccessorWrapper : IHttpContextAccessorWrapper
    {
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextAccessorWrapper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.user = httpContextAccessor?.HttpContext?.User;
        }

        public ClaimsPrincipal GetUser()
        {
            return this.user;
        }
    }
}

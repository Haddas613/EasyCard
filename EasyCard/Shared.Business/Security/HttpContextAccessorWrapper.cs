using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Business.Security
{
    public class HttpContextAccessorWrapper : IHttpContextAccessorWrapper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContext HttpContext { get => httpContextAccessor?.HttpContext; }

        public string TraceIdentifier { get => httpContextAccessor?.HttpContext?.TraceIdentifier; }

        public HttpContextAccessorWrapper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal GetUser()
        {
            return httpContextAccessor?.HttpContext?.User;
        }

        public string GetIP()
        {
            return httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.ToString();
        }

        public string GetCorrelationId()
        {
            return HttpContext?.TraceIdentifier;
        }
    }
}

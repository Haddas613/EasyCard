using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Shared.Business.Messages;
using System.Security;

namespace Shared.Api
{
    public class ApiControllerBase : ControllerBase
    {
        [NonAction]
        protected T EnsureExists<T>(T src, string entityName = null)
        {
            if (src == null)
            {
                throw new Business.Exceptions.EntityNotFoundException(ApiMessages.EntityNotFound, entityName ?? typeof(T).Name, null);
            }

            return src;
        }

        [NonAction]
        protected T SecureExists<T>(T src)
        {
            if (src == null)
            {
                throw new SecurityException(ApiMessages.YouHaveNoAccess);
            }

            return src;
        }

        [NonAction]
        protected T ValidateExists<T>(T src, string message)
        {
            if (src == null)
            {
                throw new Business.Exceptions.BusinessException(message);
            }

            return src;
        }

        [NonAction]
        protected string GetCorrelationID()
        {
            return HttpContext?.TraceIdentifier;
        }

        [NonAction]
        protected string GetIP()
        {
            return HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}

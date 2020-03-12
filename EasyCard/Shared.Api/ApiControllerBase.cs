using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Shared.Business.Messages;

namespace Shared.Api
{
    public class ApiControllerBase : ControllerBase
    {
        public T EnsureExists<T>(T src, string entityName = null)
        {
            if (src == null)
            {
                throw new Business.Exceptions.EntityNotFoundException(ApiMessages.EntityNotFound, entityName ?? typeof(T).Name, null);
            }

            return src;
        }

        public string GetCorrelationID()
        {
            return this.HttpContext.TraceIdentifier;
        }
    }
}

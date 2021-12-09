using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Shared.Business.Messages;
using System.Security;
using Shared.Business;
using Shared.Helpers;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace Shared.Api
{
    public class ApiControllerBase : ControllerBase
    {
        private CultureInfo requestCulture;

        protected CultureInfo CurrentCulture
        {
            get
            {
                if (requestCulture == null)
                {
                    requestCulture = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture?.Culture ?? CultureInfo.InvariantCulture;
                }

                return requestCulture;
            }
        }

        [NonAction]
        protected T EnsureExists<T>(T src, string entityName = null, string entityReference = null)
        {
            if (src == null)
            {
                throw new EntityNotFoundException(ApiMessages.EntityNotFound, entityName ?? typeof(T).Name, entityReference);
            }

            return src;
        }

        [NonAction]
        protected T EnsureConcurrency<T, TModel>(T src, TModel model)
            where T : IConcurrencyCheck
            where TModel : IConcurrencyCheck
        {
            if (Convert.ToBase64String(src.UpdateTimestamp) != Convert.ToBase64String(model.UpdateTimestamp))
            {
                throw new EntityConflictException(ApiMessages.RecordChangedSinceLastRead, typeof(T).Name);
            }

            return src;
        }

        [NonAction]
        protected T EnsureConcurrency<T>(T src, byte[] modelTimestamp)
            where T : IConcurrencyCheck
        {
            if (Convert.ToBase64String(src.UpdateTimestamp) != Convert.ToBase64String(modelTimestamp))
            {
                throw new EntityConflictException(ApiMessages.RecordChangedSinceLastRead, typeof(T).Name);
            }

            return src;
        }

        [NonAction]
        protected T ValidateExists<T>(T src, string message)
        {
            if (src == null)
            {
                throw new BusinessException(message);
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

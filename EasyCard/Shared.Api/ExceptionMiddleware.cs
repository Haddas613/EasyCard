using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Api.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            this.logger = logger;

            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            int responseStatusCode = 500;
            string result = string.Empty;
            bool logAsWarning = false;

            if (ex is EntityNotFoundException enfeEx)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = enfeEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, EntityType = enfeEx.EntityType, EntityReference = enfeEx.EntityReference });
                responseStatusCode = 404;
                logAsWarning = true;
            }
            else if (ex is EntityConflictException econEx)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = econEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, EntityType = econEx.EntityType });
                responseStatusCode = 409;
            }
            else if (ex is SecurityException securityEx)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = securityEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId });
                responseStatusCode = 403;
            }
            else if (ex is BusinessException businessEx)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = businessEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, Errors = businessEx.Errors });
                responseStatusCode = 400;
            }
            else if (ex is NotImplementedException)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = "Not Implemented", Status = StatusEnum.Error, CorrelationId = correlationId });
            }
            else if (ex is WebApiServerErrorException)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = "Error when calling underlying service", Status = StatusEnum.Error, CorrelationId = correlationId });
            }
            else if (ex is IntegrationException integrationException)
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = integrationException.Message, Status = StatusEnum.Error, CorrelationId = correlationId, AdditionalData = JObject.FromObject(new { IntegrationMessageId = integrationException.MessageId }) });
                responseStatusCode = 400;
            }
            else
            {
                result = JsonConvert.SerializeObject(new OperationResponse { Message = "System error occurred. Please contact support", Status = StatusEnum.Error, CorrelationId = correlationId });
            }

            if (ex != null)
            {
                if (logAsWarning)
                {
                    logger.LogWarning(ex, ApiErrorLogFormatter.ExceptionFormatWithDetails(ex, correlationId));
                }
                else
                {
                    logger.LogError(ex, ApiErrorLogFormatter.ExceptionFormatWithDetails(ex, correlationId));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = responseStatusCode;
            await context.Response.WriteAsync(result);
        }
    }
}

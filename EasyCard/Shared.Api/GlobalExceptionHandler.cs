using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Api.Logging;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Exceptions;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Shared.Api
{
    public class GlobalExceptionHandler
    {
        public static void HandleException(IApplicationBuilder applicationBuilder)
        {
            var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILogger<GlobalExceptionHandler>>();

            applicationBuilder.Run(async context =>
            {
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                var ex = exceptionHandlerPathFeature?.Error;

                var correlationId = context.TraceIdentifier;
                int responseStatusCode = 500;
                string result = string.Empty;

                if (ex != null)
                {
                    logger.LogError(ApiErrorLogFormatter.ExceptionFormatWithDetails(ex, correlationId));
                }

                if (ex is EntityNotFoundException enfeEx)
                {
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = enfeEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, EntityType = enfeEx.EntityType });
                    responseStatusCode = 404;
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
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = businessEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId });
                    responseStatusCode = 400;
                }
                else
                {
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = "System error occurred. Please contact support", Status = StatusEnum.Error, CorrelationId = correlationId });
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = responseStatusCode;
                await context.Response.WriteAsync(result);
            });
        }
    }
}

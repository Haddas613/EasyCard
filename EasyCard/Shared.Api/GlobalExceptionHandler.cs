using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Exceptions;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Shared.Api
{
    public static class GlobalExceptionHandler
    {
        public static void HandleException(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.Run(async context =>
            {
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                var ex = exceptionHandlerPathFeature?.Error;

                var correlationId = context.TraceIdentifier;
                int responseStatusCode = 500;
                string result = string.Empty;

                // TODO: log errors
                //_logger.LogError(ex, $"Exception {correlationId}: {ex.Message}");

                if (ex is EntityNotFoundException enfeEx)
                {
                    //TODO: log details
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = enfeEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, EntityType = enfeEx.EntityType });
                    responseStatusCode = 404;
                }
                else if (ex is EntityConflictException econEx)
                {
                    //TODO: log details
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = econEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId, EntityType = econEx.EntityType });
                    responseStatusCode = 409;
                }
                else if (ex is SecurityException securityEx)
                {
                    //TODO: log details
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = securityEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId });
                    responseStatusCode = 403;
                }
                else if (ex is BusinessException businessEx)
                {
                    //TODO: log details
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

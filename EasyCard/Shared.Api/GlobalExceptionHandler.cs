using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Exceptions;
using System;
using System.Collections.Generic;
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

                if(ex is EntityNotFoundException)
                {
                    //TODO: log details
                    var enfeEx = (EntityNotFoundException)ex;
                    result = JsonConvert.SerializeObject(new OperationResponse { Message = enfeEx.Message, Status = StatusEnum.Error, CorrelationId = correlationId });

                    responseStatusCode = 404;
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

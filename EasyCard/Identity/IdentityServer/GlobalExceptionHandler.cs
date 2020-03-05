using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer
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

                var logger = applicationBuilder.ApplicationServices.GetService<ILogger>();
                logger.LogError(ex, $"Exception {correlationId}: {ex.Message}");

                result = JsonConvert.SerializeObject(new { Message = "System error occurred. Please contact support", Status = "error", CorrelationId = correlationId });

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = responseStatusCode;
                await context.Response.WriteAsync(result);
            });
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IRequestLogStorageService requestLogStorageService;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, IRequestLogStorageService requestLogStorageService)
        {
            this.next = next;

            this.logger = logger;

            this.requestLogStorageService = requestLogStorageService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Get || context.Request.Method == HttpMethods.Head || context.Request.Method == HttpMethods.Options)
            {
                await next(context);
                return;
            }

            var log = new LogRequestEntity(DateTime.UtcNow, context.TraceIdentifier)
            {
                RequestMethod = context.Request.Method,
                RequestUrl = $"{context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString}",
                IpAddress = context.Connection?.RemoteIpAddress?.ToString()
            };

            if (context.Request.ContentType?.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase) != true || context.Request.Method == HttpMethods.Delete)
            {
                var responseBody = new MemoryStream();

                try
                {
                    var originalBodyStream = context.Response.Body;
                    context.Response.Body = responseBody;

                    try
                    {
                        await next.Invoke(context);
                    }
                    catch (Exception ex)
                    {
                        var correlationId = context.TraceIdentifier;

                        logger.LogError(ex, $"Exception {correlationId}: {ex.Message}");

                        var result = JsonConvert.SerializeObject(new OperationResponse { Message = "System error occurred. Please contact support", Status = StatusEnum.Error, CorrelationId = correlationId });

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(result);
                    }

                    log.ResponseStatus = context.Response.StatusCode.ToString();

                    try
                    {
                        log.ResponseBody = await FormatResponse(context.Response);

                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Cannot log response", ex);
                    }
                }
                finally
                {
                    responseBody.Dispose();
                }
            }
            else
            {
                var injectedRequestStream = new MemoryStream();
                var responseBody = new MemoryStream();

                try
                {
                    string requestLog = null;

                    if (!IsCreditCardRequest(context.Request.Method, context.Request.Path))
                    {
                        try
                        {
                            using (var bodyReader = new StreamReader(context.Request.Body))
                            {
                                var bodyAsText = await bodyReader.ReadToEndAsync();
                                if (string.IsNullOrWhiteSpace(bodyAsText) == false)
                                {
                                    requestLog = bodyAsText;
                                }

                                var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);
                                await injectedRequestStream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
                                injectedRequestStream.Seek(0, SeekOrigin.Begin);
                                context.Request.Body = injectedRequestStream;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning("Cannot log request", ex);
                        }
                    }

                    log.RequestBody = requestLog;

                    var originalBodyStream = context.Response.Body;
                    context.Response.Body = responseBody;

                    try
                    {
                        await next.Invoke(context);
                    }
                    catch (Exception ex)
                    {
                        var correlationId = context.TraceIdentifier;

                        logger.LogError(ex, $"Exception {correlationId}: {ex.Message}");

                        var result = JsonConvert.SerializeObject(new OperationResponse { Message = "System error occurred. Please contact support", Status = StatusEnum.Error, CorrelationId = correlationId });

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(result);
                    }

                    log.ResponseStatus = context.Response.StatusCode.ToString();

                    try
                    {
                        log.ResponseBody = await FormatResponse(context.Response);

                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Cannot log response", ex);
                    }
                }
                finally
                {
                    injectedRequestStream.Dispose();
                    responseBody.Dispose();
                }
            }

            try
            {
                await requestLogStorageService.Save(log);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Cannot log request", ex);
            }
        }

        // TODO:
        private bool IsCreditCardRequest(string requestMethod, string requestUrl)
        {
            return requestUrl?.StartsWith("/api/merchant/", StringComparison.InvariantCultureIgnoreCase) == true && requestUrl?.EndsWith("/addCreditCard", StringComparison.InvariantCultureIgnoreCase) == true && requestMethod == HttpMethods.Post;
        }

        private async Task<string> FormatRequest(Stream request)
        {
            request.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(request).ReadToEndAsync();
            request.Seek(0, SeekOrigin.Begin);

            return text;
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }
    }
}

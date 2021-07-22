using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Extensions
{
    /// <summary>
    /// Adds support for query string "Bearer" authentication.
    /// Only works with /hubs routes.
    /// </summary>
    public class QueryJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string name, JwtBearerOptions options)
        {
            // save the original OnMessageReceived event
            var originalOnMessageReceived = options.Events.OnMessageReceived;

            options.Events.OnMessageReceived = async context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    // attempt to read the access token from the query string
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                        context.HttpContext.Request.Headers.Add("Authorization", $"Bearer {accessToken}");
                    }
                }

                // call the original OnMessageReceived event
                await originalOnMessageReceived(context);
            };
        }
    }
}

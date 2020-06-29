using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public static class DatabaseLoggerExtensions
    {
        public static ILoggingBuilder AddDatabase(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerDatabaseProvider>());

            //LoggerProviderOptions.RegisterProviderOptions<ConsoleLoggerOptions, LoggerDatabaseProvider>(builder.Services);
            return builder;
        }
    }
}

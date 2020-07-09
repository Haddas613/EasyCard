using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace Shared.Api.Logging
{
    public class LoggerDatabaseProvider : ILoggerProvider
    {
        private readonly string connectionString;
        private readonly string apiName;
        private readonly IHttpContextAccessor httpContextAccessor;

        public LoggerDatabaseProvider(string connectionString, IHttpContextAccessor httpContextAccessor, string apiName)
        {
            this.connectionString = connectionString;
            this.httpContextAccessor = httpContextAccessor;
            this.apiName = apiName;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName, connectionString, httpContextAccessor, apiName);
        }

        public void Dispose()
        {
        }

        public class Logger : ILogger
        {
            private readonly string categoryName;
            private readonly string connectionString;
            private readonly IHttpContextAccessor httpContextAccessor;
            private readonly string apiName;

            public Logger(string categoryName, string connectionString, IHttpContextAccessor httpContextAccessor, string apiName)
            {
                this.connectionString = connectionString;
                this.categoryName = categoryName;
                this.httpContextAccessor = httpContextAccessor;
                this.apiName = apiName;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            // TODO: filter log level
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                RecordMsg(logLevel, eventId, state, exception, formatter);
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new NoopDisposable();
            }

            private void RecordMsg<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var log = new DatabaseLogEntry
                {
                    ID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                    LogLevel = logLevel.ToString(),
                    CategoryName = categoryName,
                    Message = formatter(state, exception),
                    UserName = httpContextAccessor.HttpContext?.User?.GetDoneBy(),
                    UserID = httpContextAccessor.HttpContext?.User?.GetDoneByID(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationID = httpContextAccessor.HttpContext?.TraceIdentifier,
                    Exception = FormatException(exception),
                    IP = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    ApiName = apiName,
                    Host = httpContextAccessor.HttpContext?.Request?.Host.ToString(),
                    Url = httpContextAccessor.HttpContext?.Request?.GetDisplayUrl(),
                    MachineName = Environment.MachineName
                };

                string sql = "INSERT INTO [dbo].[SystemLog] ([ID], [LogLevel], [CategoryName], [Message], [UserName], [UserID], [IP], [Timestamp], [CorrelationID], [Exception], [ApiName], [Host], [Url], [MachineName]) Values (@ID, @LogLevel, @CategoryName, @Message, @UserName, @UserID, @IP, @Timestamp, @CorrelationID, @Exception, @ApiName, @Host, @Url, @MachineName);";

                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var affectedRows = connection.Execute(sql, log);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot write log message to database: {ex.Message}");
                }
            }

            private string FormatException(Exception ex)
            {
                if (ex == null)
                {
                    return null;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(ex.GetType().ToString());
                sb.AppendLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Inner exception: {ex.InnerException.Message} ({ex.InnerException.GetType().ToString()})");
                    sb.AppendLine(ex.InnerException.StackTrace);
                }

                return sb.ToString();
            }

            private class NoopDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}

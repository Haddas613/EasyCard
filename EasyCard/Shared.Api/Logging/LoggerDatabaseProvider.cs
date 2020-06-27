using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Shared.Api.Logging
{
    public class LoggerDatabaseProvider : ILoggerProvider
    {
        private readonly string connectionString;

        public LoggerDatabaseProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName, connectionString);
        }

        public void Dispose()
        {
        }

        public class Logger : ILogger
        {
            private readonly string categoryName;
            private readonly string connectionString;

            public Logger(string categoryName, string connectionString)
            {
                this.connectionString = connectionString;
                this.categoryName = categoryName;
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
                var log = new
                {
                    ID = Guid.NewGuid(), // TODO: value from event properties
                    LogLevel = logLevel.ToString(),
                    CategoryName = categoryName,
                    Message = formatter(state, exception),
                    User = "username",
                    Timestamp = DateTime.UtcNow
                };

                string sql = "INSERT INTO SystemLog (ID, LogLevel, CategoryName, Message, [User], [Timestamp]) Values (@ID, @LogLevel, @CategoryName, @Message, @User, @Timestamp);";

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

            private class NoopDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}

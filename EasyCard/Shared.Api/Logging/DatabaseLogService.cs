using Dapper;
using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Shared.Api.Logging
{
    public class DatabaseLogService : IDatabaseLogService
    {
        private readonly string connectionString;

        public DatabaseLogService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<SummariesResponse<DatabaseLogEntry>> GetLogEntries(DatabaseLogQuery query)
        {
            int take = query.Take > 0 ? Math.Min(100, query.Take.Value) : 100;
            int skip = query.Skip < 0 ? 0 : query.Skip.GetValueOrDefault();

            var builder = new SqlBuilder();

            var selector = builder.AddTemplate("SELECT [ID], [LogLevel], [CategoryName], [Message], [UserName], [UserID], [IP], [Timestamp], [CorrelationID], [Exception], [ApiName], [Host], [Url], [MachineName] FROM [dbo].[SystemLog] WITH(NOLOCK) /**where**/ /**orderby**/ OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", new { take, skip });

            //var count = builder.AddTemplate("SELECT COUNT (*) FROM [dbo].[SystemLog] WITH(NOLOCK) /**where**/");

            builder.OrderBy($"[{GetSortBy(query)}] {(query.SortDesc.GetValueOrDefault(true) ? "DESC" : "ASC")}");

            // filters
            if (query.LogLevel != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.LogLevel)} = @{nameof(query.LogLevel)}", new { LogLevel = query.LogLevel.ToString() });
            }

            if (!string.IsNullOrWhiteSpace(query.CorrelationID))
            {
                builder.Where($"{nameof(DatabaseLogEntry.CorrelationID)} = @{nameof(query.CorrelationID)}", new { query.CorrelationID });
            }

            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                builder.Where($"{nameof(DatabaseLogEntry.CategoryName)} = @{nameof(query.CorrelationID)}", new { query.CategoryName });
            }

            if (query.UserID != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.UserID)} = @{nameof(query.UserID)}", new { query.UserID });
            }

            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                builder.Where($"{nameof(DatabaseLogEntry.UserName)} = @{nameof(query.UserName)}", new { query.UserName });
            }

            if (!string.IsNullOrWhiteSpace(query.IP))
            {
                builder.Where($"{nameof(DatabaseLogEntry.IP)} = @{nameof(query.IP)}", new { query.IP });
            }

            if (query.FromID != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.ID)} > @{nameof(query.FromID)}", new { query.FromID });
            }

            if (query.ToID != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.ID)} <= @{nameof(query.ToID)}", new { query.ToID });
            }

            if (query.From != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.Timestamp)} >= @{nameof(query.From)}", new { query.From });
            }

            if (query.To != null)
            {
                builder.Where($"{nameof(DatabaseLogEntry.Timestamp)} < @{nameof(query.To)}", new { To = query.To.Value.Date.AddDays(1) });
            }

            if (!string.IsNullOrWhiteSpace(query.Message))
            {
                builder.Where($"{nameof(DatabaseLogEntry.Message)} LIKE @{nameof(query.Message)}", new { Message = query.Message.Replace("*", "%") });
            }

            if (!string.IsNullOrWhiteSpace(query.Exception))
            {
                builder.Where($"{nameof(DatabaseLogEntry.Exception)} LIKE @{nameof(query.Exception)}", new { Exception = query.Exception.Replace("*", "%") });
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var response = new SummariesResponse<DatabaseLogEntry>
                {
                    //NumberOfRecords = await connection.QuerySingleOrDefaultAsync<int>(count.RawSql, count.Parameters),
                    Data = await connection.QueryAsync<DatabaseLogEntry>(selector.RawSql, selector.Parameters)
                };

                return response;
            }
        }

        private string GetSortBy(DatabaseLogQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var allProps = typeof(DatabaseLogEntry).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(d => d.Name).ToList();
                var existingProp = allProps.FirstOrDefault(d => d.Equals(query.SortBy, StringComparison.InvariantCultureIgnoreCase));
                if (existingProp == null)
                {
                    throw new ArgumentException("Invalid SortBy property");
                }

                return existingProp;
            }
            else
            {
                return "ID";
            }
        }
    }
}

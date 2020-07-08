using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Shared.Api.Logging
{
    public class DatabaseLogService
    {
        private readonly string connectionString;

        public DatabaseLogService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<DatabaseLogEntry> GetLogEntries(DatabaseLogQuery query)
        {
            string sql = "SELECT [ID], [LogLevel], [CategoryName], [Message], [UserName], [UserID], [IP], [Timestamp], [CorrelationID], [Exception] FROM [dbo].[SystemLog] WITH(NOLOCK) OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            using (var connection = new SqlConnection(connectionString))
            {
                return connection.Query<DatabaseLogEntry>(sql, new { query.Take, query.Skip });
            }
        }
    }
}

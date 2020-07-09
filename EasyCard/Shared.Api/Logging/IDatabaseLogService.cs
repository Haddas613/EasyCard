using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api.Logging
{
    public interface IDatabaseLogService
    {
        Task<SummariesResponse<DatabaseLogEntry>> GetLogEntries(DatabaseLogQuery query);
    }
}

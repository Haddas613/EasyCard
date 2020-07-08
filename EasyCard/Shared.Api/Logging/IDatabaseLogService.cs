using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public interface IDatabaseLogService
    {
        IEnumerable<DatabaseLogEntry> GetLogEntries(DatabaseLogQuery query);
    }
}

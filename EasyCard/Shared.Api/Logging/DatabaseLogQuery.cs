using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public class DatabaseLogQuery : FilterBase
    {
        public Guid? FromID { get; set; }

        public Guid? ToID { get; set; }

        public Microsoft.Extensions.Logging.LogLevel? LogLevel { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string CorrelationID { get; set; }

        public string Exception { get; set; }

        public string IP { get; set; }

        public string CategoryName { get; set; }

        public string Message { get; set; }

        public string UserName { get; set; }

        public string UserID { get; set; }
    }
}

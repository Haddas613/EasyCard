using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public class DatabaseLogEntry
    {
        public Guid? ID { get; set; }

        public string LogLevel { get; set; }

        public string CategoryName { get; set; }

        /// <summary>
        /// Use wildcard
        /// </summary>
        public string Message { get; set; }

        public string UserName { get; set; }

        public Guid? UserID { get; set; }

        public DateTime? Timestamp { get; set; }

        public string CorrelationID { get; set; }

        public string Exception { get; set; }

        public string IP { get; set; }

        public string ApiName { get; set; }

        public string Host { get; set; }

        public string Url { get; set; }

        public string MachineName { get; set; }
    }
}

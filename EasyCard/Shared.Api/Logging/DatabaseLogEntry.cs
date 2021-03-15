using Shared.Api.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Logging
{
    public class DatabaseLogEntry
    {
        [MetadataOptions(Hidden = true)]
        public Guid? ID { get; set; }

        public string LogLevel { get; set; }

        public string CategoryName { get; set; }

        /// <summary>
        /// Use wildcard
        /// </summary>
        public string Message { get; set; }

        public string UserName { get; set; }

        [MetadataOptions(Hidden = true)]
        public Guid? UserID { get; set; }

        public DateTime? Timestamp { get; set; }

        public string CorrelationID { get; set; }

        [MetadataOptions(Hidden = true)]
        public string Exception { get; set; }

        public string IP { get; set; }

        public string ApiName { get; set; }

        [MetadataOptions(Hidden = true)]
        public string Host { get; set; }

        [MetadataOptions(Hidden = true)]
        public string Url { get; set; }

        [MetadataOptions(Hidden = true)]
        public string MachineName { get; set; }
    }
}

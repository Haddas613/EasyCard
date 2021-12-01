using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api
{
    public class LogRequestEntity : ITableEntity
    {
        public LogRequestEntity()
        {
        }

        public LogRequestEntity(DateTime requestDate, string correlationId)
        {
            this.RowKey = correlationId;
            this.RequestDate = requestDate;
            this.PartitionKey = this.RequestDate.ToString("yy-MM-dd");
        }

        public DateTime RequestDate { get; set; }

        public string RequestUrl { get; set; }

        public string RequestMethod { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        public string ResponseStatus { get; set; }

        public string Headers { get; set; }

        public string UserID { get; set; }

        public string IpAddress { get; set; }

        //[IgnoreProperty]
        public string CorrelationId
        {
            get { return this.RowKey; } set { this.RowKey = value; }
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

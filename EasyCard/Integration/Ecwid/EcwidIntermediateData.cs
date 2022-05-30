using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid
{
    public class EcwidIntermediateData : ITableEntity
    {
        public string Request { get; set; }

        private readonly string partitionKey = DateTime.UtcNow.ToString("yy-MM-dd");

        public EcwidIntermediateData()
        {
        }

        public EcwidIntermediateData(string request, string correlationId)
        {
            this.PartitionKey = partitionKey;
            this.CorrelationId = correlationId;
            this.Request = request;
        }

        public string CorrelationId
        {
            get => this.RowKey;

            set
            {
                this.RowKey = value;
            }
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

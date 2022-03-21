using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class ThreeDSIntermediateData : ITableEntity
    {
        public string AuthenticationValue { get; set; }

        public string Eci { get; set; }

        private readonly string partitionKey = "1";

        public ThreeDSIntermediateData()
        {
        }

        public ThreeDSIntermediateData(string threeDSServerTransID, string authenticationValue, string eci)
        {
            this.PartitionKey = partitionKey;
            this.ThreeDSServerTransID = threeDSServerTransID;
            this.AuthenticationValue = authenticationValue;
            this.Eci = eci;
        }

        public string ThreeDSServerTransID
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

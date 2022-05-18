using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ThreeDSIntermediateData : ITableEntity
    {
        public string AuthenticationValue { get; set; }

        public string Eci { get; set; }

        public string Xid { get; set; }

        public string TransStatus { get; set; } = "Y";

        public string Request { get; set; }

        public Guid? ThreeDSChallengeID { get; set; }

        private readonly string partitionKey = "1";

        public ThreeDSIntermediateData()
        {
        }

        public ThreeDSIntermediateData(string threeDSServerTransID, string authenticationValue, string eci, string xid, Guid? threeDSChallengeID)
        {
            this.PartitionKey = partitionKey;
            this.ThreeDSServerTransID = threeDSServerTransID;
            this.AuthenticationValue = authenticationValue;
            this.Eci = eci;
            this.Xid = xid;
            this.ThreeDSChallengeID = threeDSChallengeID;
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

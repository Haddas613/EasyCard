using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration
{
    public class IntegrationMessage : ITableEntity
    {
        public IntegrationMessage()
        {
        }

        public IntegrationMessage(DateTime messageDate, string entityID, string messageId, string correlationId)
        {
            this.messageDate = messageDate;
            this.correlationId = correlationId;
            this.EntityID = entityID;
            SetPartitionKey();

            this.RowKey = messageId;
        }

        private DateTime messageDate;

        public DateTime MessageDate
        {
            get => messageDate;

            set
            {
                messageDate = value;
                SetPartitionKey();
            }
        }

        public string Method { get; set; }

        public string EntityID { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }

        public string Address { get; set; }

        public string ResponseStatus { get; set; }

        private string correlationId;

        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string CorrelationId
        {
            get => correlationId;

            set
            {
                correlationId = value;
                SetPartitionKey();
            }
        }

        public string MessageId
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Action { get; set; }

        private void SetPartitionKey()
        {
            this.PartitionKey = $"{this.EntityID}";
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

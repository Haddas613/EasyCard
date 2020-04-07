using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration
{
    public class IntegrationMessage : TableEntity
    {
        public IntegrationMessage(DateTime messageDate, string messageId, string correlationId)
        {
            this.MessageId = messageId;
            this.MessageDate = messageDate;
            this.CorrelationId = correlationId;

            this.RowKey = this.MessageId;
            this.PartitionKey = $"{this.MessageDate.ToString("yy-MM-dd")}-{this.CorrelationId}";
        }

        private DateTime messageDate;

        public DateTime MessageDate
        {
            get => messageDate;

            set
            {
                messageDate = value;
                this.PartitionKey = $"{this.MessageDate.ToString("yy-MM-dd")}-{this.CorrelationId}";
            }
        }

        public string Request { get; set; }

        public string Response { get; set; }

        public string Address { get; set; }

        public string ResponseStatus { get; set; }

        public string CorrelationId { get; set; }

        public long? MerchantID { get; set; }

        public string MessageId { get; set; }

        public string Action { get; set; }
    }
}

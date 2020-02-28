using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration
{
    public class IntegrationMessage : TableEntity
    {
        public IntegrationMessage()
        {
            this.messageDate = DateTime.UtcNow;
        }

        public IntegrationMessage(DateTime messageDate, string messageId)
        {
            this.RowKey = messageId;
            this.MessageDate = messageDate;
            this.PartitionKey = this.MessageDate.ToString("yy-MM-dd");
        }

        private DateTime messageDate;

        public DateTime MessageDate
        {
            get => messageDate;

            set
            {
                messageDate = value;
                this.PartitionKey = this.messageDate.ToString("yy-MM-dd");
            }
        }

        public string Request { get; set; }

        public string Response { get; set; }

        public string Address { get; set; }

        public string ResponseStatus { get; set; }

        public string CorrelationId { get; set; }

        public long? MerchantID { get; set; }

        [IgnoreProperty]
        public string MessageId
        {
            get { return this.RowKey; } set { this.RowKey = value; }
        }
    }
}

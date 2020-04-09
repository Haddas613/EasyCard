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
            this.messageDate = messageDate;
            this.correlationId = correlationId;
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

        public string Request { get; set; }

        public string Response { get; set; }

        public string Address { get; set; }

        public string ResponseStatus { get; set; }

        private string correlationId;

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
            get { return RowKey; } set { RowKey = value; }
        }

        public string Action { get; set; }

        private void SetPartitionKey()
        {
            this.PartitionKey = $"{this.messageDate.ToString("yy-MM-dd")}-{this.correlationId}";
        }
    }
}

﻿using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Sms
{
    public class SmsMessage : ITableEntity
    {
        public SmsMessage()
        {
        }

        public SmsMessage(DateTime messageDate, string messageId)
        {
            this.RowKey = messageId;
            this.messageDate = messageDate;
            this.PartitionKey = this.messageDate.ToString("yy-MM-dd");
        }

        private DateTime messageDate = DateTime.UtcNow;

        public DateTime MessageDate
        {
            get
            {
                return this.messageDate;
            }

            set
            {
                this.messageDate = value;
                this.PartitionKey = this.messageDate.ToString("yy-MM-dd");
            }
        }

        public string Body { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public Guid? MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        //[IgnoreProperty]
        public string MessageId { get => RowKey; set => RowKey = value; }

        public string CorrelationId { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

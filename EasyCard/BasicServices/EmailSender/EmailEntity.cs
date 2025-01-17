﻿using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Shared.Helpers.Email;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicServices
{
    public class EmailEntity : ITableEntity
    {
        public EmailEntity()
        {

        }

        public EmailEntity(EmailQueueMessage emailQueue, Email email)
        {
            this.RowKey = emailQueue.MessageID.ToString();
            this.PartitionKey = emailQueue.MessageDate.ToString("yy-MM-dd");

            this.Subject = email.Subject;
            this.TemplateCode = email.TemplateCode;
            this.EmailTo = email.EmailTo;
            this.Substitutions = JsonConvert.SerializeObject(email.Substitutions);
            this.Attachments = JsonConvert.SerializeObject(email.Attachments);
        }

        public string Subject { get; set; }

        public string TemplateCode { get; set; }

        public string EmailTo { get; set; }

        public string Substitutions { get; set; }

        public string Attachments { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

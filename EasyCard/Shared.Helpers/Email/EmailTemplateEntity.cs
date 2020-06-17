using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Email
{
    /// <summary>
    /// Represents email templates in azure table storage
    /// </summary>
    public class EmailTemplateEntity : TableEntity
    {
        public static string DefaultPartitionKey
        {
            get { return "1"; } // TODO: partition key
        }

        public EmailTemplateEntity()
        {
        }

        public EmailTemplateEntity(string templateCode)
        {
            this.RowKey = templateCode;
            this.PartitionKey = DefaultPartitionKey;
        }

        public string SubjectTemplate { get; set; }

        public string BodyTemplate { get; set; }

        [IgnoreProperty]
        public string TemplateCode
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }
    }
}

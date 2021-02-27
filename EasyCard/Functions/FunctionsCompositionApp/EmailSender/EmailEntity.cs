using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Shared.Helpers.Email;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailSender
{
    public class EmailEntity : TableEntity
    {
        public EmailEntity()
        {
        }

        public string Subject { get; set; }

        public string TemplateCode { get; set; }

        public string EmailTo { get; set; }

        public string Substitutions { get; set; }

        public string Attachments { get; set; }

        public DateTime? SentDateTime { get; set; }
    }
}

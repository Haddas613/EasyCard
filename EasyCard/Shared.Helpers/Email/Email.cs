using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers.Email
{
    public class Email
    {
        public string Subject { get; set; }

        public string TemplateCode { get; set; }

        public string EmailTo { get; set; }

        public EmailSubstitution[] Substitutions { get; set; }

        public string Attachment { get; set; }
    }

    public class EmailSubstitution
    {
        public string Substitution { get; set; }

        public string Value { get; set; }
    }
}

using Shared.Helpers.Templating;
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

        public TextSubstitution[] Substitutions { get; set; }

        public string[] Attachments { get; set; }
    }
}

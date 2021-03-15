using Merchants.Api.Models.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.Shva
{
    //TODO: Model validation
    public class NewPasswordRequest
    {
        public Guid? TerminalID { get; set; }

        public Guid? TerminalTemplateID { get; set; }

        public ExternalSystemRequest ExternalSystem { get; set; }
    }
}

using Merchants.Api.Models.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.Shva
{
    public class ChangePasswordRequest
    {
        public Guid? TerminalID { get; set; }

        public long? TerminalTemplateID { get; set; }

        public string NewPassword { get; set; }
    }
}

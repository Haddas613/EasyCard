using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.User
{
    public class LinkUserToTerminalRequest
    {
        [Required]
        public Guid TerminalID { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}

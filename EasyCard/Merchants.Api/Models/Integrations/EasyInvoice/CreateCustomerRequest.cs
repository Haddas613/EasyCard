using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Integrations.EasyInvoice
{
    public class CreateCustomerRequest
    {
        [Required]
        public Guid TerminalID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(64, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(64, MinimumLength = 6)]
        public string Password { get; set; }
    }
}

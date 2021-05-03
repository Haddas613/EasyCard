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
        [EmailAddress]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(64, MinimumLength = 6)]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(10, MinimumLength = 6)]
        public string BusinessID { get; set; }
    }
}

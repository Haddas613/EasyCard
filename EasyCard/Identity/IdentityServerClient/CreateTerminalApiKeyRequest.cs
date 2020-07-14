using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerClient
{
    public class CreateTerminalApiKeyRequest
    {
        [Required]
        public Guid TerminalID { get; set; }

        [Required]
        public Guid MerchantID { get; set; }
    }
}
